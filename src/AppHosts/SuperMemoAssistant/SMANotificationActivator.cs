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
// Modified On:  2020/03/11 18:37
// Modified By:  Alexis

#endregion




using System.Runtime.InteropServices;
using System.Windows;
using Anotar.Serilog;
using Microsoft.QueryStringDotNET;
using SuperMemoAssistant.Sys.Windows;

namespace SuperMemoAssistant
{
  /// <summary>Handles user actions from Windows Toast Desktop notifications</summary>
  [ClassInterface(ClassInterfaceType.None)]
  [ComSourceInterfaces(typeof(INotificationActivationCallback))]
  [Guid("85DE7F06-9588-4EE6-ABB0-F212B01647FE")]
  [ComVisible(true)]
  public class SMANotificationActivator : NotificationActivator
  {
    #region Methods Impl

    /// <inheritdoc />
    public override void OnActivated(string arguments, NotificationUserInput userInput, string appUserModelId)
    {
      Application.Current.Dispatcher.Invoke(delegate
      {
        if (arguments.Length == 0)
          return;

        QueryString args = QueryString.Parse(arguments);

        switch (args["action"])
        {
          default:
            LogTo.Debug($"Unknown notification action {args["action"]}");
            break;
        }
      });
    }

    #endregion
  }
}
