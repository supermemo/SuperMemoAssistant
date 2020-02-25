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
// Created On:   2019/02/25 22:02
// Modified On:  2019/02/25 23:20
// Modified By:  Alexis

#endregion




using System;
using System.ComponentModel;
using System.Drawing;
using Newtonsoft.Json;
using SuperMemoAssistant.Extensions;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace SuperMemoAssistant.Plugins.Models
{
  public class PluginMetadata
    : IEquatable<PluginMetadata>, INotifyPropertyChanged
  {
    #region Properties & Fields - Public

    public bool     Enabled       { get; set; }
    public string   DisplayName   { get; set; }
    public string   PackageName   { get; set; }
    public string   Description   { get; set; }
    public string   Author        { get; set; }
    public DateTime UpdatedAt     { get; set; }
    public string   IconBase64    { get; set; }
    public bool     IsDevelopment { get; set; }

    [JsonIgnore]
    public int Rating { get; set; }

    [JsonIgnore]
    public Image Icon => IconBase64 == null
      ? null
      : ImageEx.FromBase64(IconBase64);

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

    public static bool operator ==(PluginMetadata left,
                                   PluginMetadata right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(PluginMetadata left,
                                   PluginMetadata right)
    {
      return !Equals(left, right);
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
