using EasyHook;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Hooks.InjectLib
{
  public class SMInject : IEntryPoint
  {
    protected List<LocalHook> LocalHooks { get; } = new List<LocalHook>();

    protected SMHookCallback Callback { get; set; }
    protected bool Exited { get; set; } = false;

    protected AutoResetEvent DataAvailableEvent { get; set; }
    protected ReaderWriterLockSlim RWLock { get; } = new ReaderWriterLockSlim();

    protected ConcurrentQueue<(string, object[])> DataQueue { get; set; } = new ConcurrentQueue<(string, object[])>();

    public HashSet<string> TargetFilePaths { get; set; }
    public HashSet<IntPtr> TargetHandles { get; set; } = new HashSet<IntPtr>();



    //
    // Instance

    public static SMInject Instance { get; private set; }
    public SMInject(RemoteHooking.IContext InContext)
    {
      Instance = this;
      DataAvailableEvent = new AutoResetEvent(false);

      // TODO: Switch to WCF DuplexClientBase
      Callback = (SMHookCallback)RemoteHooking.IpcConnectClient<MarshalByRefObject>(HookConst.ChannelName);
      Task.Run((Action)KeepAlive);

      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      try
      {
        Callback.OnException((Exception)e.ExceptionObject);
      }
      catch (Exception)
      {
      }
    }



    //
    // Core

    public void Run(RemoteHooking.IContext InContext)
    {
      try
      {
        try
        {
          //System.Diagnostics.Debugger.Launch();
          InstallHooks();

          Callback.OnHookInstalled(true);
          TargetFilePaths = new HashSet<string>(Callback.GetTargetFilePaths().Select(s => s.ToLowerInvariant()));

          RemoteHooking.WakeUpProcess();
        }
        catch (Exception ex)
        {
          Callback.OnHookInstalled(false, ex);
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
            ProcessData(data.Item1, data.Item2);
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
        }
      }
      finally
      {
        Exited = true;

        try
        {
          foreach (var lh in LocalHooks)
            lh?.Dispose();

          LocalHook.Release();
        }
        catch
        {
        }
      }
    }

    protected void InstallHooks()
    {
      LocalHooks.AddRange(IOHooks.InstallHooks());

      foreach (LocalHook lc in LocalHooks)
        lc.ThreadACL.SetExclusiveACL(new Int32[1] { 0 });
    }

    protected void ProcessData(string funcName, object[] datas)
    {
      switch (funcName)
      {
        case "CreateFile":
          Callback.OnFileCreate((string)datas[0], (IntPtr)datas[1]);
          break;

        case "SetFilePointer":
          Callback.OnFileSeek((IntPtr)datas[0], (UInt32)datas[1]);
          break;

        case "WriteFile":
          Callback.OnFileWrite((IntPtr)datas[0], (Byte[])datas[1], (UInt32)datas[2]);
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
      }
    }

    public void Debug(string str, params object[] args)
    {
      Callback.Debug(str, args);
    }

    public void Enqueue(string funcName, params object[] datas)
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
  }
}
