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
// Modified On:  2020/02/25 11:57
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using PluginManager.Services;
using SuperMemoAssistant.Plugins.Models;
using SuperMemoAssistant.Services.Sentry;
using SuperMemoAssistant.SMA;

namespace SuperMemoAssistant.Plugins.Services
{
  public class PluginRepositoryService : DefaultPluginRepositoryService<PluginMetadata>
  {
    #region Constants & Statics

    public static PluginRepositoryService Instance { get; } = new PluginRepositoryService();

    #endregion




    #region Constructors

    protected PluginRepositoryService() { }

    #endregion




    #region Properties Impl - Public

    public override string UpdateUrl     => Core.CoreConfig.Updates.PluginsUpdateUrl;
    public override bool   UpdateEnabled => Core.CoreConfig.Updates.EnablePluginsUpdates;

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override Dictionary<string, PluginMetadata> ToIdMetadataDictionary(List<PluginMetadata> metadatas)
    {
      return metadatas.ToDictionary(k => k.PackageName);
    }

    public override void SetHttpClientHeaders(HttpClient client)
    {
      base.SetHttpClientHeaders(client);

      client.DefaultRequestHeaders.Add("AnonymizedDeviceId", SentryEx.DeviceId);
    }

    #endregion
  }
}
