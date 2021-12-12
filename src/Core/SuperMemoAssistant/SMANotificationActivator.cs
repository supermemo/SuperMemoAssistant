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




namespace SuperMemoAssistant
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.InteropServices;
  using System.Windows;
  using Anotar.Serilog;
  using Interop.SMA.Notifications;
  using Microsoft.QueryStringDotNET;
  using Plugins;
  using SMA;
  using Squirrel.Extensions;
  using Sys.Windows;

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

        var args = QueryString.Parse(arguments);

        if (args.Contains(NotificationManager.PluginSessionGuidArgName))
        {
          var pluginSessionGuidStr = args[NotificationManager.PluginSessionGuidArgName];

          if (Guid.TryParse(pluginSessionGuidStr, out var pluginSessionGuid) == false)
          {
            LogTo.Error("A notification was activated for an invalid GUID.\r\nGUID: {Guid}\r\nArgs: {Args}", pluginSessionGuidStr, args);
            return;
          }

          var plugin = SMAPluginManager.Instance[pluginSessionGuid];

          if (plugin == null)
          {
            LogTo.Debug("A notification was activated for an expired plugin.\r\n{Args}", args);
            return;
          }

          LogTo.Debug("Plugin Toast activated.\r\nArgs: {Args}", args);

          var argDict       = args.ToDictionary(k => k.Name, v => v.Value);
          var userInputDict = userInput.ToDictionary(k => k.Key, v => v.Value);

          var activationData = new ToastActivationData(
            plugin.Package.Id,
            plugin.Package.Version,
            argDict,
            userInputDict);

          SMA.Core.NotificationMgr.RaiseToastActivated(activationData);

          return;
        }

        if (args.Contains("action") == false)
        {
          LogTo.Warning("Received a Toast activation without an action argument: '{Args}'", args);
          return;
        }

        LogTo.Debug("SMA Toast activated.\r\nArgs: {Args}", args);

        switch (args["action"])
        {
          // Restart plugin after crash
          case SMAPluginManager.ToastActionRestartAfterCrash:
            if (args.Contains(SMAPluginManager.ToastActionParameterPluginId) == false)
            {
              LogTo.Warning("Received a ToastActionRestartAfterCrash toast activation without a plugin id parameter: '{Arguments}'",
                            arguments);
              return;
            }

            var packageId = args[SMAPluginManager.ToastActionParameterPluginId];
            SMAPluginManager.Instance.StartPluginAsync(packageId).RunAsync();
            break;

          default:
            LogTo.Warning("Unknown notification action {V}: '{Arguments}'", args["action"], arguments);
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
