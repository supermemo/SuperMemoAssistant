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
// Created On:   2018/05/12 01:38
// Modified On:  2018/06/06 03:48
// Modified By:  Alexis

#endregion




using System;
using System.IO;
using System.Windows.Forms;

namespace SuperMemoAssistant.Interop
{
  public static class SMAConst
  {
    #region Constants & Statics

    public const string Name = "SuperMemoAssistant";

    #endregion




    public static class Paths
    {
      #region Constants & Statics

      public const string CollectionSMAFolder      = "sma";
      public const string CollectionElementsFolder = "elements";
      public const string CollectionPluginsFolder  = "plugins";
      public const string CollectionSystemFolder   = "system";

      public static string AppDataPath =>
        Path.Combine(
          Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
          Name
        );

      public static string PluginPath =>
        Path.Combine(
          AppDataPath,
          "Plugins"
        );
      public static string AppDomainCachePath =>
        Path.Combine(
          AppDataPath,
          "AppDomainCache"
        );

      public static string ConfigPath =>
        Path.Combine(
          AppDataPath,
          "Configs"
        );

      #endregion
    }

    public static class Files
    {
      #region Constants & Statics

      public const string CollectionSystemDbFileName       = "sma_system.litedb";
      public const string CollectionPluginDatabaseFileName = "plugin.litedb";

      #endregion
    }

    public static class Assembly
    {
      #region Constants & Statics

      public const string SMInjectionLib = "SuperMemoAssistant.Hooks.InjectLib.dll";

      #endregion




      #region Methods

      public static string GetInjectionLibFilePath()
      {
        return Path.Combine(
          Application.StartupPath,
          SMInjectionLib
        );
      }

      #endregion
    }
  }
}
