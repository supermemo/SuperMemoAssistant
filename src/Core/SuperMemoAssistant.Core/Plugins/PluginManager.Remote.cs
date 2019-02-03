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
// Created On:   2019/01/26 03:53
// Modified On:  2019/01/26 06:57
// Modified By:  Alexis

#endregion




using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Anotar.Serilog;

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
      LogTo.Information("Starting Plugin IPC Server");

      // Generate random channel name
      GenerateIpcServerChannelName();

      System.Collections.IDictionary properties = new System.Collections.Hashtable()
      {
        { "name", IpcServerChannelName },
        { "portName", IpcServerChannelName }
      };

      // Setup ACL : allow access from all users. Channel is protected by a random name.
      DiscretionaryAcl dacl = new DiscretionaryAcl(false, false, 1);

      dacl.AddAccess(
        AccessControlType.Allow,
        new SecurityIdentifier(
          WellKnownSidType.WorldSid,
          null),
        -1,
        InheritanceFlags.None,
        PropagationFlags.None);

      CommonSecurityDescriptor secDescr = new CommonSecurityDescriptor(false, false,
                                                                       ControlFlags.GroupDefaulted |
                                                                       ControlFlags.OwnerDefaulted |
                                                                       ControlFlags.DiscretionaryAclPresent,
                                                                       null, null, null,
                                                                       dacl);

      // Formatter sink
      // TODO: Custom sink
      BinaryServerFormatterSinkProvider binaryProv = new BinaryServerFormatterSinkProvider { TypeFilterLevel = TypeFilterLevel.Full };

      // Create server
      IpcServer = new IpcServerChannel(properties, binaryProv, secDescr);

      ChannelServices.RegisterChannel(IpcServer, false);

      // Initialize with SMA interface
      RemotingServices.Marshal(this, IpcServerChannelName);
    }

    private void StopIpcServer()
    {
      IpcServer.StopListening(null);
      IpcServer = null;
    }

    private void GenerateIpcServerChannelName()
    {
      RNGCryptoServiceProvider rnd     = new RNGCryptoServiceProvider();
      var                      data    = new byte[30];
      StringBuilder            builder = new StringBuilder();

      rnd.GetBytes(data);

      for (int i = 0; i < 20 + data[0] % 10; i++)
      {
        var b = (byte)(data[i] % 62);

        if (b <= 9)
          builder.Append((char)('0' + b));
        else if (b >= 10 && b <= 35)
          builder.Append((char)('A' + (b - 10)));
        else if (b >= 36 && b <= 61)
          builder.Append((char)('a' + (b - 36)));
      }

      IpcServerChannelName = builder.ToString();
    }

    #endregion
  }
}
