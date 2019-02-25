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
// Created On:   2019/01/20 08:05
// Modified On:  2019/01/21 19:36
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace SuperMemoAssistant.Plugins.PackageManager.NuGet
{
  public class NuGetSourceRepositoryProvider : SourceRepositoryProvider
  {
    #region Properties & Fields - Non-Public

    private static readonly IEnumerable<string> _defaultSources = new List<string>
    {
      "https://api.nuget.org/v3/index.json"
    };

    #endregion




    #region Constructors

    /// <inheritdoc />
    public NuGetSourceRepositoryProvider(ISettings settings) : base(settings, _defaultSources) { }

    #endregion
  }

  /// <summary>
  ///   Creates and caches SourceRepository objects, which are the combination of
  ///   PackageSource instances with a set of supported resource providers. It also manages the set
  ///   of default source repositories. Original from: https://github.com/Wyamio/Wyam/ Copyright (c)
  ///   2014 Dave Glick
  /// </summary>
  public class SourceRepositoryProvider : ISourceRepositoryProvider
  {
    #region Properties & Fields - Non-Public
    
    private readonly ConcurrentDictionary<PackageSource, SourceRepository> _repositoryCache
      = new ConcurrentDictionary<PackageSource, SourceRepository>();

    private readonly List<Lazy<INuGetResourceProvider>> _resourceProviders;

    #endregion




    #region Constructors

    public SourceRepositoryProvider(ISettings           settings,
                                    IEnumerable<string> defaultSources = null)
    {
      // Create the package source provider (needed primarily to get default sources)
      PackageSourceProvider = new PackageSourceProvider(settings);

      // Add the v3 provider as default
      _resourceProviders = new List<Lazy<INuGetResourceProvider>>();
      _resourceProviders.AddRange(Repository.Provider.GetCoreV3());

      if (defaultSources != null)
        foreach (var src in defaultSources)
          CreateRepository(src);
    }

    #endregion




    #region Properties Impl - Public

    public IPackageSourceProvider PackageSourceProvider { get; }

    #endregion




    #region Methods Impl

    /// <summary>Creates or gets a non-default source repository by PackageSource.</summary>
    public SourceRepository CreateRepository(PackageSource packageSource) => CreateRepository(packageSource, FeedType.Undefined);

    /// <summary>Creates or gets a non-default source repository by PackageSource.</summary>
    public SourceRepository CreateRepository(PackageSource packageSource,
                                             FeedType      feedType) =>
      _repositoryCache.GetOrAdd(packageSource, x => new SourceRepository(x, _resourceProviders));

    /// <summary>Gets all cached repositories.</summary>
    public IEnumerable<SourceRepository> GetRepositories() => _repositoryCache.Values;

    #endregion




    #region Methods

    /// <summary>Creates or gets a non-default source repository.</summary>
    public SourceRepository CreateRepository(string packageSource) =>
      CreateRepository(new PackageSource(packageSource));

    #endregion
  }
}
