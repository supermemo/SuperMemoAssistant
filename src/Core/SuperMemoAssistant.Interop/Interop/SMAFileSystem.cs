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
// Created On:   2019/01/21 14:16
// Modified On:  2019/01/21 14:18
// Modified By:  Alexis

#endregion




using System;
using System.IO;
using SuperMemoAssistant.Interop.Plugins;

namespace SuperMemoAssistant.Interop
{
  public static class SMAFileSystem
  {
    #region Constants & Statics

    public const string CollectionSMAFolder      = "sma";
    public const string CollectionElementsFolder = "elements";
    public const string CollectionPluginsFolder  = "plugins";
    public const string CollectionSystemFolder   = "system";

    public static string AppRootPath =>
      Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        SMAConst.Name
      );

    public static string ConfigPath =>
      Path.Combine(AppRootPath,
                   "Configs"
      );

    public static string DataPath =>
      Path.Combine(AppRootPath,
                   "Data"
      );

    public static string AppDomainCachePath =>
      Path.Combine(AppRootPath,
                   "AppDomainCache"
      );

    public static string PluginPath =>
      Path.Combine(AppRootPath,
                   "Plugins"
      );

    public static string PluginPackagePath =>
      Path.Combine(PluginPath,
                   "Packages"
      );

    public static string PluginConfigPath =>
      Path.Combine(PluginPath,
                   "plugins.json"
      );

    public static string MakePluginDataPath(ISMAPlugin plugin) =>
      Path.Combine(DataPath, plugin.Name);

    #endregion
  }
}
