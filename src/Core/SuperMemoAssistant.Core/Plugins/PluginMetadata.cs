using System;
using System.Drawing;
using Newtonsoft.Json;
using SuperMemoAssistant.Extensions;

namespace SuperMemoAssistant.Plugins {
  public class PluginMetadata
  {
    #region Properties & Fields - Public

    public bool     Enabled     { get; set; }
    public string   Name        { get; set; }
    public string   Description { get; set; }
    public string   Author      { get; set; }
    public DateTime UpdatedAt   { get; set; }
    [JsonIgnore]
    public int Rating { get;        set; }
    public string IconBase64 { get; set; }

    [JsonIgnore]
    public Image Icon => IconBase64 == null
      ? null
      : ImageEx.FromBase64(IconBase64);

    #endregion
  }
}