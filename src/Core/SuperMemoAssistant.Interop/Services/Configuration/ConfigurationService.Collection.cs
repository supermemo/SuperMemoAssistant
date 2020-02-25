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
// Modified On:  2020/02/10 10:47
// Modified By:  Alexis

#endregion




using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using Extensions.System.IO;

namespace SuperMemoAssistant.Services.Configuration
{
  public class CollectionConfigurationService : ConfigurationServiceBase
  {
    #region Properties & Fields - Non-Public

    private readonly SMCollection _collection;
    private readonly string       _subDir;

    #endregion




    #region Constructors

    public CollectionConfigurationService(SMCollection collection, string subDir)
    {
      _collection = collection;
      _subDir     = subDir;

      EnsureFolderExists();
    }

    public CollectionConfigurationService(SMCollection collection, ISMAPlugin plugin)
    {
      _collection = collection;
      _subDir     = plugin.AssemblyName;

      EnsureFolderExists();
    }

    #endregion




    #region Methods Impl

    protected override DirectoryPath GetDefaultConfigDirectoryPath() => _collection.GetSMAConfigsSubFolder(_subDir);

    #endregion
  }
}
