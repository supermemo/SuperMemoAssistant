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
// Modified On:  2019/03/02 04:11
// Modified By:  Alexis

#endregion




using System;
using System.Windows;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Services.Configuration;
using SuperMemoAssistant.Services.IO.HotKeys;
using SuperMemoAssistant.Services.IO.Keyboard;
using SuperMemoAssistant.Services.IO.Logger;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable UnusedTypeParameter

namespace SuperMemoAssistant.Services
{
  public static class Svc
  {
    #region Constants & Statics

    private static ISuperMemoAssistant _sma;

    public static ISuperMemoAssistant SMA
    {
      get => _sma;
      set
      {
        _sma = value;
        OnSMAAvailable?.Invoke(value);
      }
    }
    public static ISuperMemo SM => SMA?.SM;
    public static ISMAPlugin Plugin { get; set; }

    public static IKeyboardHotKeyService KeyboardHotKeyLegacy { get; set; }
    public static IKeyboardHookService   KeyboardHotKey       { get; set; }
    public static HotKeyManager          HotKeyManager        { get; set; }

    public static Logger Logger { get; set; }
    
    public static ConfigurationServiceBase Configuration { get; set; }
    public static ConfigurationServiceBase CollectionConfiguration { get; set; }
    public static ConfigurationServiceBase SharedConfiguration { get; set; }

    public static Application App { get; set; }

    #endregion




    #region Events

    public static event Action<ISuperMemoAssistant> OnSMAAvailable;

    #endregion
  }

  public static class Svc<T>
    where T : ISMAPlugin
  {
    #region Constants & Statics

    public static T Plugin => (T)Svc.Plugin;

    #endregion
  }
}
