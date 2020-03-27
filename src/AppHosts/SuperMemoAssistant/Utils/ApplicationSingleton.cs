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
// Modified On:  2020/03/19 01:53
// Modified By:  Alexis

#endregion




using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using SuperMemoAssistant.Models;

namespace SuperMemoAssistant.Utils
{
  // https://github.com/novotnyllc/SingleInstanceHelper
  /// <summary>Helper for creating or passing command args to a single application instance</summary>
  public static class ApplicationSingleton
  {
    #region Constants & Statics

    /// <summary>
    ///   Unique name to base the single instance decision on. Default's to a hash based on the
    ///   executable location.
    /// </summary>
    public static string UniqueName { get; set; } = GetRunningProcessHash();

    private static          Mutex                  _mutexApplication;
    private static readonly object                 _mutexLock = new object();
    private static          bool                   _firstApplicationInstance;
    private static          NamedPipeServerStream  _namedPipeServerStream;
    private static          SynchronizationContext _syncContext;
    private static          Action<string[]>       _otherInstanceCallback;

    #endregion




    #region Methods

    private static string GetMutexName() => $@"Mutex_{Environment.UserDomainName}_{Environment.UserName}_{UniqueName}";
    private static string GetPipeName()  => $@"Pipe_{Environment.UserDomainName}_{Environment.UserName}_{UniqueName}";

    /// <summary>
    ///   Determines if the application should continue launching or return because it's not
    ///   the first instance. When not the first instance, the command line args will be passed to the
    ///   first one.
    /// </summary>
    /// <param name="otherInstanceCallback">
    ///   Callback to execute on the first instance with command
    ///   line args from subsequent launches. Will not run on the main thread, marshalling may be
    ///   required.
    /// </param>
    /// <param name="args">Arguments from Main()</param>
    /// <returns>true if the first instance, false if it's not the first instance.</returns>
    public static bool IsOnlyInstance(Action<string[]> otherInstanceCallback, string[] args)
    {
      _otherInstanceCallback = otherInstanceCallback ?? throw new ArgumentNullException(nameof(otherInstanceCallback));

      if (IsApplicationFirstInstance())
      {
        _syncContext = SynchronizationContext.Current;
        // Setup Named Pipe listener
        NamedPipeServerCreateServer();
        return true;
      }
      else
      {
        // We are not the first instance, send the named pipe message with our payload and stop loading
        var namedPipeXmlPayload = new AppPayload
        {
          CommandLineArguments = Environment.GetCommandLineArgs().ToList()
        };

        // Send the message
        NamedPipeClientSendOptions(namedPipeXmlPayload);
        return false; // Signal to quit
      }
    }

    /// <summary>Checks if this is the first instance of this application. Can be run multiple times.</summary>
    /// <returns></returns>
    private static bool IsApplicationFirstInstance()
    {
      if (_mutexApplication == null)
        lock (_mutexLock)
          // Allow for multiple runs but only try and get the mutex once
          if (_mutexApplication == null)
            _mutexApplication = new Mutex(true, GetMutexName(), out _firstApplicationInstance);

      return _firstApplicationInstance;
    }

    /// <summary>
    ///   Uses a named pipe to send the currently parsed options to an already running
    ///   instance.
    /// </summary>
    /// <param name="namedPipePayload"></param>
    private static void NamedPipeClientSendOptions(AppPayload namedPipePayload)
    {
      try
      {
        using (var namedPipeClientStream = new NamedPipeClientStream(".", GetPipeName(), PipeDirection.Out))
        {
          namedPipeClientStream.Connect(3000); // Maximum wait 3 seconds

          var ser = new DataContractJsonSerializer(typeof(AppPayload));
          ser.WriteObject(namedPipeClientStream, namedPipePayload);
        }
      }
      catch (Exception)
      {
        // Error connecting or sending
      }
    }

    /// <summary>Starts a new pipe server if one isn't already active.</summary>
    private static void NamedPipeServerCreateServer()
    {
      // 
      // Create pipe and start the async connection wait
      _namedPipeServerStream = new NamedPipeServerStream(
        GetPipeName(),
        PipeDirection.In,
        1,
        PipeTransmissionMode.Byte,
        PipeOptions.Asynchronous); // | PipeOptions.CurrentUserOnly);

      // Begin async wait for connections
      _namedPipeServerStream.BeginWaitForConnection(NamedPipeServerConnectionCallback, _namedPipeServerStream);
    }

    /// <summary>
    ///   The function called when a client connects to the named pipe. Note: This method is
    ///   called on a non-UI thread.
    /// </summary>
    /// <param name="iAsyncResult"></param>
    private static void NamedPipeServerConnectionCallback(IAsyncResult iAsyncResult)
    {
      try
      {
        // End waiting for the connection
        _namedPipeServerStream.EndWaitForConnection(iAsyncResult);

        var ser     = new DataContractJsonSerializer(typeof(AppPayload));
        var payload = (AppPayload)ser.ReadObject(_namedPipeServerStream);

        // payload contains the data sent from the other instance
        if (_syncContext != null)
          _syncContext.Post(_ => _otherInstanceCallback(payload.CommandLineArguments.ToArray()), null);
        else
          _otherInstanceCallback(payload.CommandLineArguments.ToArray());
      }
      catch (ObjectDisposedException)
      {
        // EndWaitForConnection will exception when someone calls closes the pipe before connection made
        // In that case we dont create any more pipes and just return
        // This will happen when app is closing and our pipe is closed/disposed
        return;
      }
      catch (Exception)
      {
        // ignored
      }
      finally
      {
        // Close the original pipe (we will create a new one each time)
        _namedPipeServerStream.Dispose();
      }

      // Create a new pipe for next connection
      NamedPipeServerCreateServer();
    }

    private static string GetRunningProcessHash()
    {
      using (var hash = SHA256.Create())
      {
        var processPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        var bytes       = hash.ComputeHash(Encoding.UTF8.GetBytes(processPath));
        return Convert.ToBase64String(bytes);
      }
    }

    #endregion
  }
}
