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
// Created On:   2019/02/25 22:02
// Modified On:  2019/02/27 03:06
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Anotar.Serilog;
using AsyncEvent;
using Process.NET;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.SMA.Configs;
using SuperMemoAssistant.SMA.UI;
using SuperMemoAssistant.SuperMemo;
using SuperMemoAssistant.SuperMemo.SuperMemo17;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Content.Layout;
using SuperMemoAssistant.Sys;

namespace SuperMemoAssistant.SMA
{
  /// <summary>
  ///   Wrapper around a SM management instance that handles SuperMemo App lifecycle events
  ///   (start, exit, ...) and provides a safe interface to interact with SuperMemo
  /// </summary>
  public class SMA
    : PerpetualMarshalByRefObject,
      ISuperMemoAssistant, // Proxy for wrapped SMxx object
      IDisposable
  {
    #region Constants & Statics

    protected static readonly Dictionary<Regex, Func<SMCollection, string, SuperMemoBase>>
      SMTitleFactoryMap =
        new Dictionary<Regex, Func<SMCollection, string, SuperMemoBase>>
        {
          {
            new Regex(SM17.RE_WindowTitle,
                      RegexOptions.Compiled | RegexOptions.IgnoreCase),
            (c,
             b) => new SM17(c,
                            b)
          }
        };


    public static SMA Instance { get; } = new SMA();

    #endregion




    #region Properties & Fields - Non-Public

    internal SuperMemoBase SMMgmt { get; set; }

    #endregion




    #region Constructors

    /// <summary>
    ///   Create an instance of the wrapper that will start a SM instance and attach the
    ///   management engine.
    /// </summary>
    protected SMA()
    {
      Svc.SMA = this;

      SMAUI.Initialize();
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
      SMMgmt?.Dispose();
    }

    #endregion




    #region Properties & Fields - Public

    public StartupCfg Config    { get; set; }
    public IProcess   SMProcess => SMMgmt?.SMProcess;

    public System.Diagnostics.Process NativeProcess => SMProcess.Native;

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public SMCollection Collection { get => SMMgmt?.Collection; private set => throw new InvalidOperationException(); }
    /// <inheritdoc />
    public virtual SMAppVersion AppVersion => SMMgmt?.AppVersion ?? SMConst.Versions.vInvalid;
    /// <inheritdoc />
    public bool IgnoreUserConfirmation
    {
      get => SMMgmt?.IgnoreUserConfirmation ?? false;
      set
      {
        if (SMMgmt != null) SMMgmt.IgnoreUserConfirmation = value;
      }
    }
    
    /// <inheritdoc />
    public ISuperMemoRegistry Registry => SuperMemoRegistry.Instance;
    /// <inheritdoc />
    public ISuperMemoUI       UI       => SuperMemoUI.Instance;
    
    /// <inheritdoc />
    public IEnumerable<string> Layouts => LayoutManager.Instance.Layouts.Select(l => l.Name);

    #endregion




    #region Methods

    //
    // Collection loading management

    public async Task<bool> Start(SMCollection collection)
    {
      if (SMMgmt != null)
        return false;

      try
      {
        Config = LoadConfig();

        // TODO: Look at PE version and select Management Engine version
        SMMgmt = new SM17(collection,
                          Config.SMBinPath);

        await SMMgmt.Start();
        // TODO: Ensure opened collection (windows title) matches parameter
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Failed to load SM17.");

        SMMgmt = null;

        try
        {
          await OnSMStoppedEvent.InvokeAsync(this,
                                             new SMProcessArgs(this,
                                                               null));
        }
        catch (Exception pluginEx)
        {
          LogTo.Error(pluginEx,
                      "Exception while notifying plugins OnSMStoppedEvent.");
        }

        // TODO: Handle exception

        return false;
      }

      return true;
    }

    public StartupCfg LoadConfig()
    {
      return Svc.Configuration.Load<StartupCfg>().Result ?? new StartupCfg();
    }

    public Task SaveConfig(bool sync)
    {
      var task = Svc.Configuration.Save<StartupCfg>(Config);

      if (sync)
        task.Wait();

      return task;
    }

    public async Task OnSMStarting()
    {
      try
      {
        await OnSMStartingEvent.InvokeAsync(this,
                                            new SMEventArgs(this));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Error while notifying OnSMStartingEvent");
      }
    }

    public async Task OnSMStarted()
    {
      await SaveConfig(false);

      try
      {
        await OnSMStartedEvent.InvokeAsync(this,
                                           new SMProcessArgs(this,
                                                             SMProcess.Native));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Error while notifying OnSMStartedEvent");
      }
    }

    public async Task OnSMStopped()
    {
      SMMgmt = null;

      try
      {
        await OnSMStoppedEvent.InvokeAsync(this,
                                           null);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Error while notifying plugins OnSMStoppedEvent");
      }
    }

    #endregion




    #region Events

    public virtual event AsyncEventHandler<SMProcessArgs> OnSMStartedEvent;
    public virtual event AsyncEventHandler<SMEventArgs>   OnSMStartingEvent;
    public virtual event AsyncEventHandler<SMProcessArgs> OnSMStoppedEvent;

    #endregion
  }
}
