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
// Created On:   2018/05/08 13:06
// Modified On:  2019/01/05 04:06
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Anotar.Serilog;
using Process.NET;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Plugins;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.SuperMemo.SuperMemo17;
using SuperMemoAssistant.Sys;

namespace SuperMemoAssistant.SuperMemo
{
  /// <summary>
  ///   Wrapper around a SM management instance that handles SuperMemo App lifecycle events
  ///   (start, exit, ...) and provides a safe interface to interact with SuperMemo
  /// </summary>
  [InitOnLoad]
  public class SMA
    : SMMarshalByRefObject,
      ISuperMemoAssistant, // Proxy for wrapped SMxx object
      IDisposable
  {
    #region Constants & Statics

    protected static readonly Dictionary<Regex, Func<SMCollection, SuperMemoBase>>
      SMTitleFactoryMap =
        new Dictionary<Regex, Func<SMCollection, SuperMemoBase>>
        {
          {
            new Regex(SM17.RE_WindowTitle,
                      RegexOptions.Compiled | RegexOptions.IgnoreCase),
            c => new SM17(c)
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

      Config = LoadConfig();
      //StartMonitoring();
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
      //StopMonitoring();

      SMMgmt?.Dispose();
    }

    #endregion




    #region Properties & Fields - Public

    public CoreCfg Config { get; set; }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public SMCollection Collection { get => SMMgmt?.Collection; private set => throw new InvalidOperationException(); }
    /// <inheritdoc />
    public virtual SMAppVersion AppVersion => SMMgmt?.AppVersion ?? SMConst.Versions.vInvalid;
    /// <inheritdoc />
    public IProcess SMProcess => SMMgmt?.SMProcess;
    /// <inheritdoc />
    public bool IgnoreUserConfirmation
    {
      get => SMMgmt?.IgnoreUserConfirmation ?? false;
      set
      {
        if (SMMgmt != null) SMMgmt.IgnoreUserConfirmation = value;
      }
    }

    public ISuperMemoRegistry Registry => SuperMemoRegistry.Instance;
    public ISuperMemoUI       UI       => SuperMemoUI.Instance;

    /// <inheritdoc />
    public bool IsRunning => SMMgmt != null;

    #endregion




    #region Methods Impl

    //
    // Collection loading management

    public bool Start(SMCollection collection)
    {
      if (SMMgmt != null)
        return false;

      try
      {
        // TODO: Look at PE version and select Management Engine version
        var dummy = new SM17(collection);

        // TODO: Ensure opened collection (windows title) matches parameter
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Failed to load SM17.");

        SMMgmt = null;

        try
        {
          OnSMStoppedEvent?.Invoke(this,
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

    #endregion




    #region Methods

    public CoreCfg LoadConfig()
    {
      return Svc<CorePlugin>.Configuration.Load<CoreCfg>().Result ?? new CoreCfg();
    }

    public void SaveConfig(bool sync)
    {
      var task = Svc<CorePlugin>.Configuration.Save<CoreCfg>(Config);

      if (sync)
        task.Wait();
    }

    public void OnSMStartingImpl(SuperMemoBase smMgmt)
    {
      SMMgmt = smMgmt;

      try
      {
        OnSMStartingEvent?.Invoke(this,
                                  new SMEventArgs(this));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Error while notifying plugins OnSMStartingEvent");
      }
    }

    /// <summary>Called from this very class when a new matching SM Instance is found</summary>
    public void OnSMStartedImpl()
    {
      SMMgmt.OnSMStoppedEvent += OnSMStoppedImpl;

      SaveConfig(false);

      try
      {
        OnSMStartedEvent?.Invoke(this,
                                 new SMProcessArgs(this,
                                                   SMProcess));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Error while notifying plugins OnSMStartedEvent");
      }
    }

    protected void OnSMStoppedImpl(object        sender,
                                   SMProcessArgs args)
    {
      SMMgmt = null;

      try
      {
        OnSMStoppedEvent?.Invoke(this,
                                 ProxifyArgs(args));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Error while notifying plugins OnSMStoppedEvent");
      }
    }

    protected EventHandler<T> MakeEventProxy<T>(EventHandler<T> eventProxy)
      where T : SMEventArgs
    {
      void Proxy(object sm,
                 T      args)
      {
        try
        {
          eventProxy?.Invoke(this,
                             ProxifyArgs(args));
        }
        catch (Exception ex)
        {
          LogTo.Error(ex,
                      "Error while invoking proxified event");
        }
      }

      return Proxy;
    }

    protected T ProxifyArgs<T>(T args)
      where T : SMEventArgs
    {
      return args.With(a => a.SMMgmt = this);
    }


    //
    // Process-monitoring-related

/*
/// <summary>
/// Starts a System-wide Watch on Processes being started
/// </summary>
    protected void StartMonitoring()
    {
      StopMonitoring();

      ProcessStartedWatcher =
 new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
      ProcessStartedWatcher.EventArrived += ProcessStarted_EventArrived;
      ProcessStartedWatcher.Start();
    }

    /// <summary>
    /// Stop watching, cleanup
    /// </summary>
    protected void StopMonitoring()
    {
      if (ProcessStartedWatcher != null)
      {
        ProcessStartedWatcher.Stop();
        ProcessStartedWatcher.EventArrived -= ProcessStarted_EventArrived;
        ProcessStartedWatcher = null;
      }
    }

    /// <summary>
    /// We are being notified a new process has been started.
    /// Check whether this is the SuperMemo we are interested in.
    /// If we already wrap a SM Management instance, ignore the notification.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void ProcessStarted_EventArrived(object sender, EventArrivedEventArgs e)
    {
      if (SMMgmt != null)
        return;

      string procName = (string)e.NewEvent.Properties["ProcessName"].Value;

      if (procName?.Contains("SuperMemo") == false)
        return;

      int processId = (int)e.NewEvent.GetPropertyValue("ProcessID");
      Process process = Process.GetProcessById(processId);
      ISuperMemo smMgmt = null;

      try
      {
        if (TryCreateInstance(process, CollectionName, out smMgmt))
          OnSMStartedImpl(this, new SMProcessArgs(smMgmt, process));
      }
      catch (Exception ex)
      {
        // TODO: Log/Warn + Callback for UI notification
      }
    }


    //
    // Process-collection-related

    /// <summary>
    /// Creates a new SM management instance for given collection name, if available
    /// </summary>
    /// <param name="collectionName">As displayed in SM window's title</param>
    /// <returns>SM management instance, or null if instance for given collection name cannot be found</returns>
    protected ISuperMemo CreateFromInstance(string collectionName)
    {
      ISuperMemo smMgmt = null;

      Process.GetProcesses()
        .FirstOrDefault(p => TryCreateInstance(p, collectionName, out smMgmt));

      return smMgmt;
    }

    /// <summary>
    /// Attempts to create a new SM management interface for given process and collection name
    /// </summary>
    /// <param name="p">Target Process</param>
    /// <param name="collectionName"></param>
    /// <param name="instance">Resulting SM mgmt interface</param>
    /// <returns>Whether the operation was successfull</returns>
    protected bool TryCreateFromInstance(Process p, string collectionName, out ISuperMemo instance)
    {
      var allTitleRegEx = SMTitleFactoryMap.Keys;

      instance = allTitleRegEx.Select(
        r =>
        {
          var match = r.Match(p.ProcessName);

          return match.Success && collectionName.Equals(match.Groups[1].Value)
            ? SMTitleFactoryMap[r](p)
            : null;
        }
      ).FirstOrDefault(i => i != null);

      return instance != null;
    }
*/

    /// <summary>Probe running smXX.exe processes</summary>
    /// <returns>Open collection names</returns>
    public static IEnumerable<SMCollection> GetRunningInstances()
    {
      var allTitleRegEx = SMTitleFactoryMap.Keys;

      return System.Diagnostics.Process.GetProcesses()
                   .Select(p => allTitleRegEx
                                .Select(r => r.Match(p.ProcessName))
                                .FirstOrDefault(r => r.Success)
                                ?.Groups
                   )
                   .Where(g => g != null)
                   .Select(g => new SMCollection(g[1].Value,
                                                 g[2].Value));
    }

    #endregion




    #region Events

    //
    // ISuperMemo Events

    public virtual event EventHandler<SMProcessArgs> OnSMStartedEvent;
    public virtual event EventHandler<SMEventArgs>   OnSMStartingEvent;
    public virtual event EventHandler<SMProcessArgs> OnSMStoppedEvent;

    #endregion
  }
}
