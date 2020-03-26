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
// Modified On:  2020/02/27 15:53
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Anotar.Serilog;
using PluginManager.PackageManager;
using PluginManager.PackageManager.Models;
using PluginManager.Services;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins.Models;
using SuperMemoAssistant.Services.Sentry;
using SuperMemoAssistant.SMA;

// ReSharper disable PossibleMultipleEnumeration

namespace SuperMemoAssistant.Plugins.Services
{
  /// <inheritdoc />
  public class PluginRepositoryService : DefaultPluginRepositoryService<PluginMetadata>
  {
    #region Constants & Statics

    public static PluginRepositoryService Instance { get; } = new PluginRepositoryService();

    #endregion




    #region Properties & Fields - Non-Public

    private Dictionary<string, (DateTime createdAt, IEnumerable<PluginPackage<PluginMetadata>> plugins)> SearchCacheMap { get; }

    #endregion




    #region Constructors

    protected PluginRepositoryService()
    {
      SearchCacheMap = new Dictionary<string, (DateTime, IEnumerable<PluginPackage<PluginMetadata>>)>();
    }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public override string UpdateUrl => Core.CoreConfig.Updates.PluginsUpdateUrl;

    /// <inheritdoc />
    public override bool UpdateEnabled => Core.CoreConfig.Updates.EnablePluginsUpdates;

    #endregion




    #region Methods Impl
    
    /// <inheritdoc />
    public override async Task<List<PluginMetadata>> FetchPluginMetadataList(CancellationToken cancellationToken = default)
    {
      var metadatas = await base.FetchPluginMetadataList(cancellationToken).ConfigureAwait(false);

      if (metadatas == null)
        return null;

      var idMetadataMap = metadatas.ToDictionary(GetPackageIdFromMetadata);

      Application.Current.Dispatcher.Invoke(() =>
      {
        foreach (var pluginInst in SMAPluginManager.Instance.AllPlugins.Where(p => p.IsDevelopment == false))
        {
          var newMeta = idMetadataMap.SafeGet(pluginInst.Package.Id);

          if (newMeta == null)
            continue;

          pluginInst.Metadata.Author      = newMeta.Author;
          pluginInst.Metadata.DisplayName = newMeta.DisplayName;
          pluginInst.Metadata.Description = newMeta.Description;
          pluginInst.Metadata.IconBase64  = newMeta.IconBase64;
          pluginInst.Metadata.Labels      = newMeta.Labels;
          pluginInst.Metadata.Rating      = newMeta.Rating;
          pluginInst.Metadata.UpdatedAt   = newMeta.UpdatedAt;
        }
      });

      if (await SMAPluginManager.Instance.SaveConfigAsync().ConfigureAwait(false) == false)
        LogTo.Warning("Failed to save the local plugin repository file after updating the plugins' metadata");

      return metadatas;
    }

    /// <inheritdoc />
    protected override string GetPackageIdFromMetadata(PluginMetadata metadata)
    {
      return metadata.PackageName;
    }

    /// <inheritdoc />
    protected override void SetHttpClientHeaders(HttpClient client)
    {
      base.SetHttpClientHeaders(client);

      client.DefaultRequestHeaders.Add("AnonymizedDeviceId", SentryEx.DeviceId);
    }

    #endregion




    #region Methods

    /// <summary>
    ///   Search available NuGet repositories for all packages matching
    ///   <paramref name="searchTerm" /> and <paramref name="enablePrerelease" />. Only NuGet packages
    ///   that are also indexed by the API pointed to by <see cref="UpdateUrl" /> will be included.
    /// </summary>
    /// <param name="searchTerm">Part or totality of the package name to look for</param>
    /// <param name="enablePrerelease">Whether to include packages that are marked as pre-release</param>
    /// <param name="packageManager">The package manager</param>
    /// <param name="forceRefresh">Whether to invalidate cache or not</param>
    /// <param name="expireAfterSec">Delay after which a cached search is considered expired</param>
    /// <param name="cancellationToken"></param>
    /// <returns>All available packages or <see langword="null" /></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<IEnumerable<PluginPackage<PluginMetadata>>> SearchPlugins(
      string                               searchTerm,
      bool                                 enablePrerelease,
      PluginPackageManager<PluginMetadata> packageManager,
      bool                                 forceRefresh      = false,
      int                                  expireAfterSec    = 1800,
      CancellationToken                    cancellationToken = default)
    {
      var cachedSearch = SearchCacheMap.SafeGet(searchTerm);

      if (forceRefresh == false && cachedSearch != default && IsCacheExpired(cachedSearch.createdAt, expireAfterSec) == false)
        return cachedSearch.plugins;

      ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
      ServicePointManager.SecurityProtocol                    =  SecurityProtocolType.Tls12;

      var plugins = await SearchPlugins(searchTerm, enablePrerelease, packageManager, cancellationToken).ConfigureAwait(false);

      SearchCacheMap[searchTerm] = (DateTime.Now, plugins);

      return plugins;
    }

    /// <summary>Checks whether <paramref name="createdAt" /> is considered expired</summary>
    /// <param name="createdAt">The DateTime at which the cache entry was created</param>
    /// <param name="expireAfterSec">Delay after which a cached entry is considered expired</param>
    /// <returns>whether <paramref name="createdAt" /> is considered expired</returns>
    private static bool IsCacheExpired(DateTime createdAt, int expireAfterSec)
    {
      return createdAt.AddSeconds(expireAfterSec) < DateTime.Now;
    }

    #endregion
  }
}
