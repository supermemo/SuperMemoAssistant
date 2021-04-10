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




// ReSharper disable UnusedParameter.Local
// ReSharper disable ClassNeverInstantiated.Global

namespace SuperMemoAssistant.Hooks.InjectLib
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.Remoting;
  using System.Runtime.Remoting.Channels.Ipc;
  using System.Security.Principal;
  using System.Threading;
  using System.Threading.Tasks;
  using EasyHook;
  using Sentry;
  using SMA.Hooks.Services;
  using SuperMemo;

  public sealed partial class SMInject : MarshalByRefObject, ISMInject, IEntryPoint, IDisposable
  {
    #region Properties & Fields - Non-Public

    private readonly string _injectLibChannelName;

    private IDisposable SentryInstance { get; }

    private ISMAHook SMA { get; set; }

    private bool HasExited { get; set; }

    #endregion




    #region Constructors

    static SMInject()
    {
      RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
    }

    [SuppressMessage("Microsoft.Performance", "CA1801")]
    [SuppressMessage("Redundancy", "RCS1163:Unused parameter.", Justification = "Prototype cannot be changed")]
    public SMInject(RemoteHooking.IContext context,
                    string                 channelName,
                    NativeData             nativeData)
    {
      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
      AppDomain.CurrentDomain.AssemblyResolve    += CurrentDomain_AssemblyResolve;

      SentryInstance = SentrySdk.Init("https://a63c3dad9552434598dae869d2026696@sentry.io/1362046");

      SMA = (ISMAHook)RemoteHooking.IpcConnectClient<MarshalByRefObject>(channelName);
      SMInjectServer =
        RemoteHooking.IpcCreateServer(ref _injectLibChannelName, WellKnownObjectMode.Singleton, this, WellKnownSidType.WorldSid);

      _ = Task.Factory.StartNew(KeepAlive, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public void Dispose()
    {
      HasExited = true;

      CleanupHooks();

      _execAvailableEvent.Dispose();
      _dataAvailableEvent.Dispose();
      _smProcess.Dispose();

      SMInjectServer?.StopListening(null);

      SentryInstance.Dispose();
    }

    #endregion




    #region Properties & Fields - Public

    public IpcServerChannel SMInjectServer { get; }

    #endregion




    #region Methods Impl

    public override object InitializeLifetimeService()
    {
      return null;
    }

    #endregion




    #region Methods

    [SuppressMessage("Microsoft.Performance", "CA1801")]
    [SuppressMessage("Redundancy", "RCS1163:Unused parameter.", Justification = "Prototype cannot be changed")]
    public void Run(RemoteHooking.IContext inContext,
                    string                 channelName,
                    NativeData             nativeData)
    {
      try
      {
        try
        {
          InstallHooks();
          InstallSM(nativeData);

          SMA.OnHookInstalled(_injectLibChannelName, null);
        }
        catch (Exception ex)
        {
          SMA.OnHookInstalled(_injectLibChannelName, ex);
          Environment.Exit(1);
          return;
        }
        finally
        {
          RemoteHooking.WakeUpProcess();
        }

        DispatchMessages();
      }
      catch (RemotingException)
      {
        // Channel closed, exit.
        SMA = null;
      }
      catch (Exception ex)
      {
        OnException(ex);
      }
      finally
      {
        Dispose();
      }
    }

    private void CurrentDomain_UnhandledException(object                      sender,
                                                  UnhandledExceptionEventArgs e)
    {
      OnException(e.ExceptionObject as Exception);
    }

    private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs e)
    {
      var assembly = AppDomain.CurrentDomain
                              .GetAssemblies()
                              .FirstOrDefault(a => a.FullName == e.Name);

      if (assembly != null)
        return assembly;

      var assemblyName = e.Name.Split(',').First() + ".dll";
      var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName);

      if (File.Exists(assemblyPath))
        try
        {
          return Assembly.LoadFrom(assemblyPath);
        }
        catch (Exception ex)
        {
          OnException(ex);

          throw;
        }

      OnException(new FileNotFoundException($"Assembly {assemblyName} could not be found in {assemblyPath}"));

      return null;
    }

    #endregion
  }
}
