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
// Created On:   2018/05/30 12:47
// Modified On:  2019/01/25 16:39
// Modified By:  Alexis

#endregion




using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Anotar.Serilog;
using Newtonsoft.Json;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Plugins.PackageManager;
using SuperMemoAssistant.Plugins.PackageManager.NuGet;
using SuperMemoAssistant.Services.IO;
using SuperMemoAssistant.SuperMemo;

// ReSharper disable RedundantTypeArgumentsOfMethod

namespace SuperMemoAssistant.Plugins
{
  public class PluginManager : IDisposable
  {
    #region Constants & Statics

    public static PluginManager Instance { get; } = new PluginManager();

    #endregion




    #region Constructors

    protected PluginManager()
    {
      SMA.Instance.OnSMStartedEvent += OnSMStarted;
      SMA.Instance.OnSMStoppedEvent += OnSMStopped;
    }

    /// <inheritdoc />
    public void Dispose()
    {
      UnloadPlugins();
    }

    #endregion




    #region Methods

    private void OnSMStarted(object        sender,
                             SMProcessArgs e)
    {
      LoadPlugins();
    }

    private void OnSMStopped(object        sender,
                             SMProcessArgs e)
    {
      UnloadPlugins();
    }

    protected void LoadPlugins()
    {
      DirectoryEx.EnsureExists(SMAFileSystem.PluginPackagePath);
      var oldLvl = Logger.Instance.SetMinimumLevel(Serilog.Events.LogEventLevel.Verbose);


      Task.Run(
        async () =>
        {
          var cancelSrc = new CancellationTokenSource();

          var pm = new PluginPackageManager<PluginMetadata>(
            SMAFileSystem.PluginPath,
            SMAFileSystem.PluginPackagePath,
            SMAFileSystem.PluginConfigPath,
            s => new NuGetSourceRepositoryProvider(s));

          //ModuleInit.Fody
          //PropertyChanged.Fody

          bool r1 = await pm.InstallPluginAsync(
            "ModuleInit.Fody",
            new PluginMetadata { Name = "ModuleInit.Fody", Author = "Simon " },
            cancellationToken: cancelSrc.Token);

          bool r2 = await pm.InstallPluginAsync(
            "PropertyChanged.Fody",
            new PluginMetadata { Name = "PropertyChanged.Fody", Author = "Simon " },
            cancellationToken: cancelSrc.Token);

          await pm.UninstallPluginAsync(
            "PropertyChanged.Fody",
            cancellationToken: cancelSrc.Token);

          LogTo.Information($"Results: {r1} {r2}");
        }
      ).Wait();

      //foreach (var plugin in Runner.Plugins)
      //LogTo.Debug($"[PluginMgr] Loaded plugin {plugin.Name} ({plugin.Version})");

      Logger.Instance.SetMinimumLevel(oldLvl);
    }

    protected void UnloadPlugins() { }

    public void ReloadPlugins()
    {
      UnloadPlugins();
      LoadPlugins();
    }

    #endregion
  }

  public class PluginMetadata
  {
    #region Properties & Fields - Public

    public bool     Enabled     { get; set; }
    public string   Name        { get; set; }
    public string   Description { get; set; }
    public string   Author      { get; set; }
    public DateTime UpdatedAt   { get; set; }
    [JsonIgnore]
    public int Rating { get;        set; }
    public string IconBase64 { get; set; }

    [JsonIgnore]
    public Image Icon => IconBase64 == null
      ? null
      : ImageEx.FromBase64(IconBase64);

    #endregion
  }
}
