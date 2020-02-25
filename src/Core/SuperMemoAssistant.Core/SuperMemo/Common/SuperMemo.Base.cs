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
// Modified On:  2020/02/22 16:41
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Anotar.Serilog;
using PluginManager.Interop.Sys;
using Process.NET;
using Process.NET.Memory;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.SMA.Hooks;
using SuperMemoAssistant.SuperMemo.Hooks;

namespace SuperMemoAssistant.SuperMemo.Common
{
  public abstract class SuperMemoCore : SuperMemoBase
  {
    #region Constructors

    /// <inheritdoc />
    protected SuperMemoCore(SMCollection collection, string binPath)
      : base(collection, binPath)
    {
      Core.SM = this;

      _registry = new SuperMemoRegistryCore();
      _ui       = new SuperMemoUICore();
      _hook     = new SMHookEngine();
    }

    #endregion




    #region Properties & Fields - Public

    public new SuperMemoRegistryCore Registry => _registry;
    public new SuperMemoUICore       UI       => _ui;

    #endregion
  }

  /// <summary>Convenience class that implements shared code</summary>
  public abstract class SuperMemoBase
    : PerpetualMarshalByRefObject,
      IDisposable,
      ISuperMemo
  {
    #region Properties & Fields - Non-Public

    private readonly string _binPath;

    protected SuperMemoRegistryCore _registry;
    protected SuperMemoUICore       _ui;
    protected SMHookEngine          _hook;

    private IPointer _ignoreUserConfirmationPtr;

    #endregion




    #region Constructors

    protected SuperMemoBase(SMCollection collection,
                            string       binPath)
    {
      Collection = collection;
      _binPath   = binPath;
    }

    public virtual void Dispose()
    {
      _ignoreUserConfirmationPtr = null;

      if (SMProcess?.Native != null)
        SMProcess.Native.Exited -= OnSMExited;

      try
      {
        _hook.CleanupHooks();
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Failed to cleanup SMHookEngine");
      }

      try
      {
        Core.SMA.OnSMStopped();
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "An exception occured in one of OnSMStoppedEvent handlers");
      }
    }

    #endregion




    #region Properties & Fields - Public

    public IProcess SMProcess { get; private set; }

    #endregion




    #region Properties Impl - Public

    public SMCollection Collection { get; }

    public int ProcessId => SMProcess.Native?.Id ?? -1;

    public bool IgnoreUserConfirmation
    {
      get => _ignoreUserConfirmationPtr.Read<bool>();
      set => _ignoreUserConfirmationPtr.Write<bool>(0, value);
    }
    public ISuperMemoRegistry Registry => _registry;
    public ISuperMemoUI       UI       => _ui;

    //
    // ISuperMemo Methods

    public Version AppVersion { get; private set; }

    #endregion




    #region Methods

    //
    // SM-App Lifecycle

    public async Task Start(NativeData nativeData)
    {
      AppVersion = nativeData.SMVersion;

      await OnPreInit();

      var smProcess = await _hook.CreateAndHook(
        Collection,
        _binPath,
        GetIOCallbacks(),
        nativeData
      );

      SMProcess               =  smProcess ?? throw new InvalidOperationException("Failed to start SuperMemo process");
      SMProcess.Native.Exited += OnSMExited;

      Core.Natives = smProcess.Procedures;

      await OnPostInit();

      _hook.SignalWakeUp();
    }

    protected virtual async Task OnPreInit()
    {
      await Core.SMA.OnSMStarting();
    }

    protected virtual async Task OnPostInit()
    {
      _ignoreUserConfirmationPtr = SMProcess[Core.Natives.Globals.IgnoreUserConfirmationPtr];

      await Core.SMA.OnSMStarted();
    }

    protected virtual void OnSMExited(object    called,
                                      EventArgs args)
    {
      Dispose();
    }

    protected virtual IEnumerable<ISMAHookIO> GetIOCallbacks()
    {
      return new ISMAHookIO[]
      {
        _registry.Element,
        _registry.Component,
        _registry.Text,
        _registry.Binary,
        _registry.Concept,
        _registry.Image,
        _registry.Template,
        _registry.Sound,
        _registry.Video
      };
    }

    #endregion
  }
}
