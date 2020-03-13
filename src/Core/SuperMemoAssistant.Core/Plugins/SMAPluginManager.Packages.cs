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
// Modified On:  2020/03/02 11:10
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PluginManager.PackageManager.Models;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Plugins.Models;
using SuperMemoAssistant.Plugins.Services;

namespace SuperMemoAssistant.Plugins
{
  public partial class SMAPluginManager
  {
    #region Methods

    /// <summary>
    ///   Search available NuGet repositories for all packages matching
    ///   <paramref name="searchTerm" /> and <paramref name="enablePrerelease" />. Only NuGet packages
    ///   that are also listed on the <see cref="PluginRepositoryService" /> will be included.
    /// </summary>
    /// <param name="searchTerm">Part or totality of the package name to look for</param>
    /// <param name="enablePrerelease">Whether to include packages that are marked as pre-release</param>
    /// <param name="forceRefresh">Whether to invalidate cache or not</param>
    /// <param name="expireAfterSec">Delay after which a cached search is considered expired</param>
    /// <param name="cancellationToken"></param>
    /// <returns>All available packages or <see langword="null" /></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<IEnumerable<PluginPackage<PluginMetadata>>> SearchPlugins(
      string            searchTerm        = SMAConst.SuperMemoPluginPackagePrefix,
      bool              enablePrerelease  = false,
      bool              forceRefresh      = false,
      int               expireAfterSec    = 1800,
      CancellationToken cancellationToken = default)
    {
      return await PluginRepositoryService
                   .Instance.SearchPlugins(searchTerm, enablePrerelease, PackageManager, forceRefresh, expireAfterSec, cancellationToken)
                   .ConfigureAwait(false);
    }
    
    /// <summary>Saves the local plugin repository state to file</summary>
    /// <returns>Success of operation</returns>
    public Task<bool> SaveConfigAsync()
    {
      return PackageManager.SaveConfigAsync();
    }

    #endregion
  }
}
