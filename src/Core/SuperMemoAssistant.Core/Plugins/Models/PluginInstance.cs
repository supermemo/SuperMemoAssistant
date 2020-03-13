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
// Modified On:  2020/02/25 16:37
// Modified By:  Alexis

#endregion




using PluginManager.Models;
using PluginManager.PackageManager.Models;
using SuperMemoAssistant.Interop.Plugins;

namespace SuperMemoAssistant.Plugins.Models
{
  public class PluginInstance : PluginInstanceBase<PluginInstance, PluginMetadata, ISMAPlugin>
  {
    #region Constructors

    /// <inheritdoc />
    public PluginInstance(LocalPluginPackage<PluginMetadata> package) : base(package) { }

    #endregion




    #region Properties & Fields - Public

    public bool HasSettings => Status == PluginStatus.Connected && Plugin != null && Plugin.HasSettings;

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public override bool IsEnabled
    {
      get => Metadata.Enabled;
      set => Metadata.Enabled = value;
    }

    public override string ToString()
    {
      return $"{Denomination} {Metadata?.DisplayName ?? Package.Id}";
    }

    #endregion




    #region Methods Impl

    public override void OnConnected(ISMAPlugin plugin)
    {
      if (IsDevelopment)
        Metadata.DisplayName = plugin.Name;

      base.OnConnected(plugin);

      OnPropertyChanged(nameof(HasSettings));
    }

    public override void OnStopped(bool crashed)
    {
      base.OnStopped(crashed);

      OnPropertyChanged(nameof(HasSettings));
    }

    /// <inheritdoc />
    public override bool Equals(PluginInstance other)
    {
      return Equals((PluginInstanceBase<PluginInstance, PluginMetadata, ISMAPlugin>)other);
    }

    #endregion
  }
}
