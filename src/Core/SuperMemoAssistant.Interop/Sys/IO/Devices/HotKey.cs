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
// Created On:   2018/05/31 08:56
// Modified On:  2018/05/31 09:41
// Modified By:  Alexis

#endregion




using System;
using System.Windows.Input;

namespace SuperMemoAssistant.Sys.IO.Devices
{
  [Serializable]
  public class HotKey : IEquatable<HotKey>
  {
    #region Constructors

    public HotKey(bool ctrl, bool alt, bool shift, bool win, Key key, string description)
      : this(MakeModifier(ctrl, alt, shift, win), key, description) { }

    public HotKey(KeyModifiers modifiers, Key key, string description)
    {
      Modifiers   = modifiers;
      Key         = key;
      Description = description;
    }

    #endregion




    #region Properties & Fields - Public

    public String       Description { get; }
    public KeyModifiers Modifiers   { get; }
    public Key          Key         { get; }
    public VKey         VKey        => (VKey)KeyInterop.VirtualKeyFromKey(Key);

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override int GetHashCode()
    {
      return (int)Modifiers * 0xF000 + (int)Key;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
      return Equals(obj as HotKey);
    }

    /// <inheritdoc />
    public bool Equals(HotKey other)
    {
      return other != null &&
        Modifiers == other.Modifiers &&
        Key == other.Key;
    }

    #endregion




    #region Methods

    public static KeyModifiers MakeModifier(bool ctrl, bool alt, bool shift, bool win)
    {
      KeyModifiers ret = KeyModifiers.None;

      if (ctrl)
        ret |= KeyModifiers.Ctrl;

      if (alt)
        ret |= KeyModifiers.Alt;

      if (shift)
        ret |= KeyModifiers.Shift;

      if (win)
        ret |= KeyModifiers.Win;

      return ret;
    }

    #endregion
  }
}
