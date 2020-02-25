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
// Created On:   2019/09/03 18:15
// Modified On:  2020/01/17 10:45
// Modified By:  Alexis

#endregion




using System;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Anotar.Serilog;

namespace SuperMemoAssistant.Extensions
{
  public static class RemotingServicesEx
  {
    #region Methods

    public static IService ConnectToIpcServer<IService>(
      string channelName,
      string channelPort = null)
    {
      return (IService)Activator.GetObject(
        typeof(IService),
        "ipc://" + channelName + "/" + (channelPort ?? channelName));
    }

    public static IpcServerChannel CreateIpcServer<IService, TService>(
      TService         service,
      string           channelName = null,
      string           portName    = null,
      WellKnownSidType aclSid      = WellKnownSidType.WorldSid)
      where IService : class
      where TService : MarshalByRefObject, IService
    {
      if (channelName == null)
        channelName = GenerateIpcServerChannelName();

      System.Collections.IDictionary properties = new System.Collections.Hashtable
      {
        { "name", channelName },
        { "portName", portName ?? channelName }
      };

      // Setup ACL : allow access from all users. Channel is protected by a random name.
      DiscretionaryAcl dacl = new DiscretionaryAcl(false, false, 1);

      dacl.AddAccess(
        AccessControlType.Allow,
        new SecurityIdentifier(
          aclSid,
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
      var ipcServer = new IpcServerChannel(properties, binaryProv, secDescr);

      ChannelServices.RegisterChannel(ipcServer, false);

      // Initialize with SMA interface
      RemotingServices.Marshal(service, channelName, typeof(IService));

      return ipcServer;
    }

    public static string GenerateIpcServerChannelName()
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

      return builder.ToString();
    }

    public static void InvokeRemote<TParam1>(
      this Action<TParam1>    @event,
      string                  eventName,
      TParam1                 p1,
      Action<Action<TParam1>> unsubscribeDelegate)
    {
      foreach (var handler in @event.GetInvocationList().Cast<Action<TParam1>>())
        try
        {
          handler(p1);
        }
        catch (RemotingException remoteEx)
        {
          LogTo.Warning(remoteEx, $"{eventName}: Remoting exception while notifying remote service - forcing unsubscribe");
          unsubscribeDelegate?.Invoke(handler);
        }
        catch (NullReferenceException)
        {
          LogTo.Warning($"Null handler called for event {eventName}.");
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, $"{eventName}: Exception while notifying remote service");
        }
    }

    #endregion
  }
}
