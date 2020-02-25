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
// Created On:   2020/01/23 08:17
// Modified On:  2020/02/13 14:51
// Modified By:  Alexis

#endregion




using System;
using System.IO;
using Extensions.System.IO;

namespace SuperMemoAssistant.Interop
{
  public static class SMAFileSystem
  {
    #region Constants & Statics

    public const string CollectionSMAFolder      = "sma";
    public const string CollectionElementsFolder = "elements";
    public const string CollectionSystemFolder   = "system";
    public const string ConfigsFolder            = "configs";

    public static DirectoryPath AppRootDir =>
      Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        SMAConst.Name
      );
    
    public static DirectoryPath AppDataRootDir =>
      Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        SMAConst.Name
      );

    public static DirectoryPath LogDir => AppDataRootDir.Combine("Logs");

    public static DirectoryPath ConfigDir => AppDataRootDir.Combine("Configs");

    public static DirectoryPath SharedConfigDir => ConfigDir.Combine("Shared");

    public static DirectoryPath DataDir => AppDataRootDir.Combine("Data");

    public static DirectoryPath PluginDir => AppDataRootDir.Combine("Plugins");

    public static DirectoryPath PluginPackageDir => PluginDir.Combine("Packages");

    public static DirectoryPath PluginDevelopmentDir => PluginDir.Combine("Development");

    public static DirectoryPath PluginHomeDir => PluginDir.Combine("Home");

    public static FilePath PluginConfigFile => PluginDir.CombineFile("plugins.json");

    public static FilePath PluginHostExeFile => GetAppExeFilePath(SMAConst.Assembly.PluginHostExe);

    public static FilePath InjectionLibFile => GetAppExeFilePath(SMAConst.Assembly.SMInjectionLib);

    #endregion




    #region Methods

    private static FilePath GetAppExeFilePath(string filename)
    {
      if (SMAExecutableInfo.Instance.ExecutableType == SMAExecutableType.SuperMemoAssistant)
        return SMAExecutableInfo.Instance.DirectoryPath.CombineFile(filename);

      throw new InvalidOperationException("GetAppExeFilePath getter is only available for SuperMemoAssistant");
    }

    #endregion
  }
}
