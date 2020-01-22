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
// Created On:   2019/09/03 18:15
// Modified On:  2020/01/22 13:18
// Modified By:  Alexis

#endregion




using System;
using System.Windows.Input;
using Newtonsoft.Json;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace SuperMemoAssistant.Sys.IO.Devices
{
  [Serializable]
  public class HotKey : IEquatable<HotKey>
  {
    #region Constructors

    public HotKey() { }

    public HotKey(Key key, bool ctrl = false, bool alt = false, bool shift = false, bool win = false)
      : this(key, MakeModifier(ctrl, alt, shift, win)) { }

    public HotKey(Key key, KeyModifiers modifiers)
    {
      Key       = key;
      Modifiers = modifiers;
    }

    #endregion




    #region Properties & Fields - Public

    public Key Key { get; set; }
    [JsonIgnore]
    public VKey VKey => (VKey)KeyInterop.VirtualKeyFromKey(Key);

    public KeyModifiers Modifiers { get; set; }
    [JsonIgnore]
    public bool Ctrl => Modifiers.HasFlag(KeyModifiers.Ctrl);
    [JsonIgnore]
    public bool Alt => Modifiers.HasFlag(KeyModifiers.Alt);
    [JsonIgnore]
    public bool Shift => Modifiers.HasFlag(KeyModifiers.Shift);
    [JsonIgnore]
    public bool Win => Modifiers.HasFlag(KeyModifiers.Meta);

    #endregion




    #region Methods Impl

    public override string ToString()
    {
      return $"{Modifiers.GetString()}+{Enum.GetName(typeof(Key), Key)}";
    }

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

    public static bool operator ==(HotKey left, HotKey right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(HotKey left, HotKey right)
    {
      return !Equals(left, right);
    }

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
        ret |= KeyModifiers.Meta;

      return ret;
    }

    #endregion
  }
}
