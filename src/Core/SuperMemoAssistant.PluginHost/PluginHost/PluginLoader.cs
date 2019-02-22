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
// Created On:   2019/02/22 19:18
// Modified On:  2019/02/22 19:19
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using SuperMemoAssistant.Interop;

namespace SuperMemoAssistant.PluginHost
{
  public static class PluginLoader
  {
    #region Methods

    public static IDisposable Create(string  pluginPackageName,
                                     Process smaProcess,
                                     string  channelName,
                                     string  homeDir,
                                     bool    isDev)
    {
      var appDomain = CreateAppDomain(pluginPackageName, homeDir, isDev);

      return (IDisposable)appDomain.CreateInstanceAndUnwrap(HostConst.PluginHostAssemblyName, HostConst.PluginHostTypeName,
        false,
        BindingFlags.Public | BindingFlags.Instance,
        null,
        new object[] { pluginPackageName, smaProcess, channelName, isDev },
        null,
        null
      );
    }

    private static AppDomain CreateAppDomain(string packageName,
                                             string homeDir,
                                             bool   isDev)
    {
      var appDomainSetup = new AppDomainSetup
      {
        ApplicationBase = homeDir,
        PrivateBinPath  = GetAppDomainBinPath(homeDir),
      };

      var permissions = GetAppDomainPermissions(packageName, isDev);

      var appDomain = AppDomain.CreateDomain(
        HostConst.AppDomainName,
        AppDomain.CurrentDomain.Evidence,
        appDomainSetup,
        permissions
      );

      return appDomain;
    }

    private static string GetAppDomainBinPath(string homeDir)
    {
      List<string> ret = new List<string> { homeDir + '\\' };

      //if (string.IsNullOrWhiteSpace(AppDomain.CurrentDomain.SetupInformation.PrivateBinPath) == false)
      //  ret.AddRange(AppDomain.CurrentDomain.SetupInformation.PrivateBinPath.Split(';'));

      //ret.Add(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);

      return string.Join(";", ret.Select(p => p.Replace('/', '\\')));
    }

    private static PermissionSet GetAppDomainPermissions(string packageName,
                                                         bool   isDev)
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

      //
      // IO
      permissions.RemovePermission(typeof(FileIOPermission));

      // Common windows locations
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

      // Plugin packages
      if (isDev == false)
        permissions.AddPermission(new FileIOPermission(
                                    FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read,
                                    SMAFileSystem.PluginPackageDir.FullPath));

      // Config
      permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess,
                                                     SMAFileSystem.ConfigDir.Combine(packageName).FullPath));

      // Data
      permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess,
                                                     SMAFileSystem.DataDir.Combine(packageName).FullPath));

      // Home
      permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess,
                                                     AppDomain.CurrentDomain.BaseDirectory));

      return permissions;
    }

    #endregion
  }
}
