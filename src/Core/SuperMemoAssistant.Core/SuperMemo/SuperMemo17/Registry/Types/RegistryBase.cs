#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   2018/05/12 18:45
// Modified On:  2018/05/30 22:17
// Modified By:  Alexis

#endregion




using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Types;
using SuperMemoAssistant.SuperMemo.Hooks;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Members;
using SuperMemoAssistant.Sys.Collections;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Types
{
  public abstract class RegistryBase<TMember, IMember>
    : SMHookIOBase,
      IRegistry<IMember>
    where TMember : RegistryMemberBase, IRegistryMember, IMember
  {
    #region Properties & Fields - Non-Public

    protected ConcurrentDictionary<int, TMember> Members { get; set; } = new ConcurrentDictionary<int, TMember>();

    protected SparseClusteredArray<byte> MemSCA { get; } = new SparseClusteredArray<byte>();
    protected SparseClusteredArray<byte> RtxSCA { get; } = new SparseClusteredArray<byte>();


    //
    // Hooks-related

    protected IEnumerable<string> TargetFiles => new[]
    {
      MemFileName,
      RtxFileName,
      //RtfFileName
    };


    //
    // Inheritance

    protected virtual  bool   IsOptional  => false;
    protected abstract string MemFileName { get; }
    protected abstract string RtxFileName { get; }
    protected abstract string RtfFileName { get; }

    #endregion




    #region Properties Impl - Public

    public IMember this[int index] => Members.SafeGet(index);

    public int Count => Members.Count;

    #endregion




    #region Methods Impl

    protected override void Initialize()
    {
      CommitFromFiles();
    }

    protected override void Cleanup()
    {
      Members.Clear();
      MemSCA.Clear();
      RtxSCA.Clear();
    }

    protected override void CommitFromMemory()
    {
      foreach (SegmentStream memStream in MemSCA.GetStreams())
      {
        var memElems = StreamToStruct<RegMemElem>(
          memStream, RegMemElem.SizeOfMemElem
        );

        foreach (var memElem in memElems.OrderBy(kv => kv.Value.rtxOffset))
        {
          SparseClusteredArray<byte>.Bounds rtxBounds = new SparseClusteredArray<byte>.Bounds(
            memElem.Value.rtxOffset - 1,
            memElem.Value.rtxOffset + memElem.Value.rtxLength - 2
          );

          using (SegmentStream rtStream = RtxSCA.GetSubsetStream(rtxBounds))
          {
            RegRtElem rtxElem = default(RegRtElem);

            if (rtStream != null)
              rtxElem = ParseRtStream(rtStream, memElem.Value);

            Commit(memElem.Key, memElem.Value, rtxElem);
          }
        }
      }

      MemSCA.Clear();
      RtxSCA.Clear();
    }

    protected override SparseClusteredArray<byte> GetSCAForFileName(string fileName)
    {
      if (MemFileName.Equals(fileName))
        return MemSCA;

      else if (RtxFileName.Equals(fileName))
        return RtxSCA;

      return null;
    }

    public override IEnumerable<string> GetTargetFilePaths()
    {
      return TargetFiles.Select(f => Collection.GetRegistryFilePath(f));
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc />
    public IEnumerator<IMember> GetEnumerator()
    {
      return Members.Values.ToList().GetEnumerator();
    }

    #endregion




    #region Methods


    //
    // Native format parsing

    protected static RegRtElem ParseRtStream(Stream rtxStream, RegMemElem mem)
    {
      if (mem.rtxOffset == 0)
        return new RegRtElem()
        {
          value   = null,
          no      = 0,
          unknown = 2,
        };

      using (BinaryReader binStream = new BinaryReader(rtxStream, Encoding.Default, true))
      {
        rtxStream.Seek(mem.rtxOffset - 1, SeekOrigin.Begin);

        RegRtElem rtx = new RegRtElem
        {
          value   = binStream.ReadBytes(mem.rtxLength - sizeof(int) - sizeof(byte)),
          no      = binStream.ReadInt32(),
          unknown = binStream.ReadByte()
        };

        return rtx;
      }
    }


    //
    // Lifecycle

    protected void CommitFromFiles()
    {
      var memFilePath = Collection.GetRegistryFilePath(MemFileName);
      var rtxFilePath = Collection.GetRegistryFilePath(RtxFileName);

      if (IsOptional && (File.Exists(memFilePath) == false || File.Exists(rtxFilePath) == false))
        return;

      using (Stream memStream = File.OpenRead(memFilePath))
      using (Stream rtxStream = File.OpenRead(rtxFilePath))
        //using (Stream rtfStream = File.OpenRead(rtfFilePath))
      {
        Dictionary<int, RegMemElem> memElems = StreamToStruct<RegMemElem>(
          memStream, RegMemElem.SizeOfMemElem
        );

        foreach (var memElem in memElems.OrderBy(kv => kv.Value.rtxOffset))
        {
          var rtxElem = ParseRtStream(rtxStream, memElem.Value);

          Commit(memElem.Key, memElem.Value, rtxElem);
        }
      }
    }

    protected virtual void Commit(int id, RegMemElem mem, RegRtElem rtxOrRtf)
    {
      // TODO: Rtf
      var member = Members.SafeGet(id);

      if (member == null)
      {
        Members[id] = Create(id, mem, rtxOrRtf);
        return;
      }

      member.Update(mem, rtxOrRtf);
    }

    #endregion




    #region Methods Abs

    protected abstract TMember Create(int id, RegMemElem mem, RegRtElem rtxOrRtf);

    #endregion
  }
}
