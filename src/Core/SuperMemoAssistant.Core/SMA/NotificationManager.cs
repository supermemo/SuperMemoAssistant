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




namespace SuperMemoAssistant.SMA
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using Windows.Data.Xml.Dom;
  using Windows.UI.Notifications;
  using Anotar.Serilog;
  using Extensions;
  using Interop.SMA.Notifications;
  using Microsoft.QueryStringDotNET;
  using Plugins;
  using Sys.Windows;

  public class NotificationManager : INotificationManager
  {
    #region Constants & Statics

    public static NotificationManager Instance { get; } = new NotificationManager();

    public const string PluginSessionGuidArgName = "pluginSessionGuid";

    private const string ToastNodeName           = "toast";
    private const string ActionNodeName          = "action";
    private const string ActionArgumentsAttrName = "arguments";
    private const string ToastLaunchAttrName     = "launch";

    #endregion




    #region Constructors

    protected NotificationManager() { }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
    public bool ShowDesktopNotification(string toastXml, Guid pluginSessionGuid)
    {
      try
      {
        var plugin = SMAPluginManager.Instance[pluginSessionGuid];

        if (plugin == null)
        {
          LogTo.Warning("A plugin tried to display windows desktop notification but its session GUID is invalid.");
          return false;
        }

        if (DesktopNotificationManager.IsApiAvailable() == false)
        {
          LogTo.Warning("Plugin {PluginName} tried to display windows desktop notification but Windows API is not available.",
                        plugin.Package?.Id);
          return false;
        }

        // Create and augment the XML definition
        var doc = new XmlDocument();
        doc.LoadXml(toastXml);

        doc.SelectNodes($"//{ToastNodeName}")
           .ForEach(n => AddPluginSessionGuidArgument(n, pluginSessionGuid));

        doc.SelectNodes($"//{ActionNodeName}")
           .ForEach(n => AddPluginSessionGuidArgument(n, pluginSessionGuid));

        // And create the toast notification
        var toast = new ToastNotification(doc);

        // And then show it
        DesktopNotificationManager.CreateToastNotifier().Show(toast);

        return true;
      }
      catch (Exception ex)
      {
        // ToastNotifier.Show() throws a generic exception when the notification API isn't available
        LogTo.Warning(ex, "Failed to show desktop toast notification.\r\nToast xml: {ToastXml}", toastXml);
      }

      return false;
    }

    #endregion




    #region Methods

    public void RaiseToastActivated(ToastActivationData toastActivationData)
    {
      OnToastActivated.InvokeRemote(
        nameof(OnToastActivated),
        toastActivationData,
        h => OnToastActivated -= h
      );
    }

    private static void AddPluginSessionGuidArgument(IXmlNode node, Guid pluginSessionGuid)
    {
      try
      {
        var attrNode = FindOrAddAttribute(node);
        var args     = QueryString.Parse(attrNode.NodeValue as string);

        args.Set(PluginSessionGuidArgName, pluginSessionGuid.ToString());

        attrNode.Value = args.ToString();
      }
      catch (Exception ex)
      {
        LogTo.Warning(ex, "An exception occured while setting Plugin's notification arguments.");
      }
    }

    private static XmlAttribute FindOrAddAttribute(IXmlNode node)
    {
      switch (node.LocalName)
      {
        case ToastNodeName:
          return FindOrAddAttribute(node, ToastLaunchAttrName);

        case ActionNodeName:
          return FindOrAddAttribute(node, ActionArgumentsAttrName);

        default:
          throw new InvalidOperationException("FindOrAddAttribute called on invalid node type {node.LocalName}");
      }
    }

    private static XmlAttribute FindOrAddAttribute(IXmlNode node, string attrName)
    {
      var doc      = node.OwnerDocument;
      var attrNode = (XmlAttribute)node.Attributes.GetNamedItem(attrName);

      if (attrNode == null)
      {
        attrNode = doc.CreateAttribute(attrName)
                      .With(a => a.Value = string.Empty);

        node.Attributes.SetNamedItem(attrNode);
      }

      return attrNode;
    }

    #endregion




    #region Events

    /// <inheritdoc />
    public event Action<ToastActivationData> OnToastActivated;

    #endregion
  }
}
