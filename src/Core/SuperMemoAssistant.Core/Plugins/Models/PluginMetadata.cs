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
// Modified On:  2020/03/12 21:13
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace SuperMemoAssistant.Plugins.Models
{
  /// <summary>Contains the metadata associated to each plugin</summary>
  public class PluginMetadata
    : IEquatable<PluginMetadata>, INotifyPropertyChanged
  {
    #region Constants & Statics

    public const string OfficialLabel = "Official";
    public const string VerifiedLabel = "Verified";

    #endregion




    #region Properties & Fields - Public

    /// <summary>Author(s) of the plugin</summary>
    public string Author { get; set; }

    /// <summary>The friendly name for the Plugin (as opposed to the package name)</summary>
    public string DisplayName { get; set; }

    /// <summary>The description for what the plugin does</summary>
    public string Description { get; set; }

    /// <summary>Whether the plugin is enabled or not</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>A base64-encoded string representing the plugin's icon</summary>
    public string IconBase64 { get; set; }

    /// <summary>
    ///   Whether this is a development plugin (as opposed to one installed from the NuGet
    ///   repository)
    /// </summary>
    public bool IsDevelopment { get; set; }

    /// <summary>The plugin's labels (e.g. "Official", "Verified")</summary>
    public IEnumerable<string> Labels { get; set; } = Array.Empty<string>();

    /// <summary>The assembly package name</summary>
    public string PackageName { get; set; }

    /// <summary>The user rating for the plugin</summary>
    public int Rating { get; set; }

    /// <summary>When was the plugin last updated</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Whether this is an official (SuperMemo.wiki) plugin</summary>
    [JsonIgnore]
    public bool IsOfficial => Labels?.Contains(OfficialLabel) ?? false;

    /// <summary>
    ///   Whether the plugin has been verified by the SuperMemo.wiki team. This is a minor
    ///   endorsement
    /// </summary>
    [JsonIgnore]
    public bool IsVerified => Labels?.Contains(VerifiedLabel) ?? false;

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType() != GetType())
        return false;

      return Equals((PluginMetadata)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      unchecked
      {
        return ((PackageName != null ? PackageName.GetHashCode() : 0) * 397) ^ IsDevelopment.GetHashCode();
      }
    }

    /// <inheritdoc />
    public bool Equals(PluginMetadata other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;

      return string.Equals(PackageName, other.PackageName) && IsDevelopment == other.IsDevelopment;
    }

    #endregion




    #region Methods

    /// <summary></summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(PluginMetadata left,
                                   PluginMetadata right)
    {
      return Equals(left, right);
    }

    /// <summary></summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(PluginMetadata left,
                                   PluginMetadata right)
    {
      return !Equals(left, right);
    }

    #endregion




    #region Events

    /// <inheritdoc />
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
