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
// Created On:   2018/05/31 04:23
// Modified On:  2018/05/31 10:43
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using PluginManager.Interop.Sys;
using SuperMemoAssistant.Sys.IO.Devices;
using Message = SuperMemoAssistant.Sys.IO.Devices.Message;

namespace SuperMemoAssistant.Services.IO.Keyboard
{
  /// <summary>https://stackoverflow.com/questions/1153009/how-can-i-convert-system-windows-input-key-to-system-windows-forms-keys</summary>
  public class KeyboardHotKeyService 
    : PerpetualMarshalByRefObject,
      IKeyboardHotKeyService, IDisposable
  {
    #region Constants & Statics

    private static volatile MessageWindow          _wnd;
    private static volatile IntPtr                 _hwnd;
    private static          SynchronizationContext _syncContext;
    private static readonly ManualResetEvent       _windowReadyEvent = new ManualResetEvent(false);
    private static          int                    _id               = 0;
    
    public static KeyboardHotKeyService Instance { get; } = new KeyboardHotKeyService();

    #endregion




    #region Properties & Fields - Non-Public

    private ConcurrentDictionary<HotKey, (int id, Action action)> RegisteredHotKeys { get; } =
      new ConcurrentDictionary<HotKey, (int id, Action action)>();

    #endregion




    #region Constructors

    static KeyboardHotKeyService()
    {
      Thread messageLoop = new Thread(delegate() { Application.Run(new MessageWindow()); })
      {
        Name         = "KeyboardHotKeyService_MessageLoopThread",
        IsBackground = true,
      };

      messageLoop.SetApartmentState(ApartmentState.STA);
      messageLoop.Start();
    }

    protected KeyboardHotKeyService() { }

    /// <inheritdoc />
    public void Dispose()
    {
      foreach (var hotKey in RegisteredHotKeys.Keys.ToList())
        UnregisterHotKey(hotKey);

      _syncContext.Send(delegate { _wnd.Close(); },
                        null);
    }

    #endregion




    #region Methods

    public (bool success, HotKey usedBy) RegisterHotKey(HotKey hotKey, Action callback)
    {
      if (RegisteredHotKeys.ContainsKey(hotKey))
        return (false, RegisteredHotKeys.Keys.FirstOrDefault(hk => hk.Equals(hotKey)));

      _windowReadyEvent.WaitOne();

      int id = Interlocked.Increment(ref _id);
      bool success = (bool)_wnd.Invoke(new RegisterHotKeyDelegate(RegisterHotKeyInternal), _hwnd, id,
                                       (uint)hotKey.Modifiers,
                                       (uint)hotKey.VKey);

      if (success)
        RegisteredHotKeys[hotKey] = (id, callback);

      return (success, null);
    }

    public bool UnregisterHotKey(HotKey hotKey)
    {
      if (RegisteredHotKeys.ContainsKey(hotKey) == false)
        return false;

      bool success = (bool)_wnd.Invoke(new UnRegisterHotKeyDelegate(UnRegisterHotKeyInternal), _hwnd,
                                       RegisteredHotKeys[hotKey].id);

      if (success)
        RegisteredHotKeys.TryRemove(hotKey, out _);

      return success;
    }

    private bool RegisterHotKeyInternal(IntPtr hwnd, int id, uint modifiers, uint key)
    {
      return Native.RegisterHotKey(hwnd, id, modifiers, key);
    }

    private bool UnRegisterHotKeyInternal(IntPtr hwnd, int id)
    {
      return Native.UnregisterHotKey(_hwnd, id);
    }

    private void OnHotKeyPressed(HotKeyEventArgs e)
    {
      HotKey hk = new HotKey(KeyInterop.KeyFromVirtualKey((int)e.VKey), e.Modifiers);

      RegisteredHotKeys.TryGetValue(hk, out var hkData);

      hkData.action?.Invoke();
    }

    #endregion




    private delegate bool RegisterHotKeyDelegate(IntPtr hwnd, int id, uint modifiers, uint key);

    private delegate bool UnRegisterHotKeyDelegate(IntPtr hwnd, int id);


    private class MessageWindow : Form
    {
      #region Constructors

      public MessageWindow()
      {
        _wnd  = this;
        _hwnd = Handle;
        _windowReadyEvent.Set();

        _syncContext = SynchronizationContext.Current;
      }

      #endregion




      #region Methods Impl

      protected override void WndProc(ref System.Windows.Forms.Message m)
      {
        if (m.Msg == (int)Message.HOTKEY)
        {
          HotKeyEventArgs e = new HotKeyEventArgs(m.LParam);
          Instance.OnHotKeyPressed(e);
        }

        base.WndProc(ref m);
      }

      protected override void SetVisibleCore(bool value)
      {
        // Ensure the window never becomes visible
        base.SetVisibleCore(false);
      }

      #endregion
    }
  }
}
