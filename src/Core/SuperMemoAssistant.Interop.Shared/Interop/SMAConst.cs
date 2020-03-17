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
// Modified On:  2020/03/15 17:16
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.Interop
{
  public static class SMAConst
  {
    #region Constants & Statics

    /// <summary>The app name</summary>
    public const string Name = "SuperMemoAssistant";

    /// <summary>Name SMA plugins assemblies' names should always be prefixed with</summary>
    public const string SuperMemoPluginPackagePrefix = "SuperMemoAssistant.Plugins.";

    /// <summary>The change log file name</summary>
    public const string ChangeLogFileName = "ChangeLog";

    #endregion




    /// <summary>Assemblies data</summary>
    public static class Assembly
    {
      #region Constants & Statics

      /// <summary>SMA executable file name</summary>
      public const string SuperMemoAssistantExe = "SuperMemoAssistant.exe";

      /// <summary>SMA plugin host executable file name</summary>
      public const string PluginHostExe = "PluginHost.exe";

      /// <summary>SuperMemo injection library</summary>
      public const string SMInjectionLib = "SuperMemoAssistant.Hooks.InjectLib.dll";

      #endregion
    }
  }
}
