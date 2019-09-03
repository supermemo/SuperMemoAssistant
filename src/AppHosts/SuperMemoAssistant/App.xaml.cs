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
// Created On:   2019/02/13 13:55
// Modified On:  2019/02/25 00:50
// Modified By:  Alexis

#endregion




using System.Threading.Tasks;
using System.Windows;
using Anotar.Serilog;
using Forge.Forms;
using Hardcodet.Wpf.TaskbarNotification;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Services.IO.Logger;
using SuperMemoAssistant.SMA;

namespace SuperMemoAssistant
{
  /// <summary>Interaction logic for App.xaml</summary>
  public partial class App : Application
  {
    private TaskbarIcon _taskbarIcon;




    #region Methods Impl

    protected override void OnExit(ExitEventArgs e)
    {
      _taskbarIcon.Dispose();

      Logger.Instance.Shutdown();
      ModuleInitializer.SentryInstance?.Dispose();

      base.OnExit(e);
    }

    #endregion




    #region Methods

    private async void Application_Startup(object           o1,
                                     StartupEventArgs e1)
    {
      DispatcherUnhandledException += (o2, e2) => LogTo.Error(e2.Exception, "Unhandled exception");

      _taskbarIcon = (TaskbarIcon)FindResource("TbIcon");

      var selectionWdw = new CollectionSelectionWindow();

      selectionWdw.ShowDialog();

      var selectedCol = selectionWdw.Collection;

      if (selectionWdw.Collection != null)
      {
        Core.SMA.OnSMStoppedEvent += Instance_OnSMStoppedEvent;

        if (await Core.SMA.Start(selectedCol).ConfigureAwait(true) == false)
        {
          await Show.Window().For(
            new Alert(
              $"SMA failed to start. Please check the logs in '{SMAFileSystem.LogDir.FullPath}' for details.",
              "Error")
          );
          Shutdown();
        }
      }
      else
      {
        Shutdown();
      }
    }

    private Task Instance_OnSMStoppedEvent(object                               sender,
                                           Interop.SuperMemo.Core.SMProcessArgs e)
    {
      return Dispatcher.InvokeAsync(Shutdown).Task;
    }

    #endregion
  }
}
