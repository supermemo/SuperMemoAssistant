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
// Created On:   2020/01/22 09:58
// Modified On:  2020/01/22 10:16
// Modified By:  Alexis

#endregion




using System;
using Anotar.Serilog;
using SuperMemoAssistant.Exceptions;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Plugins;
using SuperMemoAssistant.Services.Configuration;
using SuperMemoAssistant.Services.IO.HotKeys;
using SuperMemoAssistant.Services.IO.Keyboard;
using SuperMemoAssistant.Services.IO.Logger;
using SuperMemoAssistant.Services.Sentry;
using SuperMemoAssistant.SuperMemo.Common.Content.Layout;

// ReSharper disable NotAccessedVariable
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable RedundantAssignment

namespace SuperMemoAssistant
{
  public static class ModuleInitializer
  {
    #region Constants & Statics

    public static IDisposable SentryInstance { get; private set; }

    #endregion




    #region Methods

    public static void Initialize()
    {
      try
      {
        // Required for logging
        SuperMemoAssistant.SMA.Core.SharedConfiguration = new ConfigurationService(SMAFileSystem.SharedConfigDir);

        SuperMemoAssistant.SMA.Core.Logger = LoggerFactory.Create(SMAConst.Name, SuperMemoAssistant.SMA.Core.SharedConfiguration, Services.Sentry.SentryEx.LogToSentry);

#pragma warning disable CS0436 // Type conflicts with imported type
        Logger.ReloadAnotarLogger(typeof(SuperMemoAssistant.ModuleInitializer));
#pragma warning restore CS0436 // Type conflicts with imported type
        
        // ReSharper disable once RedundantNameQualifier
        var appType = typeof(SuperMemoAssistant.App);
        SuperMemoAssistant.SMA.Core.SMAVersion = appType.GetAssemblyVersion();

        var releaseName = $"SuperMemoAssistant@{SuperMemoAssistant.SMA.Core.SMAVersion}";

        SentryInstance = SentryEx.Initialize("https://a63c3dad9552434598dae869d2026696@sentry.io/1362046", releaseName);

        SuperMemoAssistant.SMA.Core.Configuration  = new ConfigurationService(SMAFileSystem.ConfigDir.Combine("Core"));
        SuperMemoAssistant.SMA.Core.KeyboardHotKey = KeyboardHookService.Instance;
        SuperMemoAssistant.SMA.Core.HotKeyManager  = HotKeyManager.Instance.Initialize(SuperMemoAssistant.SMA.Core.Configuration, SuperMemoAssistant.SMA.Core.KeyboardHotKey);
        SuperMemoAssistant.SMA.Core.SMA            = new SMA.SMA();

        object tmp;
        tmp = LayoutManager.Instance;
        tmp = SMAPluginManager.Instance;
      }
      catch (SMAException ex)
      {
        LogTo.Warning(ex, "Error during SuperMemoAssistant Initialize().");
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception thrown during SuperMemoAssistant module Initialize().");
      }
    }

    #endregion
  }
}
