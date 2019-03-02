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
// Created On:   2019/02/28 00:04
// Modified On:  2019/03/01 02:18
// Modified By:  Alexis

#endregion




using System;
using System.ComponentModel;
using SuperMemoAssistant.Sys.IO.Devices;

namespace SuperMemoAssistant.Services.IO.HotKeys
{
  public delegate void HotKeyChangedDelegate(HotKeyData hkData,
                                             HotKey     actualBefore,
                                             HotKey     actualAfter);

  public class HotKeyData : INotifyPropertyChanged
  {
    #region Constructors

    /// <inheritdoc />
    public HotKeyData(string id,
                      string description,
                      bool   enabled,
                      bool   isGlobal,
                      HotKey defaultHotKey,
                      HotKey actualHotKey,
                      Action callback)
    {
      Id            = id;
      Description   = description;
      Enabled       = enabled;
      IsGlobal      = isGlobal;
      DefaultHotKey = defaultHotKey;
      ActualHotKey  = actualHotKey;
      Callback      = callback;
    }

    #endregion




    #region Properties & Fields - Public

    public string Id          { get; }
    public string Description { get; }

    public bool Enabled  { get; set; }
    public bool IsGlobal { get; }

    public HotKey DefaultHotKey { get; }
    public HotKey ActualHotKey  { get; set; }

    public Action Callback { get; }

    #endregion




    #region Methods

    public void OnActualHotKeyChanged(object before, object after)
    {
      if (before == null || before == after)
        return;

      HotKeyChanged?.Invoke(
        this,
        before as HotKey, after as HotKey
        );
    }

    #endregion




    #region Events

    public event HotKeyChangedDelegate       HotKeyChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
