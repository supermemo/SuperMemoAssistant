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
// Created On:   2019/01/19 05:11
// Modified On:  2019/01/19 05:20
// Modified By:  Alexis

#endregion




using System;
using Anotar.Serilog;
using DeviceId;
using Sentry;
using Sentry.Protocol;
using Serilog;

// ReSharper disable once CheckNamespace
namespace SuperMemoAssistant.Services.Sentry
{
  public static class SentryEx
  {
    #region Constants & Statics

    public const string Id = "https://a63c3dad9552434598dae869d2026696@sentry.io/1362046";
    private static User _user;

    #endregion




    #region Methods

    public static LoggerConfiguration LogToSentry(this LoggerConfiguration config)
    {
      return config.WriteTo.Sentry();
    }

    public static IDisposable Initialize(string releaseName)
    {
      try
      {
        _user = new User
        {
          Username = System.Security.Principal.WindowsIdentity.GetCurrent().Name,
          Id       = GetSystemFingerprint()
        };

        var ret = SentrySdk.Init(o =>
        {
#if DEBUG
          //o.Debug = true;
#endif
          o.Dsn = new Dsn(Id);
          o.Release = releaseName;
          o.BeforeSend = BeforeSend;
        });

        SentrySdk.ConfigureScope(s =>
          {
            s.User = _user;
          }
        );

        return ret;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception while initializing Sentry");
        return null;
      }
    }

    private static SentryEvent BeforeSend(SentryEvent sentryEvent)
    {
      if (sentryEvent.HasUser() == false)
        sentryEvent.User = _user;

      return sentryEvent;
    }

    private static string GetSystemFingerprint()
    {
      return new DeviceIdBuilder()
             .AddMachineName()
             .AddMacAddress()
             .AddProcessorId()
             .AddMotherboardSerialNumber()
             .ToString();
    }

    #endregion
  }
}
