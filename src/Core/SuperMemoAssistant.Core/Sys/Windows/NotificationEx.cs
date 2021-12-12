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




namespace SuperMemoAssistant.Sys.Windows
{
  using System.Diagnostics.CodeAnalysis;
  using global::Windows.Data.Xml.Dom;
  using global::Windows.UI.Notifications;
  using Microsoft.Toolkit.Uwp.Notifications;

  public static class NotificationEx
  {
    #region Methods

    public static void ShowDesktopNotification(this string notificationText, params ToastButton[] buttons)
    {
      var toastContent = new ToastContent
      {
        Visual = new ToastVisual
        {
          BindingGeneric = new ToastBindingGeneric
          {
            Children =
            {
              new AdaptiveText
              {
                Text = notificationText
              }
            }
          }
        }
      };

      if (buttons != null && buttons.Length > 0)
      {
        var toastActions = new ToastActionsCustom();

        foreach (var toastButton in buttons)
          toastActions.Buttons.Add(toastButton);

        toastContent.Actions = toastActions;
      }

      toastContent.Show();
    }

    [SuppressMessage("Design", "RCS1075:Avoid empty catch clause that catches System.Exception.",
                     Justification =
                       "ToastNotifier.Show() throws a generic exception when the notification API isn't available")]
    public static void Show(this ToastContent toastContent)
    {
      try
      {
        if (DesktopNotificationManager.IsApiAvailable() == false)
          return;

        var doc = new XmlDocument();
        doc.LoadXml(toastContent.GetContent());

        // And create the toast notification
        var toast = new ToastNotification(doc);

        // And then show it
        DesktopNotificationManager.CreateToastNotifier().Show(toast);
      }
      catch (System.Exception)
      {
        // ToastNotifier.Show() throws a generic exception when the notification API isn't available
      }
    }

    #endregion
  }
}
