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
  using Configs;
  using Services.Configuration;
  using Services.IO.Diagnostics;
  using Services.IO.HotKeys;
  using Services.IO.Keyboard;
  using SuperMemo.Common;
  using SuperMemo.Hooks;
  using SuperMemo.Natives;

  public static class Core
  {
    #region Constants & Statics

    public static string SMAVersion { get; set; }

    public static SMA           SMA     { get; set; }
    public static SuperMemoCore SM      { get; set; }
    public static SMHookEngine  Hook    { get; set; }
    public static SMNatives     Natives { get; set; }

    public static CoreCfg CoreConfig { get; set; }

    public static Logger Logger { get; set; }

    public static IKeyboardHookService KeyboardHotKey { get; set; }
    public static HotKeyManager        HotKeyManager  { get; set; }

    public static NotificationManager NotificationMgr { get; set; }

    public static ConfigurationServiceBase Configuration           { get; set; }
    public static ConfigurationServiceBase CollectionConfiguration { get; set; }
    public static ConfigurationServiceBase SharedConfiguration     { get; set; }

    #endregion
  }
}
