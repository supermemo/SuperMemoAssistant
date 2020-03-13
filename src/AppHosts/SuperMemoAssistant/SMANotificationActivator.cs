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
// Modified On:  2020/03/13 01:48
// Modified By:  Alexis

#endregion




using System.Runtime.InteropServices;
using System.Windows;
using Anotar.Serilog;
using Microsoft.QueryStringDotNET;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins;
using SuperMemoAssistant.Sys.Windows;

namespace SuperMemoAssistant
{
  /// <summary>Handles user actions from Windows Toast Desktop notifications</summary>
  [ClassInterface(ClassInterfaceType.None)]
  [ComSourceInterfaces(typeof(INotificationActivationCallback))]
  [Guid("55832db8-45ea-5ead-9291-9549b25a5f0c")]
  [ComVisible(true)]
  public class SMANotificationActivator : NotificationActivator
  {
    #region Constants & Statics

    public const string AppUserModelId = "com.squirrel.SuperMemoAssistant.SuperMemoAssistant";

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override void OnActivated(string arguments, NotificationUserInput userInput, string appUserModelId)
    {
      Application.Current.Dispatcher.Invoke(delegate
      {
        if (arguments.Length == 0)
          return;

        QueryString args = QueryString.Parse(arguments);

        if (args.Contains("action") == false)
        {
          LogTo.Warning($"Received a Toast activation without an action argument: '{arguments}'");
          return;
        }

        switch (args["action"])
        {
          // Restart plugin after crash
          case SMAPluginManager.ToastActionRestartAfterCrash:
            if (args.Contains(SMAPluginManager.ToastActionParameterPluginId) == false)
            {
              LogTo.Error($"Received a ToastActionRestartAfterCrash toast activation without a plugin id parameter: '{arguments}'");
              return;
            }

            var packageId = args[SMAPluginManager.ToastActionParameterPluginId];
            SMAPluginManager.Instance.StartPlugin(packageId).RunAsync();
            break;

          default:
            LogTo.Warning($"Unknown notification action {args["action"]}: '{arguments}'");
            break;
        }
      });
    }

    #endregion




#if false
    /// <summary>
    /// Helper function to get the Squirrel-generated CLSID
    /// </summary>
    /// <returns></returns>
    public static string GetActivatorCLSID()
    {
      return Utility.CreateGuidFromHash(AppUserModelId).ToString();
    }

#endif
  }
}
