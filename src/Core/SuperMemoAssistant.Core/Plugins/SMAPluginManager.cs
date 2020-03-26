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
// Modified On:  2020/03/13 02:07
// Modified By:  Alexis

#endregion




using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Extensions.System.IO;
using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Uwp.Notifications;
using PluginManager;
using PluginManager.Contracts;
using PluginManager.Interop.Contracts;
using PluginManager.Logger;
using PluginManager.PackageManager.Models;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Plugins.Models;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.Sys.Windows;

// ReSharper disable RedundantTypeArgumentsOfMethod

namespace SuperMemoAssistant.Plugins
{
  using TPluginManager =
    PluginManagerBase<SMAPluginManager, PluginInstance, PluginMetadata, IPluginManager<ISuperMemoAssistant>, ISuperMemoAssistant, ISMAPlugin
    >;

  /// <inheritdoc cref="TPluginManager"/>
  public partial class SMAPluginManager : TPluginManager, IPluginLocations
  {
    #region Constants & Statics

    public static SMAPluginManager Instance { get; } = new SMAPluginManager();

    public const string ToastActionRestartAfterCrash = "PluginRestartAfterCrash";
    public const string ToastActionParameterPluginId = "PluginId";

    #endregion




    #region Properties & Fields - Non-Public

    private readonly PluginManagerLogAdapter          _logAdapter = new PluginManagerLogAdapter();
    private          DispatcherSynchronizationContext _uiSynchronizationContext;

    #endregion




    #region Constructors

    private SMAPluginManager()
    {
      Core.SMA.OnSMStartedEvent += OnSMStarted;
      Core.SMA.OnSMStoppedEvent += OnSMStopped;

      base.Initialize(false).Wait();
    }

    #endregion




    #region Methods

    private async Task OnSMStarted(object sender, SMProcessArgs e)
    {
      // ReSharper disable once AssignNullToNotNullAttribute
      _uiSynchronizationContext = new DispatcherSynchronizationContext(Application.Current.Dispatcher);

      await StartPlugins().ConfigureAwait(false);
    }

    private void OnSMStopped(object sender, SMProcessArgs e)
    {
      base.Cleanup();
    }

    /// <summary>Adds an additional handler for <see cref="SMAPluginManager" /> log output</summary>
    /// <param name="logger">The log handler</param>
    public void AddLogger(Action<string> logger) => _logAdapter.AddLogger(logger);

    /// <summary>Removes <paramref name="logger" /> from list of log output handlers</summary>
    /// <param name="logger">The log handler</param>
    public void RemoveLogger(Action<string> logger) => _logAdapter.RemoveLogger(logger);

    #endregion




    #region IPluginManagerBase

    /// <inheritdoc />
    protected override void OnPluginCrashed(PluginInstance pluginInstance)
    {
      ToastContent toastContent = new ToastContent
      {
        Visual = new ToastVisual
        {
          BindingGeneric = new ToastBindingGeneric
          {
            Children =
            {
              new AdaptiveText
              {
                Text = $"{pluginInstance.ToString().CapitalizeFirst()} has crashed."
              }
            }
          }
        },
        Actions = new ToastActionsCustom
        {
          Buttons =
          {
            // Restart action
            new ToastButton("Restart",
                            new QueryString
                            {
                              { "action", ToastActionRestartAfterCrash },
                              { ToastActionParameterPluginId, pluginInstance.Package.Id }
                            }.ToString())
            {
              ActivationType = ToastActivationType.Background
            },

            // Open logs folder action
            new ToastButton("Open the logs folder", SMAFileSystem.LogDir.FullPathWin)
            {
              ActivationType = ToastActivationType.Protocol
            }
          }
        }
      };

      var doc = new XmlDocument();
      doc.LoadXml(toastContent.GetContent());

      // And create the toast notification
      var toast = new ToastNotification(doc);

      // And then show it
      DesktopNotificationManager.CreateToastNotifier().Show(toast);
    }

    /// <inheritdoc />
    public override string GetPluginHostTypeAssemblyName(PluginInstance pluginInstance)
    {
      return "SuperMemoAssistant.Interop";
    }

    /// <inheritdoc />
    public override string GetPluginHostTypeQualifiedName(PluginInstance pluginInstance)
    {
      return "SuperMemoAssistant.Interop.Plugins.PluginHost";
    }

    /// <inheritdoc />
    public override ISuperMemoAssistant GetCoreInstance()
    {
      return Core.SMA;
    }

    /// <inheritdoc />
    public override PluginInstance CreatePluginInstance(LocalPluginPackage<PluginMetadata> package)
    {
      return new PluginInstance(package);
    }

    /// <inheritdoc />
    public override PluginMetadata CreateDevMetadata(string packageName, FileVersionInfo fileVersionInfo)
    {
      return new PluginMetadata
      {
        Enabled       = true,
        DisplayName   = fileVersionInfo.ProductName,
        PackageName   = packageName,
        Description   = "Development plugin",
        IsDevelopment = true,
      };
    }

    /// <inheritdoc />
    public override IPluginLocations Locations => this;

    /// <inheritdoc />
    public override ILogAdapter LogAdapter => _logAdapter;

    /// <inheritdoc />
    public override SynchronizationContext UISynchronizationContext => _uiSynchronizationContext;

    #endregion




    #region IPluginLocations

    /// <inheritdoc />
    public DirectoryPath PluginDir => SMAFileSystem.PluginDir;
    /// <inheritdoc />
    public DirectoryPath PluginHomeDir => SMAFileSystem.PluginHomeDir;
    /// <inheritdoc />
    public DirectoryPath PluginPackageDir => SMAFileSystem.PluginPackageDir;
    /// <inheritdoc />
    public DirectoryPath PluginDevelopmentDir => SMAFileSystem.PluginDevelopmentDir;
    /// <inheritdoc />
    public FilePath PluginHostExeFile => SMAFileSystem.PluginHostExeFile;
    /// <inheritdoc />
    public FilePath PluginConfigFile => SMAFileSystem.PluginConfigFile;

    #endregion
  }
}
