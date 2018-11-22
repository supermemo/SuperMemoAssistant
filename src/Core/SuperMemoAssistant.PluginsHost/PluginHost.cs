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
// Created On:   2018/05/31 13:44
// Modified On:  2018/11/17 02:10
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.PluginsHost.Services.Devices;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.IO.Devices;
using SuperMemoAssistant.Sys;

// ReSharper disable PossibleMultipleEnumeration

namespace SuperMemoAssistant.PluginsHost
{
  public class PluginHost : SMMarshalByRefObject, IDisposable
  {
    #region Constants & Statics

    private const string AppDomainName = "PluginsHost_AppDomain";

    public static PluginHost Instance { get; private set; }

    #endregion




    #region Properties & Fields - Non-Public

    private   IEnumerable<ISMAPlugin>              _plugins;
    protected Dictionary<string, DirectoryCatalog> DirectoryCatalogs { get; set; }

    #endregion




    #region Constructors

    public PluginHost()
    {
      Instance = this;
    }

    /// <inheritdoc />
    public void Dispose()
    {
      try
      {
        SystemDb.Instance.Dispose();

        foreach (var plugin in Plugins)
          try
          {
            plugin.Dispose();
          }
          catch (Exception ex)
          {
            // TODO: Log
          }
      }
      catch (Exception ex)
      {
        // TODO: Log
      }

      Container.Dispose();
    }

    #endregion




    #region Properties & Fields - Public

    public CompositionContainer Container { get; private set; }

    public IEnumerable<ISMAPlugin> Plugins => _plugins ?? (_plugins = BuildPlugins());

    #endregion




    #region Methods

    public static T Get<T>() => Instance.Container.GetExportedValue<T>();

    public void Setup()
    {
      var pluginRegBuilder = new RegistrationBuilder();
      pluginRegBuilder.ForTypesMatching(t => t.IsAbstract == false
                                          && typeof(ISMAPlugin).IsAssignableFrom(t)).Export<ISMAPlugin>();
      //pluginRegBuilder.ForTypesDerivedFrom<ISMAPlugin>().Export<ISMAPlugin>();

      DirectoryCatalogs = GenerateDirectoryCatalogs(pluginRegBuilder);
      var assemblyCatalog = new AssemblyCatalog(typeof(PluginHost).Assembly);

      var catalog = new AggregateCatalog();
      catalog.Catalogs.Add(assemblyCatalog);

      foreach (var directoryCatalog in DirectoryCatalogs.Values)
        catalog.Catalogs.Add(directoryCatalog);

      Container = new CompositionContainer(catalog);
      Container.ComposeExportedValue<CompositionContainer>(Container);
      Container.ComposeExportedValue<IKeyboardHotKeyService>(KeyboardHotKeyService.Instance);

      SetupApplication();
    }

    public void PostSetup()
    {
      SetupServices();
    }

    private void SetupServices()
    {
      Svc.SMA = Get<ISuperMemoAssistant>();
    }

    private void SetupApplication()
    {
      new System.Windows.Application()
      {
        ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown
      };
    }

    public void Recompose()
    {
      foreach (var directoryCatalog in DirectoryCatalogs.Values)
      {
        directoryCatalog.Refresh();
        Container.ComposeParts(directoryCatalog.Parts);
      }

      _plugins = BuildPlugins();
    }

    public void Reload(string guid) { }

    public void Export<T>(T instance)
    {
      Container.ComposeExportedValue<T>(instance);
    }

    private IEnumerable<ISMAPlugin> BuildPlugins()
    {
      var plugins = Container.GetExportedValues<ISMAPlugin>();

      return plugins;
    }

    public static (AppDomain domain, PluginHost runner) Create(SMCollection collection)
    {
      DirectoryEx.EnsureExists(SMAConst.Paths.AppDomainCachePath);
      DirectoryEx.EnsureExists(SMAConst.Paths.PluginPath);
      DirectoryEx.EnsureExists(collection.GetSMAFolder());
      DirectoryEx.EnsureExists(collection.GetSMAElementsFolder());
      DirectoryEx.EnsureExists(collection.GetSMAPluginsFolder());
      DirectoryEx.EnsureExists(collection.GetSMASystemFolder());

      var assemblyPaths = String.Join(";",
                                      GetAssemblyPaths());

      var setup = new AppDomainSetup()
      {
        ApplicationBase       = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
        CachePath             = SMAConst.Paths.AppDomainCachePath,
        PrivateBinPath        = assemblyPaths,
        ShadowCopyFiles       = "true",
        ShadowCopyDirectories = assemblyPaths
      };

      var permissions = GetPermissions(collection);

      var domain = AppDomain.CreateDomain(
        AppDomainName,
        AppDomain.CurrentDomain.Evidence,
        setup,
        permissions
      );

      var runner = (PluginHost)domain.CreateInstanceAndUnwrap(
        typeof(PluginHost).Assembly.FullName,
        // ReSharper disable once AssignNullToNotNullAttribute
        typeof(PluginHost).FullName
      );

      return (domain, runner);
    }

    private static Dictionary<string, DirectoryCatalog> GenerateDirectoryCatalogs(RegistrationBuilder regBuilder)
    {
      return GetPluginsPath()
             .Select(p =>
             {
               var guid = PathEx.GetLastSegment(p);
               return (guid, new DirectoryCatalog(p,
                                                  regBuilder));
             })
             .ToDictionary(k => k.Item1,
                           v => v.Item2);
    }

    private static List<string> GetAssemblyPaths()
    {
      var ret = GetPluginsPath();

      if (String.IsNullOrWhiteSpace(AppDomain.CurrentDomain.SetupInformation.PrivateBinPath) == false)
        ret.AddRange(AppDomain.CurrentDomain.SetupInformation.PrivateBinPath.Split(';'));

      ret.Add(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);

      return ret;
    }

    private static List<String> GetPluginsPath()
    {
      return Directory.GetDirectories(SMAConst.Paths.PluginPath).ToList();
    }

    private static PermissionSet GetPermissions(SMCollection collection)
    {
      // TODO: Switch back to restricted
      var permissions = new PermissionSet(PermissionState.Unrestricted);

      //permissions.SetPermission(new EnvironmentPermission(PermissionState.Unrestricted));
      //permissions.SetPermission(new UIPermission(PermissionState.Unrestricted));
      //permissions.SetPermission(new FileDialogPermission(PermissionState.Unrestricted));
      //permissions.SetPermission(new MediaPermission(PermissionState.Unrestricted));
      //permissions.SetPermission(new ReflectionPermission(PermissionState.Unrestricted));

      /*permissions.SetPermission(
        new SecurityPermission(SecurityPermissionFlag.AllFlags));
          SecurityPermissionFlag.Execution | SecurityPermissionFlag.UnmanagedCode | SecurityPermissionFlag.BindingRedirects
          | SecurityPermissionFlag.Assertion | SecurityPermissionFlag.RemotingConfiguration | SecurityPermissionFlag.ControlThread));*/

      permissions.RemovePermission(typeof(FileIOPermission));
      permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess,
                                                     Path.GetTempPath()));
      permissions.AddPermission(new FileIOPermission(
                                  FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read,
                                  Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles))
      );
      permissions.AddPermission(new FileIOPermission(
                                  FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read,
                                  Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86))
      );
      permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess,
                                                     collection.GetSMAFolder()));
      permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess,
                                                     SMAConst.Paths.AppDataPath));
      permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess,
                                                     AppDomain.CurrentDomain.BaseDirectory));

      return permissions;
    }


    #endregion
  }
}
