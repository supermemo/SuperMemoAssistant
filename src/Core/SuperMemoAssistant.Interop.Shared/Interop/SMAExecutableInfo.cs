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
// Created On:   2020/02/13 00:48
// Modified On:  2020/02/13 14:37
// Modified By:  Alexis

#endregion




using System.Reflection;
using System.Text.RegularExpressions;
using Extensions.System.IO;

namespace SuperMemoAssistant.Interop
{
  public class SMAExecutableInfo
  {
    #region Constants & Statics

    private const string EntryAssemblyRegexPattern = @"/app-([\d.]+|dev)/(SuperMemoAssistant(?:\.PluginHost)?.exe)";

    public static SMAExecutableInfo Instance { get; } = new SMAExecutableInfo();

    #endregion




    #region Constructors

    protected SMAExecutableInfo()
    {
      var entryAssemblyFilePath = new FilePath(Assembly.GetEntryAssembly().Location);
      var regexPattern          = SMAFileSystem.AppRootDir.FullPath + EntryAssemblyRegexPattern;
      var regex                 = new Regex(regexPattern);
      var match                 = regex.Match(entryAssemblyFilePath.FullPath);

      DirectoryPath = entryAssemblyFilePath.Directory;

      if (match.Success)
      {
        IsPathLocalAppData = true;

        if (match.Groups.Count == 3)
        {
          IsDev = match.Groups[1].Value == "dev";

          switch (match.Groups[2].Value)
          {
            case SMAConst.Assembly.SuperMemoAssistantExe:
              ExecutableType = SMAExecutableType.SuperMemoAssistant;
              break;

            case SMAConst.Assembly.PluginHostExe:
              ExecutableType = SMAExecutableType.PluginHost;
              break;
          }
        }
      }
    }

    #endregion




    #region Properties & Fields - Public

    public DirectoryPath     DirectoryPath      { get; }
    public SMAExecutableType ExecutableType     { get; } = SMAExecutableType.Unknown;
    public bool              IsDev              { get; } = false;
    public bool              IsPathLocalAppData { get; } = false;

    #endregion
  }

  public enum SMAExecutableType
  {
    Unknown,
    SuperMemoAssistant,
    PluginHost,
  }
}
