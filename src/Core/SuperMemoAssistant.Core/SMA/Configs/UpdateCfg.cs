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
// Modified On:  2020/03/22 17:49
// Modified By:  Alexis

#endregion




using System.ComponentModel;
using AutoMapper;
using Forge.Forms.Annotations;
using Newtonsoft.Json;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.Collections;

namespace SuperMemoAssistant.SMA.Configs
{
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
    [Field(Name                                                     = "SMA Update Channel")]
    [SelectFrom("{Binding CoreUpdateChannels.Values}", SelectionType = SelectionType.ComboBoxEditable)]
    public string CoreUpdateUrlField
    {
      get => CoreUpdateChannels.Reverse.SafeGet(CoreUpdateUrl);
      set => CoreUpdateUrl = CoreUpdateChannels.SafeGet(value) ?? GetDefaultCoreUpdateUrl();
    }

    //
    // Config only

    /// <summary>All Core update channels</summary>
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

    /// <summary>The CRC32 of the ChangeLog last displayed</summary>
    [JsonProperty]
    public string ChangeLogLastCrc32 { get; set; }

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
