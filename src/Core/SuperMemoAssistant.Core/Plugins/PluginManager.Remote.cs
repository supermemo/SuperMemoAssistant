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
// Created On:   2019/02/13 13:55
// Modified On:  2019/02/22 13:39
// Modified By:  Alexis

#endregion




using System.Runtime.Remoting.Channels.Ipc;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.Plugins;

namespace SuperMemoAssistant.Plugins
{
  public partial class PluginManager
  {
    #region Properties & Fields - Non-Public

    private string IpcServerChannelName { get; set; }

    private IpcServerChannel IpcServer { get; set; }

    #endregion




    #region Methods

    private void StartIpcServer()
    {
      LogTo.Debug("Starting Plugin IPC Server");

      // Generate random channel name
      IpcServerChannelName = RemotingServicesEx.GenerateIpcServerChannelName();

      IpcServer = RemotingServicesEx.CreateIpcServer<ISMAPluginManager, PluginManager>(this, IpcServerChannelName);
    }

    private void StopIpcServer()
    {
      LogTo.Debug("Stopping Plugin IPC Server");

      IpcServer.StopListening(null);
      IpcServer = null;
    }

    #endregion
  }
}
