using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SuperMemoAssistant.Sys.IO.Devices
{
  [Serializable]
  public class Keys : List<(Key key, Task<bool> task)>
  {
    #region Constructors

    public Keys(params Key[] keys)
      : this(false, false, false, keys) { }

    public Keys(bool ctrl = false, bool alt = false, bool win = false, params Key[] keys)
    {
      Ctrl = ctrl;
      Alt  = alt;
      Win  = win;

      Add(keys);
    }

    public Keys(params (Key key, Task<bool> task)[] keys)
      : this(false, false, false, keys) { }

    public Keys(bool ctrl = false, bool alt = false, bool win = false, params (Key key, Task<bool> task)[] keys)
    {
      Ctrl = ctrl;
      Alt  = alt;
      Win  = win;

      AddRange(keys);
    }

    #endregion




    #region Properties & Fields

    public bool Alt  { get; set; }
    public bool Ctrl { get; set; }
    public bool Win  { get; set; }

    public IEnumerable<(VKey vkey, Task<bool> task)> VKeys =>
      this.Select(tv => ((VKey)KeyInterop.VirtualKeyFromKey(tv.key), tv.task));
    public IEnumerable<VKey> VKeysModifiers => GetModifiers().Select(k => (VKey)KeyInterop.VirtualKeyFromKey(k));

    #endregion




    #region Methods

    public void Add(params Key[] keys)
    {
      AddRange(keys.Select(key => (key, (Task<bool>)null)));
    }

    public static Keys operator +(Keys ks, Key key)
    {
      ks.Add((key, null));

      return ks;
    }

    public static Keys operator +(Key key, Keys ks)
    {
      ks.Insert(0, (key, null));

      return ks;
    }

    public static Keys operator +(Keys ks, (Key key, Task<bool> syncTask) vt)
    {
      ks.Add(vt);

      return ks;
    }

    public static Keys operator +((Key key, Task<bool> syncTask) vt, Keys ks)
    {
      ks.Insert(0, vt);

      return ks;
    }


    public IEnumerable<Key> GetModifiers()
    {
      List<Key> modifiers = new List<Key>();

      if (Alt)
        modifiers.Add(Key.LeftAlt);

      if (Ctrl)
        modifiers.Add(Key.LeftCtrl);

      if (Win)
        modifiers.Add(Key.LWin);

      return modifiers;
    }

    #endregion
  }
}
