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
// Modified On:  2019/03/01 01:54
// Modified By:  Alexis

#endregion




using System.Windows;
using System.Windows.Input;
using SuperMemoAssistant.Services.IO.Keyboard;
using SuperMemoAssistant.SMA.UI.DataTemplates;
using SuperMemoAssistant.SMA.UI.Settings;
using SuperMemoAssistant.Sys.IO.Devices;

namespace SuperMemoAssistant.SMA.UI
{
  // ReSharper disable once InconsistentNaming
  public static class SMAUI
  {
    #region Methods

    public static void Initialize()
    {
      Core.HotKeyManager.RegisterGlobal(
        "Settings",
        "Show settings window",
        HotKeyScope.Global,
        new HotKey(Key.O, KeyModifiers.CtrlAltShift),
        ShowGlobalSettings
      );

#if DEBUG
      Core.HotKeyManager.RegisterGlobal(
        "DebugInjectLib",
        "Attach debugger to injected lib",
        HotKeyScope.Global,
        new HotKey(Key.D, KeyModifiers.CtrlMetaShift),
        DebugInjectLib
      );
#endif
      
      // Set default data templates
      Application.Current.Dispatcher.InvokeAsync(() =>
      {
        Application.Current.Resources.MergedDictionaries.Add(new OnlinePluginPackageDataTemplate().Resources);
        Application.Current.Resources.MergedDictionaries.Add(new LocalPluginPackageDataTemplate().Resources);
      });
    }

    private static void ShowGlobalSettings()
    {
      Application.Current.Dispatcher.Invoke(SettingsWindow.ShowOrActivate);
    }

    private static void DebugInjectLib()
    {
      /*
      Core.SMA.WindowFactory.MainWindow.PostMessage(
        InjectLibMessageIds.SMA,
        new IntPtr((int)InjectLibMessageParams.AttachDebugger),
        new IntPtr(0)
      );
      */
    }

    #endregion
  }
}
