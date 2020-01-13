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
// Created On:   2019/08/07 15:20
// Modified On:  2020/01/12 10:26
// Modified By:  Alexis

#endregion




using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Anotar.Serilog;
using Process.NET.Assembly;
using Process.NET.Types;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Types;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.SuperMemo.Common.Extensions;
using SuperMemoAssistant.SuperMemo.Common.Registry.Files;
using SuperMemoAssistant.SuperMemo.Hooks;
using SuperMemoAssistant.Sys.SparseClusteredArray;

// ReSharper disable VirtualMemberCallInConstructor

namespace SuperMemoAssistant.SuperMemo.Common.Registry
{
  public abstract class RegistryBase<TMember, IMember>
    : SMHookIOBase,
      IRegistry<IMember>
    where TMember : RegistryMemberBase, IRegistryMember, IMember
  {
    #region Properties & Fields - Non-Public

    //
    // Sync

    private readonly ManualResetEventSlim _waitForMemberEvent = new ManualResetEventSlim(true);
    private          int                  _waitForMemberId    = -1;


    //
    // Core

    protected ConcurrentDictionary<int, TMember> Members { get; } = new ConcurrentDictionary<int, TMember>();

    protected SparseClusteredArray<byte> MemSCA { get; } = new SparseClusteredArray<byte>();
    protected SparseClusteredArray<byte> RtxSCA { get; } = new SparseClusteredArray<byte>();


    //
    // Hooks-related

    protected IEnumerable<string> TargetFiles { get; }


    //
    // Inheritance

    protected abstract IRegistryFileDescriptor FileDesc { get; }
    protected abstract IRegistryUpdater        Updater  { get; }

    protected abstract IntPtr RegistryPtr { get; }

    #endregion




    #region Constructors

    /// <inheritdoc />
    protected RegistryBase()
    {
      TargetFiles = new[]
      {
        Collection.GetMemFilePath(FileDesc),
        Collection.GetRtxFilePath(FileDesc),
        //Collection.GetRtfFilePath(FileDesc)
      };
    }

    #endregion




    #region Properties Impl - Public

    public IMember this[int index] => Members.SafeGet(index);

    public int Count => Members.Count;

    #endregion




    #region Methods Impl

    protected override void Initialize()
    {
      if (FileDesc == null)
      {
        LogTo.Error("Registry File Descriptor is NULL");
        throw new NullReferenceException(nameof(FileDesc));
      }

      if (Updater == null)
      {
        LogTo.Error("Registry Updater is NULL");
        throw new NullReferenceException(nameof(Updater));
      }

      base.Initialize();
    }

    protected override void Cleanup()
    {
      Members.Clear();
      MemSCA.Clear();
      RtxSCA.Clear();
    }

    protected override void CommitFromFiles()
    {
      Updater.CommitFromFiles(Collection, FileDesc);
    }

    protected override void CommitFromMemory()
    {
      Updater.CommitFromMemory(FileDesc, MemSCA, RtxSCA);
    }

    protected override SparseClusteredArray<byte> GetSCAForFileName(string fileName)
    {
      if (FileDesc.MemFileName.Equals(fileName, StringComparison.CurrentCultureIgnoreCase))
        return MemSCA;

      if (FileDesc.RtxFileName.Equals(fileName, StringComparison.CurrentCultureIgnoreCase))
        return RtxSCA;

      return null;
    }

    public override IEnumerable<string> GetTargetFilePaths()
    {
      return TargetFiles;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<IMember> GetEnumerator()
    {
      return Members.Values.ToList().GetEnumerator();
    }

    public IEnumerable<IMember> FindByName(Regex regex)
    {
      return Members.Values.Where(m => m.Empty == false && regex.IsMatch(m.Name)).Cast<IMember>().ToList();
    }

    public IMember FirstOrDefaultByName(string exactName)
    {
      return Members.Values.FirstOrDefault(m => m.Empty == false && m.Name == exactName);
    }

    public IMember FirstOrDefaultByName(Regex regex)
    {
      return Members.Values.FirstOrDefault(m => m.Empty == false && regex.IsMatch(m.Name));
    }

    #endregion




    #region Methods

    //
    // Lifecycle

    public TMember Get(int id)
    {
      return Members.SafeGet(id);
    }

    protected virtual void OnMemberAddedOrUpdated(TMember member)
    {
      if (Members.ContainsKey(member.Id) == false)
        Members[member.Id] = member;

      if (member.Id == _waitForMemberId)
        _waitForMemberEvent.Set();
    }

    //
    // SuperMemo natives

    public int AddMember(string textOrPath)
    {
      try
      {
        _waitForMemberEvent.Reset();

        int ret = Core.Natives.Registry.AddMember.Invoke(
          RegistryPtr,
          new DelphiUTF16String(textOrPath));

        if (ret > 0)
        {
          _waitForMemberId = ret;

          if (Members.ContainsKey(ret) == false)
            _waitForMemberEvent.Wait(AssemblyFactory.ExecutionTimeout);
        }

        return ret;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return -1;
      }
      finally
      {
        _waitForMemberId = -1;
      }
    }

    public int ImportFile(string textOrPath,
                          string registryName)
    {
      try
      {
        _waitForMemberEvent.Reset();

        Core.SM.IgnoreUserConfirmation = true;

        int ret = Core.Natives.Registry.ImportFile.Invoke(
          RegistryPtr,
          new DelphiUTF16String(textOrPath),
          new DelphiUTF16String(registryName));

        if (ret > 0)
        {
          _waitForMemberId = ret;

          if (Members.ContainsKey(ret) == false)
            _waitForMemberEvent.Wait(AssemblyFactory.ExecutionTimeout);
        }

        return ret;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return -1;
      }
      finally
      {
        Core.SM.IgnoreUserConfirmation = false;
        _waitForMemberId               = -1;
      }
    }

    #endregion




    #region Methods Abs

    public abstract TMember CreateInternal(int id);

    #endregion
  }
}
