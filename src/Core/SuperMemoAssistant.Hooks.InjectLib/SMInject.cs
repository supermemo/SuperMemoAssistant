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
// Created On:   2018/05/12 01:26
// Modified On:  2018/12/23 06:26
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading;
using System.Threading.Tasks;
using EasyHook;

// ReSharper disable ClassNeverInstantiated.Global

namespace SuperMemoAssistant.Hooks.InjectLib
{
  public class SMInject : IEntryPoint
  {
    #region Constants & Statics

    //
    // Instance

    public static SMInject Instance { get; private set; }

    #endregion




    #region Properties & Fields - Non-Public

    protected List<LocalHook> LocalHooks { get; } = new List<LocalHook>();

    protected bool Exited { get; set; } = false;

    protected AutoResetEvent       DataAvailableEvent { get; set; }
    protected ReaderWriterLockSlim RWLock             { get; } = new ReaderWriterLockSlim();

    protected ConcurrentQueue<(string, object[])> DataQueue { get; set; } = new ConcurrentQueue<(string, object[])>();

    #endregion




    #region Constructors

    // ReSharper disable once UnusedParameter.Local
    public SMInject(RemoteHooking.IContext _)
    {
      Instance           = this;
      DataAvailableEvent = new AutoResetEvent(false);

      //Debugger.Launch();

      // TODO: Switch to WCF DuplexClientBase
      Callback = (SMHookCallback)RemoteHooking.IpcConnectClient<MarshalByRefObject>(HookConst.ChannelName);
      Task.Run((Action)KeepAlive);

      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    #endregion




    #region Properties & Fields - Public

    public SMHookCallback Callback { get; set; }

    public HashSet<string> TargetFilePaths { get; set; } = new HashSet<string>();
    public HashSet<IntPtr> TargetHandles   { get; }      = new HashSet<IntPtr>();

    #endregion




    #region Methods

    private void CurrentDomain_UnhandledException(object                      sender,
                                                  UnhandledExceptionEventArgs e)
    {
      try
      {
        Callback.OnException((Exception)e.ExceptionObject);
      }
      catch
      {
        // ignored
      }
    }


    //
    // Core

    public void Run(RemoteHooking.IContext inContext)
    {
      SMHooks smHooks = null;

      try
      {
        try
        {
          InstallHooks();

          Callback.OnHookInstalled(true);
          TargetFilePaths = new HashSet<string>(Callback.GetTargetFilePaths().Select(s => s.ToLowerInvariant()));

          smHooks = new SMHooks();

          RemoteHooking.WakeUpProcess();

          smHooks.InstallWndProcHook();
        }
        catch (Exception ex)
        {
          Callback.OnHookInstalled(false,
                                   ex);
          return;
        }

        while (true)
        {
          DataAvailableEvent.WaitOne();

          Queue<(string, object[])> localQueue;
          RWLock.EnterWriteLock();

          try
          {
            localQueue = new Queue<(string, object[])>(DataQueue.Count);

            while (DataQueue.TryDequeue(out var data))
              localQueue.Enqueue(data);

            DataAvailableEvent.Reset();
          }
          finally
          {
            RWLock.ExitWriteLock();
          }

          int count = localQueue.Count;

          for (int i = 0; i < count; i++)
          {
            var data = localQueue.Dequeue();
            ProcessData(data.Item1,
                        data.Item2);
          }
        }
      }
      catch (RemotingException)
      {
        // Channel closed, exit.
        Callback = null;
      }
      catch (Exception ex)
      {
        try
        {
          Callback.OnException(ex);
        }
        catch
        {
          // ignored
        }
      }
      finally
      {
        Exited = true;

        try
        {
          smHooks?.Dispose();
          /*SMWndProc.Disable();
          SMWndProc.Dispose();*/
        }
        catch (Exception ex)
        {
          try
          {
            Callback?.OnException(ex);
          }
          catch
          {
            // ignored
          }
        }

        try
        {
          foreach (var lh in LocalHooks)
            lh?.Dispose();

          LocalHook.Release();
        }
        catch (Exception ex)
        {
          try
          {
            Callback?.OnException(ex);
          }
          catch
          {
            // ignored
          }
        }
      }
    }

    protected void InstallHooks()
    {
      LocalHooks.AddRange(IOHooks.InstallHooks());

      foreach (LocalHook lc in LocalHooks)
        lc.ThreadACL.SetExclusiveACL(new[] { 0 });
    }

    protected void ProcessData(string   funcName,
                               object[] datas)
    {
      switch (funcName)
      {
        case "CreateFile":
          Callback.OnFileCreate((string)datas[0],
                                (IntPtr)datas[1]);
          break;

        case "SetFilePointer":
          Callback.OnFileSeek((IntPtr)datas[0],
                              (UInt32)datas[1]);
          break;

        case "WriteFile":
          Callback.OnFileWrite((IntPtr)datas[0],
                               (Byte[])datas[1],
                               (UInt32)datas[2]);
          break;

        case "CloseHandle":
          Callback.OnFileClose((IntPtr)datas[0]);
          break;
      }
    }

    protected void KeepAlive()
    {
      try
      {
        while (!Exited)
        {
          Callback.KeepAlive();

          for (int i = 0; i < 30 && !Exited; i++)
            Thread.Sleep(1000);
        }
      }
      catch (Exception)
      {
        // ignored
      }
    }

    public void Debug(string          str,
                      params object[] args)
    {
      Callback.Debug(str,
                     args);
    }

    public void OnException<T>(T ex)
      where T : Exception
    {
      Callback.OnException(ex);
    }

    public bool OnUserMessage(int wParam)
    {
      switch (wParam)
      {
        case 9100199:
          if (Debugger.IsAttached == false)
            Debugger.Launch();

          else
            Debugger.Break();

          return true;

        default:
          //return Callback.OnUserMessage(wParam);
          return false;
      }
    }

    public void Enqueue(string          funcName,
                        params object[] datas)
    {
      RWLock.EnterReadLock();

      try
      {
        DataQueue.Enqueue((funcName, datas));
        DataAvailableEvent.Set();
      }
      finally
      {
        RWLock.ExitReadLock();
      }
    }

    #endregion
  }
}
