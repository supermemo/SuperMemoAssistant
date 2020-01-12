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
// Created On:   2019/03/02 18:29
// Modified On:  2020/01/12 12:01
// Modified By:  Alexis

#endregion




using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using EasyHook;
using Sentry;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.SMA.Hooks;
using SuperMemoAssistant.SuperMemo;

// ReSharper disable ClassNeverInstantiated.Global

namespace SuperMemoAssistant.Hooks.InjectLib
{
  public partial class SMInject : IEntryPoint
  {
    #region Properties & Fields - Non-Public

    private IDisposable SentryInstance { get; }

    private SMAHookCallback SMA { get; set; }

    private bool HasExited { get; set; }

    #endregion




    #region Constructors

    // ReSharper disable once UnusedParameter.Local
    public SMInject(RemoteHooking.IContext context,
                    string                 channelName,
                    NativeData             nativeData)
    {
      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
      AppDomain.CurrentDomain.AssemblyResolve    += CurrentDomain_AssemblyResolve;

      SentryInstance = SentrySdk.Init("https://a63c3dad9552434598dae869d2026696@sentry.io/1362046");

      // TODO: Switch to WCF DuplexClientBase
      SMA = (SMAHookCallback)RemoteHooking.IpcConnectClient<MarshalByRefObject>(channelName);
      Task.Factory.StartNew(KeepAlive, TaskCreationOptions.LongRunning);
    }

    #endregion




    #region Methods

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

          SMA.OnHookInstalled(true);
        }
        catch (Exception ex)
        {
          SMA.OnHookInstalled(false,
                              ex);
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
        try
        {
          SMA.OnException(ex);
        }
        catch
        {
          // ignored
        }
      }
      finally
      {
        HasExited = true;

        CleanupHooks();

        SentryInstance.Dispose();
      }
    }

    private void CurrentDomain_UnhandledException(object                      sender,
                                                  UnhandledExceptionEventArgs e)
    {
      try
      {
        SMA.OnException((Exception)e.ExceptionObject);
      }
      catch
      {
        // ignored
      }
    }

    private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs e)
    {
      var assembly = AppDomain.CurrentDomain
                              .GetAssemblies()
                              .FirstOrDefault(a => a.FullName == e.Name);

      if (assembly != null)
        return assembly;

      var assemblyName = e.Name.Split(',').First() + ".dll";
      var assemblyPath = SMAFileSystem.AppRootDir
                                      .CombineFile(assemblyName);

      return assemblyPath.Exists() == false
        ? null
        : Assembly.LoadFrom(assemblyPath.FullPath);
    }

    #endregion
  }
}
