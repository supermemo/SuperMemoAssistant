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
// Created On:   2019/09/03 18:08
// Modified On:  2019/12/13 16:30
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anotar.Serilog;
using AsyncEvent;
using Process.NET;
using Process.NET.Windows;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.SMA.Configs;
using SuperMemoAssistant.SMA.UI;
using SuperMemoAssistant.SuperMemo.Common;
using SuperMemoAssistant.SuperMemo.Common.Content.Layout;
using SuperMemoAssistant.SuperMemo.SuperMemo17;
using SuperMemoAssistant.Sys;
using SuperMemoAssistant.Sys.IO;

namespace SuperMemoAssistant.SMA
{
  /// <summary>
  ///   Wrapper around a SM management instance that handles SuperMemo App lifecycle events
  ///   (start, exit, ...) and provides a safe interface to interact with SuperMemo
  /// </summary>
  public class SMA
    : PerpetualMarshalByRefObject,
      ISuperMemoAssistant,
      IDisposable
  {
    #region Constants & Statics

    public static SMA Instance { get; } = new SMA();

    #endregion




    #region Properties & Fields - Non-Public

    private CollectionsCfg _collectionsCfg;

    protected SuperMemoCore _sm;

    #endregion




    #region Constructors

    /// <summary>
    ///   Create an instance of the wrapper that will start a SM instance and attach the
    ///   management engine.
    /// </summary>
    protected SMA()
    {
      Core.SMA = this;
    }


    /// <inheritdoc />
    public virtual void Dispose()
    {
      _sm?.Dispose();
    }

    #endregion




    #region Properties & Fields - Public

    public StartupCfg    StartupConfig    { get; set; }
    public CollectionCfg CollectionConfig { get; set; }

    public SuperMemoCore SMBase => _sm;

    public IProcess SMProcess => _sm?.SMProcess;

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public IEnumerable<string> Layouts => LayoutManager.Instance.Layouts
                                                       .Select(l => l.Name)
                                                       .OrderBy(n => n)
                                                       .ToList();

    public ISuperMemo SM => _sm;

    #endregion




    #region Methods

    //
    // Collection loading management

    public async Task<bool> Start(SMCollection collection)
    {
      try
      {
        if (_sm != null)
          throw new InvalidOperationException("_sm is already instanciated");

        LoadConfig(collection);

        if (new FilePath(StartupConfig.SMBinPath).Exists() == false)
          throw new FileNotFoundException($"Invalid file path for sm executable file: '{StartupConfig.SMBinPath}' could not be found.");

        // TODO: Look at PE version and select Management Engine version
        _sm = new SM17(collection,
                       StartupConfig.SMBinPath);

        // TODO: Move somewhere else
        _sm.UI.ElementWdw.OnAvailable += OnSuperMemoWindowsAvailable;

        await _sm.Start();
        // TODO: Ensure opened collection (windows title) matches parameter
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Failed to start SM.");

        _sm?.Dispose();
        _sm = null;

        try
        {
          if (OnSMStoppedEvent != null)
            await OnSMStoppedEvent.InvokeAsync(this,
                                               new SMProcessArgs(_sm, null));
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

    public void ApplySuperMemoWindowStyles()
    {
      if (CollectionConfig.CollapseElementWdwTitleBar)
        WindowStyling.MakeWindowTitleless(_sm.UI.ElementWdw.Handle);
    }

    public void LoadConfig(SMCollection collection)
    {
      var knoPath = collection.GetKnoFilePath();

      // StartupCfg

      StartupConfig = Svc.Configuration.Load<StartupCfg>().Result ?? new StartupCfg();

      // CollectionsCfg

      _collectionsCfg  = Svc.Configuration.Load<CollectionsCfg>().Result ?? new CollectionsCfg();
      CollectionConfig = _collectionsCfg.CollectionsConfig.SafeGet(knoPath);

      if (CollectionConfig == null)
      {
        CollectionConfig                           = new CollectionCfg();
        _collectionsCfg.CollectionsConfig[knoPath] = CollectionConfig;
      }
    }

    public Task SaveConfig(bool sync)
    {
      var tasks = new[]
      {
        Svc.Configuration.Save<StartupCfg>(StartupConfig),
        Svc.Configuration.Save<CollectionsCfg>(_collectionsCfg),
      };

      var task = Task.WhenAll(tasks);

      if (sync)
        task.Wait();

      return task;
    }

    public async Task OnSMStarting()
    {
      try
      {
        if (OnSMStartingEvent != null)
          await OnSMStartingEvent.InvokeAsync(this,
                                              new SMEventArgs(_sm));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception while notifying starting");
        throw;
      }
    }

    public async Task OnSMStarted()
    {
      try
      {
        await SaveConfig(false);

        SMAUI.Initialize();

        if (OnSMStartedEvent != null)
          await OnSMStartedEvent.InvokeAsync(
            this,
            new SMProcessArgs(_sm, SMProcess.Native));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception while notifying started");

        throw;
      }
    }

    public async Task OnSMStopped()
    {
      _sm = null;

      if (OnSMStoppedEvent != null)
        await OnSMStoppedEvent.InvokeAsync(this, null);
    }

    private void OnSuperMemoWindowsAvailable()
    {
      ApplySuperMemoWindowStyles();
    }

    #endregion




    #region Events

    public virtual event AsyncEventHandler<SMProcessArgs> OnSMStartedEvent;
    public virtual event AsyncEventHandler<SMEventArgs>   OnSMStartingEvent;
    public virtual event AsyncEventHandler<SMProcessArgs> OnSMStoppedEvent;

    #endregion
  }
}
