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
// Created On:   2019/05/08 19:57
// Modified On:  2019/08/09 11:12
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SuperMemo.Common.Extensions;
using SuperMemoAssistant.SuperMemo.Common.Registry;
using SuperMemoAssistant.SuperMemo.Common.Registry.Files;
using SuperMemoAssistant.SuperMemo.Common.Registry.Models;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;
using SuperMemoAssistant.Sys.SparseClusteredArray;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Registry
{
  public class RegistryUpdater17<TMember, IMember>
    : IRegistryUpdater
    where TMember : RegistryMemberBase, IRegistryMember, IMember
  {
    #region Properties & Fields - Non-Public

    private readonly RegistryBase<TMember, IMember> _registry;
    private readonly Action<TMember>                _onMemberCreatedOrUpdated;

    #endregion




    #region Constructors

    public RegistryUpdater17(
      RegistryBase<TMember, IMember> registry,
      Action<TMember>                onMemberCreatedOrUpdated)
    {
      _registry                 = registry;
      _onMemberCreatedOrUpdated = onMemberCreatedOrUpdated;
    }

    #endregion




    #region Methods Impl

    public void CommitFromFiles(SMCollection            collection,
                                IRegistryFileDescriptor registryFileDesc)
    {
      var memFilePath = collection.GetMemFilePath(registryFileDesc);
      var rtxFilePath = collection.GetRtxFilePath(registryFileDesc);

      var memExists = File.Exists(memFilePath);
      var rtxExists = File.Exists(rtxFilePath);

      switch (registryFileDesc.IsOptional)
      {
        case true when rtxExists == false || memExists == false:
          return;

        case false when memExists == false:
          throw new InvalidOperationException($"({GetType().Name}) Failed to load registry file: no such file {memFilePath}");

        case false when rtxExists == false:
          throw new InvalidOperationException($"({GetType().Name}) Failed to load registry file: no such file {rtxFilePath}");
      }

      using (Stream memStream = File.OpenRead(memFilePath))
      using (Stream rtxStream = File.OpenRead(rtxFilePath))
        //using (Stream rtfStream = File.OpenRead(rtfFilePath))
      {
        Dictionary<int, RegMemElem17> memElems = memStream.StreamToStruct<RegMemElem17, RegMemElem17>(
          RegMemElem17.SizeOfMemElem,
          e => e
        );

        foreach (var id in memElems.Keys.OrderBy(id => id))
        {
          var memElem = memElems[id];
          var rtxElem = ParseRtStream(rtxStream, memElem);

          Commit(id, memElem, rtxElem);
        }
      }
    }

    public void CommitFromMemory(
      IRegistryFileDescriptor    registryFileDesc,
      SparseClusteredArray<byte> memSCA,
      SparseClusteredArray<byte> rtxSCA)
    {
      memSCA.Lock.EnterUpgradeableReadLock();
      rtxSCA.Lock.EnterUpgradeableReadLock();

      try
      {
        foreach (var memStream in memSCA.GetStreams())
        {
          var memElems = memStream.StreamToStruct<RegMemElem17, RegMemElem17>(
            RegMemElem17.SizeOfMemElem,
            e => e
          );

          foreach (var memElemKeyValue in memElems.OrderBy(kv => kv.Value.rtxOffset))
          {
            var id      = memElemKeyValue.Key;
            var memElem = memElemKeyValue.Value;

            var lower = memElem.rtxOffset - 1;
            var upper = memElem.rtxOffset + memElem.rtxLength - 2;

            if (upper < lower || upper < 0)
            {
              LogTo.Warning($"({registryFileDesc.RegistryName}) Invalid rtx offsets, upper: {upper}, lower: {lower}");
              continue;
            }

            SparseClusteredArray<byte>.Bounds rtxBounds = new SparseClusteredArray<byte>.Bounds(
              lower,
              upper
            );

            using (var rtStream = rtxSCA.GetSubsetStream(rtxBounds))
            {
              RegRtElem17 rtxElem = default;

              if (rtStream != null)
                rtxElem = ParseRtStream(rtStream,
                                        memElem);

              Commit(id, memElem, rtxElem);
            }
          }
        }

        memSCA.Clear();
        rtxSCA.Clear();
      }
      finally
      {
        memSCA.Lock.ExitUpgradeableReadLock();
        rtxSCA.Lock.ExitUpgradeableReadLock();
      }
    }

    #endregion




    #region Methods

    public virtual void SetupMember(RegistryMemberBase member, RegMemElem17 mem, RegRtElem17 rt)
    {
      member.UseCount = mem.useCount;

      member.LinkType = (RegistryLinkType)mem.linkType;

      member.RtxId     = rt.no;
      member.RtxOffset = mem.rtxOffset;
      member.RtxLength = mem.rtxLength;

      if (rt.value != null)
        member.RtxValue = Encoding.UTF8.GetString(rt.value);

      member.SlotIdOrOffset             = mem.slotIdOrOffset;
      member.SlotLengthOrConceptGroupId = mem.slotLengthOrConceptGroup;

      member.Empty = member.RtxValue == null;
    }

    public virtual void UpdateMember(RegistryMemberBase member, RegMemElem17 mem, RegRtElem17 rt)
    {
#if DEBUG && !DEBUG_IN_PROD
      LogTo.Debug("[{0} {1}] Updating \"{member.Name}\"",
                  member.GetType().Name,
                  member.Id);
#endif

      if (mem.rtxOffset == 0)
      {
        member.Empty = true;

        return;
      }

      member.Empty = false;

      member.UseCount = mem.useCount;

      member.LinkType = (RegistryLinkType)mem.linkType;

      member.RtxOffset = mem.rtxOffset;
      member.RtxLength = mem.rtxLength;

      if (rt != null)
      {
        member.RtxId    = rt.no;
        member.RtxValue = Encoding.UTF8.GetString(rt.value);
      }

      member.SlotIdOrOffset             = mem.slotIdOrOffset;
      member.SlotLengthOrConceptGroupId = mem.slotLengthOrConceptGroup;
    }

    protected void Commit(int          id,
                          RegMemElem17 mem,
                          RegRtElem17  rtxOrRtf)
    {
      var member = _registry.Get(id);

      try
      {
        if (member == null)
        {
          member = _registry.CreateInternal(id);

          SetupMember(member, mem, rtxOrRtf);
        }

        else
        {
          UpdateMember(member, mem, rtxOrRtf);
        }
      }
      finally
      {
        _onMemberCreatedOrUpdated(member);
      }
    }

    private RegRtElem17 ParseRtStream(Stream       rtxStream,
                                      RegMemElem17 mem)
    {
      if (mem.rtxOffset == 0)
        return new RegRtElem17
        {
          value   = null,
          no      = 0,
          unknown = 2,
        };

      using (BinaryReader binStream = new BinaryReader(rtxStream,
                                                       Encoding.Default,
                                                       true))
      {
        rtxStream.Seek(mem.rtxOffset - 1,
                       SeekOrigin.Begin);

        return new RegRtElem17
        {
          value   = binStream.ReadBytes(mem.rtxLength - sizeof(int) - sizeof(byte)),
          no      = binStream.ReadInt32(),
          unknown = binStream.ReadByte()
        };
      }
    }

    #endregion
  }
}
