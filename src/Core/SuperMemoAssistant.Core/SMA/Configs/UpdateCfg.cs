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

#endregion




namespace SuperMemoAssistant.SMA.Configs
{
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Diagnostics.CodeAnalysis;
  using AutoMapper;
  using Forge.Forms.Annotations;
  using Newtonsoft.Json;
  using PropertyChanged;
  using Services.UI.Configuration;
  using SuperMemoAssistant.Extensions;
  using Sys.Collections;

  /// <summary>Core configuration for SMA and Plugins updates</summary>
  [Form(Mode = DefaultFields.None)]
  [Title("Update Settings",
         IsVisible = "{Env DialogHostContext}")]
  [DialogAction("cancel",
                "Cancel",
                IsCancel = true)]
  [DialogAction("save",
                "Save",
                IsDefault = true,
                Validates = true)]
  [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
  [SuppressMessage("Design", "CA1056:Uri properties should not be strings")]
  [SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
  public class UpdateCfg : CfgBase<UpdateCfg>, INotifyPropertyChanged
  {
    #region Constants & Statics

    public const string CoreStableUpdateUrl  = "https://releases.supermemo.wiki/sma/core/stable";
    public const string CoreBetaUpdateUrl    = "https://releases.supermemo.wiki/sma/core/beta";
    public const string CoreNightlyUpdateUrl = "https://releases.supermemo.wiki/sma/core/nightly";
    public const string CoreDefaultUpdateUrl = CoreBetaUpdateUrl;

    public const string CoreStableChannel  = "Stable";
    public const string CoreBetaChannel    = "Beta";
    public const string CoreNightlyChannel = "Nightly";
    public const string CoreDefaultChannel = CoreBetaChannel;

    public const string PluginsDefaultRepositoryUrl = "https://api.nuget.org/v3/index.json";
    public const string PluginsAlphaRepositoryUrl =
      "https://pkgs.dev.azure.com/accounts0054/SuperMemoAssistant/_packaging/SuperMemoAssistant-Alpha/nuget/v3/index.json";

    #endregion




    #region Constructors

    public UpdateCfg()
    {
      CoreUpdateUrl = GetDefaultCoreUpdateUrl();
    }

    #endregion




    #region Properties & Fields - Public

    /// <summary>Enable auto-updates of SMA</summary>
    [JsonProperty]
    [Field(Name = "Enable SMA Auto-Updates")]
    public bool EnableCoreUpdates { get; set; } = true;

    /// <summary>TODO: N/A at the moment</summary>
    [JsonProperty]
    public bool EnablePluginsUpdates { get; set; } = true;

    /// <summary>Proxy to display the update combo box</summary>
    [IgnoreMap]
    [Field(Name                                                    = "SMA Update Channel")]
    [SelectFrom("{Binding CoreUpdateChannels.Keys}", SelectionType = SelectionType.ComboBoxEditable)]
    public string CoreUpdateChannelField
    {
      get => CoreUpdateChannel;
      set => CoreUpdateUrl = CoreUpdateChannels.SafeRead(value) ?? GetDefaultCoreUpdateUrl();
    }

    //
    // Config only

    /// <summary>All Core update channels</summary>
    [IgnoreMap]
    [JsonProperty]
    public BiDictionary<string, string> CoreUpdateChannels { get; set; } = new BiDictionary<string, string>
    {
      { CoreStableChannel, CoreStableUpdateUrl },
      { CoreBetaChannel, CoreBetaUpdateUrl },
      { CoreNightlyChannel, CoreNightlyUpdateUrl },
    };

    /// <summary>The current URL to use for core updates</summary>
    [JsonProperty]
    public string CoreUpdateUrl { get; set; }

    /// <summary>The current URL to use for the plugin repository</summary>
    [JsonProperty]
    public string PluginsUpdateUrl { get; set; } = "https://releases.supermemo.wiki/sma/plugins/";

    /// <summary>The custom URLs to use for the plugin nuget repositories</summary>
    [JsonProperty]
    public HashSet<string> PluginsUpdateNuGetUrls { get; set; } = new HashSet<string>
    {
      PluginsDefaultRepositoryUrl,
      PluginsAlphaRepositoryUrl
    };

    /// <summary>The CRC32 of the ChangeLog last displayed</summary>
    [IgnoreMap]
    [JsonProperty]
    public string ChangeLogLastCrc32 { get; set; }

    //
    // Helpers

    [IgnoreMap]
    [JsonIgnore]
    [DependsOn(nameof(CoreUpdateUrl))]
    public string CoreUpdateChannel => CoreUpdateChannels.Reverse.SafeGet(CoreUpdateUrl) ?? CoreDefaultChannel;

    [IgnoreMap]
    [JsonIgnore]
    public bool CoreUpdateChannelIsPrerelease => CoreUpdateChannel != CoreStableChannel;

    #endregion




    #region Methods

    private string GetDefaultCoreUpdateUrl()
    {
      return CoreUpdateChannels.SafeGet(CoreDefaultChannel) ?? CoreDefaultUpdateUrl;
    }

    #endregion




    #region Events

    /// <inheritdoc />
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
