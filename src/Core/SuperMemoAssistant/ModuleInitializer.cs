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
// Created On:   2020/03/29 00:20
// Modified On:  2020/04/10 14:13
// Modified By:  Alexis

#endregion






// ReSharper disable NotAccessedVariable
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable RedundantAssignment

namespace SuperMemoAssistant
{
  using System;
  using System.IO;
  using Anotar.Serilog;
  using Exceptions;
  using Extensions;
  using Interop;
  using Plugins;
  using Services.Configuration;
  using Services.IO.Diagnostics;
  using Services.IO.HotKeys;
  using Services.IO.Keyboard;
  using Services.Sentry;
  using SuperMemo.Common.Content.Layout;

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
        SMA.Core.SharedConfiguration = new ConfigurationService(SMAFileSystem.SharedConfigDir);

        SMA.Core.Logger = LoggerFactory.Create(SMAConst.Name, SMA.Core.SharedConfiguration, SentryEx.LogToSentry);

#pragma warning disable CS0436 // Type conflicts with imported type
        Logger.ReloadAnotarLogger(typeof(ModuleInitializer));
#pragma warning restore CS0436 // Type conflicts with imported type

        // ReSharper disable once RedundantNameQualifier
        var appType = typeof(SuperMemoAssistant.App);
        SMA.Core.SMAVersion = appType.GetAssemblyVersion();

        var releaseName = $"SuperMemoAssistant@{SMA.Core.SMAVersion}";

        SentryInstance = SentryEx.Initialize("https://a63c3dad9552434598dae869d2026696@sentry.io/1362046", releaseName);

        SMA.Core.Configuration  = new ConfigurationService(SMAFileSystem.ConfigDir.Combine("Core"));
        SMA.Core.KeyboardHotKey = KeyboardHookService.Instance;
        SMA.Core.HotKeyManager  = HotKeyManager.Instance.Initialize(SMA.Core.Configuration, SMA.Core.KeyboardHotKey);
        SMA.Core.SMA            = new SMA.SMA();

        object tmp;
        tmp = LayoutManager.Instance;
        tmp = SMAPluginManager.Instance;
      }
      catch (SMAException ex)
      {
        LogTo.Warning(ex, "Error during SuperMemoAssistant Initialize().");
        File.WriteAllText(SMAFileSystem.TempErrorLog.FullPath, $"Error during SuperMemoAssistant Initialize(): {ex}");
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception thrown during SuperMemoAssistant module Initialize().");
        File.WriteAllText(SMAFileSystem.TempErrorLog.FullPath, $"Exception thrown during SuperMemoAssistant Initialize(): {ex}");
      }
    }

    #endregion
  }
}
