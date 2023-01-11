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

#endregion




namespace SuperMemoAssistant.SuperMemo.Common
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;
  using System.Threading.Tasks;
  using Anotar.Serilog;
  using Hooks;
  using Interop.SuperMemo;
  using Interop.SuperMemo.Core;
  using PluginManager.Interop.Sys;
  using Process.NET;
  using Process.NET.Memory;
  using SMA;
  using SMA.Hooks;
  using SuperMemoAssistant.Extensions;

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
      Hook      = new SMHookEngine();
    }

    #endregion




    #region Properties & Fields - Public

    public new SuperMemoRegistryCore Registry => _registry;
    public new SuperMemoUICore       UI       => _ui;

    #endregion
  }

  /// <summary>Convenience class that implements shared code</summary>
  [SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
                   Justification = "_registry and _ui need to be protected instance fields for SuperMemoCore to compile")]
  [SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
  public abstract class SuperMemoBase
    : PerpetualMarshalByRefObject,
      IDisposable,
      ISuperMemo
  {
    #region Properties & Fields - Non-Public

    private readonly string _binPath;

    private IPointer _ignoreUserConfirmationPtr;

    protected SuperMemoRegistryCore _registry;
    protected SuperMemoUICore       _ui;
    protected SMHookEngine          Hook { get; set; }

    #endregion




    #region Constructors

    protected SuperMemoBase(SMCollection collection,
                            string       binPath)
    {
      Collection = collection;
      _binPath   = binPath;
    }

    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "<Pending>")]
    public virtual void Dispose()
    {
      _ignoreUserConfirmationPtr = null;

      if (SMProcess?.Native != null)
        SMProcess.Native.Exited -= OnSMExited;

      try
      {
        Hook.CleanupHooks();
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
        LogTo.Error(ex, "An exception occurred in one of OnSMStoppedEvent handlers");
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

    public async Task StartAsync(NativeData nativeData)
    {
      AppVersion = nativeData.SMVersion;

      var smProcess = await Hook.CreateAndHookAsync(
        Collection,
        _binPath,
        GetIOCallbacks(),
        nativeData
      ).ConfigureAwait(false);

      SMProcess               =  smProcess ?? throw new InvalidOperationException("Failed to start SuperMemo process");
      SMProcess.Native.Exited += OnSMExited;

      // TODO: Base OnSMStarted event on a more reliable cue ?
      Core.SM.UI.ElementWdw.OnAvailableInternal += ElementWdw_OnAvailable;

      Core.Natives = smProcess.Procedures;

      _ignoreUserConfirmationPtr = SMProcess[Core.Natives.Globals.IgnoreUserConfirmationPtr];

      await Core.SMA.OnSMStartingAsync().ConfigureAwait(false);

      Hook.SignalWakeUp();
    }

    private void ElementWdw_OnAvailable()
    {
      Core.SMA.OnSMStartedAsync().RunAsync();

      Core.SM.UI.ElementWdw.OnAvailableInternal -= ElementWdw_OnAvailable;
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
        _registry.Comment,
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
