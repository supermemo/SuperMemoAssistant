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
// Modified On:  2019/02/25 23:19
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using Nito.AsyncEx;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Plugins.PackageManager.NuGet;
using SysProcess = System.Diagnostics.Process;

namespace SuperMemoAssistant.Plugins
{
  /// <summary>Represents a running instance of a plugin process</summary>
  public class PluginInstance
    : IEquatable<PluginInstance>, INotifyPropertyChanged
  {
    #region Constructors

    public PluginInstance(PluginPackage<PluginMetadata> package)
    {
      Package = package;
    }

    #endregion




    #region Properties & Fields - Public

    public PluginPackage<PluginMetadata> Package  { get; }
    public PluginMetadata                Metadata => Package.Metadata;

    public PluginStatus Status  { get; private set; } = PluginStatus.Stopped;
    public ISMAPlugin   Plugin  { get; private set; }
    public Guid         Guid    { get; private set; }
    public SysProcess   Process { get; set; }

    public AsyncLock             Lock           { get; } = new AsyncLock();
    public AsyncManualResetEvent ConnectedEvent { get; } = new AsyncManualResetEvent(false);

    public ConcurrentDictionary<string, string> InterfaceChannelMap { get; } = new ConcurrentDictionary<string, string>();

    public string Denomination => Metadata.IsDevelopment ? "development plugin" : "plugin";
    public bool   HasSettings  => Status == PluginStatus.Connected && Plugin != null && Plugin.HasSettings;

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

      return Equals((PluginInstance)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      return Package != null ? Package.GetHashCode() : 0;
    }

    /// <inheritdoc />
    public bool Equals(PluginInstance other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;

      return Equals(Package, other.Package);
    }

    #endregion




    #region Methods

    public Guid OnStarting()
    {
      Status = PluginStatus.Starting;

      ConnectedEvent.Reset();

      return Guid = Guid.NewGuid();
    }

    public void OnConnected(ISMAPlugin plugin)
    {
      Status = PluginStatus.Connected;
      Plugin = plugin;

      if (Metadata.IsDevelopment)
        Metadata.DisplayName = plugin.Name;

      ConnectedEvent.Set();
    }

    public void OnStopping()
    {
      Status = PluginStatus.Stopping;
    }

    public void OnStopped()
    {
      Status  = PluginStatus.Stopped;
      Process = null;
      Plugin  = null;
      Guid    = default;
    }

    public static bool operator ==(PluginInstance left,
                                   PluginInstance right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(PluginInstance left,
                                   PluginInstance right)
    {
      return !Equals(left, right);
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }

  public enum PluginStatus
  {
    Starting,
    Connected,
    Stopping,
    Stopped,
  }
}
