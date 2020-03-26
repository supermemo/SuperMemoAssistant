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
// Modified On:  2020/03/13 15:23
// Modified By:  Alexis

#endregion




using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Services.IO.HotKeys;
using SuperMemoAssistant.Sys.ComponentModel;

namespace SuperMemoAssistant.Services.UI.Configuration
{
  /// <summary>Provides facilities to display a configuration UI</summary>
  public partial class ConfigurationWindow : Window, INotifyPropertyChanged
  {
    #region Constants & Statics

    /// <summary>
    /// Ensures only one <see cref="ConfigurationWindow"/> is open at all time
    /// </summary>
    private static Semaphore SingletonSemaphore { get; } = new Semaphore(1, 1);

    #endregion




    #region Constructors

    protected ConfigurationWindow(HotKeyManager hotKeyManager, params INotifyPropertyChanged[] configModels)
    {
      InitializeComponent();

      Array.ForEach(configModels, m => Models.Add(m));

      if (hotKeyManager != null)
      {
        Models.Add(hotKeyManager);
        HotKeyManager = hotKeyManager;
      }
    }

    #endregion




    #region Properties & Fields - Public

    /// <summary>The configuration instances to display and edit</summary>
    public ObservableCollection<object> Models { get; } = new ObservableCollection<object>();

    /// <summary>
    ///   The optional <see cref="HotKeyManager" />. Hotkey rebinding options will be offered
    ///   to the user if it is set to a valid intance.
    /// </summary>
    public HotKeyManager HotKeyManager { get; }

    /// <summary>Optional callback to override the default saving mechanism</summary>
    public Action<INotifyPropertyChanged> SaveMethod { get; set; }

    #endregion




    #region Methods

    /// <summary>Instantiates a new <see cref="ConfigurationWindow" /> if none other exist</summary>
    /// <param name="configModels">The configuration class instances that should be displayed</param>
    /// <returns>New instance or <see langword="null" /></returns>
    public static ConfigurationWindow ShowAndActivate(params INotifyPropertyChanged[] configModels)
    {
      return ShowAndActivate(null, configModels);
    }

    /// <summary>Instantiates a new <see cref="ConfigurationWindow" /> if none other exist</summary>
    /// <param name="hotKeyManager">
    ///   An optional instance of a <see cref="HotKeyManager" /> that will
    ///   be used to provide hotkey rebinding options to the user
    /// </param>
    /// <param name="configModels">The configuration class instances that should be displayed</param>
    /// <returns>New instance or <see langword="null" /></returns>
    public static ConfigurationWindow ShowAndActivate(HotKeyManager hotKeyManager, params INotifyPropertyChanged[] configModels)
    {
      return Application.Current.Dispatcher.Invoke(() =>
      {
        if (SingletonSemaphore.WaitOne(0) == false)
          return null;

        var cfgWdw = new ConfigurationWindow(hotKeyManager, configModels);
        cfgWdw.ShowAndActivate();

        return cfgWdw;
      });
    }

    private void BtnOk_Click(object          sender,
                             RoutedEventArgs e)
    {
      Close();
    }

    private void Window_KeyDown(object       sender,
                                KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Escape:
        case Key.Enter:
          BtnOk_Click(sender,
                      null);
          break;
      }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      foreach (var m in Models)
        switch (m)
        {
          case INotifyPropertyChanged config when SaveMethod != null:
            SaveMethod(config);
            break;

          case INotifyPropertyChangedEx config:
            if (config.IsChanged)
            {
              config.IsChanged = false;
              Svc.Configuration.Save(m, m.GetType()).RunAsync();
            }

            break;

          case INotifyPropertyChanged _:
            Svc.Configuration.Save(m, m.GetType()).RunAsync();
            break;
        }

      Dispatcher.Invoke(() => SingletonSemaphore.Release());
    }

    #endregion




    #region Events

    /// <inheritdoc/>
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
