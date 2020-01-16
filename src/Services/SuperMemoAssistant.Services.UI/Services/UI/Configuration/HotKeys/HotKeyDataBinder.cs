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
// Created On:   2019/03/01 02:08
// Modified On:  2019/03/01 16:56
// Modified By:  Alexis

#endregion




using System.ComponentModel;
using System.Windows.Input;
using MahApps.Metro.Controls;
using SuperMemoAssistant.Services.IO.HotKeys;
using SuperMemoAssistant.Services.UI.Extensions;

namespace SuperMemoAssistant.Services.UI.Configuration.HotKeys
{
  public class HotKeyDataBinder : INotifyPropertyChanged, IDataErrorInfo
  {
    #region Properties & Fields - Non-Public

    private readonly HotKeyManager _hotKeyManager;

    private HotKey     _hotKey;
    private HotKeyData _alreadyBoundTo;

    private bool HasAlreadyBoundError => _alreadyBoundTo != null && _alreadyBoundTo != HotKeyData;

    #endregion




    #region Constructors

    public HotKeyDataBinder(HotKeyManager hotKeyManager, HotKeyData hotKeyData)
    {
      _hotKeyManager = hotKeyManager;

      HotKeyData = hotKeyData;
      HotKey     = hotKeyData.ActualHotKey?.ToMahApps();
    }

    #endregion




    #region Properties & Fields - Public

    public HotKeyData HotKeyData { get; }

    public HotKey HotKey
    {
      get => _hotKey;
      set
      {
        _hotKey = value;

        if (value == null
          || value.ModifierKeys == ModifierKeys.None && value.Key == Key.Escape)
        {
          _alreadyBoundTo         = null;
          HotKeyData.ActualHotKey = null;
        }
        else
        {
          var smaHotKey = value.ToSMA();

          _alreadyBoundTo         = _hotKeyManager.Match(smaHotKey);

          if (HasAlreadyBoundError == false)
            HotKeyData.ActualHotKey = smaHotKey;
        }
      }
    }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public string this[string columnName]
    {
      get
      {
        switch (columnName)
        {
          case "HotKey":
            if (HasAlreadyBoundError)
              return $"Already bound to '{_alreadyBoundTo.Description}'";

            return null;
        }

        return null;
      }
    }

    /// <inheritdoc />
    public string Error => string.Empty;

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
