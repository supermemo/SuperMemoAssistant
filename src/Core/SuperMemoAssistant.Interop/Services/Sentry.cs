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
// Created On:   2018/12/30 14:34
// Modified On:  2018/12/30 14:36
// Modified By:  Alexis

#endregion




using System;
using DeviceId;
using Sentry;
using Sentry.Protocol;

namespace SuperMemoAssistant.Services
{
  public static class Sentry
  {
    public const string Id = "https://a63c3dad9552434598dae869d2026696@sentry.io/1362046";
    #region Methods

    public static IDisposable Initialize()
    {
      var ret = SentrySdk.Init(Id);

      SentrySdk.ConfigureScope(s =>
        {
          s.User = new User
          {
            Username = System.Security.Principal.WindowsIdentity.GetCurrent().Name,
            Id       = GetSystemFingerprint()
          };
        }
      );

      return ret;
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
