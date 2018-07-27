using System;
using System.Collections.Generic;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Components.Models;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Components.Types
{
  public abstract class ComponentBase : MarshalByRefObject
  {
    protected ComponentBase(short left, short top, short right, short bottom, AtFlags displayAt)
    {
#if DEBUG
      System.Diagnostics.Debug.WriteLine("[{0}] Creating component", new[] { this.GetType().Name });
#endif

      Left = SetValue(left, nameof(Left));
      Top = SetValue(top, nameof(Top));
      Right = SetValue(right, nameof(Right));
      Bottom = SetValue(bottom, nameof(Bottom));
      DisplayAt = SetValue(displayAt, nameof(DisplayAt));
    }

    protected void Update(short left, short top, short right, short bottom, AtFlags displayAt, ComponentFieldFlags flags)
    {
#if DEBUG
      System.Diagnostics.Debug.WriteLine("[{0}] Updating component", new[] { this.GetType().Name });
#endif

      Left = SetValue(Left, left, nameof(Left), ref flags);
      Top = SetValue(Top, top, nameof(Top), ref flags);
      Right = SetValue(Right, right, nameof(Right), ref flags);
      Bottom = SetValue(Bottom, bottom, nameof(Bottom), ref flags);
      DisplayAt = SetValue(DisplayAt, displayAt, nameof(DisplayAt), ref flags);
    }

    public short Left { get; set; }
    public short Top { get; set; }
    public short Right { get; set; }
    public short Bottom { get; set; }
    public AtFlags DisplayAt { get; set; }



    //
    // Internal helpers

    protected Dictionary<string, ComponentFieldFlags> FieldFlagMapping = new Dictionary<string, ComponentFieldFlags>()
    {
      { "Left", ComponentFieldFlags.Left },
      { "Top", ComponentFieldFlags.Top },
      { "Right", ComponentFieldFlags.Right },
      { "Bottom", ComponentFieldFlags.Bottom },
      { "DisplayAt", ComponentFieldFlags.DisplayAt },
      { "RegistryId", ComponentFieldFlags.RegistryId },
      { "Color", ComponentFieldFlags.Color },
      { "TextAlignment", ComponentFieldFlags.TextAlignment },
      { "ExtractStart", ComponentFieldFlags.ExtractStart },
      { "ExtractStop", ComponentFieldFlags.ExtractStop },
      { "PanelType", ComponentFieldFlags.PanelType },
      { "PlayAt", ComponentFieldFlags.PlayAt },
      { "IsContinuous", ComponentFieldFlags.IsContinuous },
      { "IsFullScreen", ComponentFieldFlags.IsFullScreen },
      { "IsFullHtml", ComponentFieldFlags.IsFullHtml },
      { "StretchType", ComponentFieldFlags.StretchType },
    };

    protected T SetValue<T>(T oldValue, T value, string name, ref ComponentFieldFlags flag, Action<T, T> onChangedAction = null)
    {
      if (Object.Equals(oldValue, value))
      {
        ComponentFieldFlags newFlag = FieldFlagMapping.SafeGet(name, ComponentFieldFlags.None);

        if (newFlag != ComponentFieldFlags.None)
          flag |= newFlag;

        onChangedAction?.Invoke(oldValue, value);

#if DEBUG
        System.Diagnostics.Debug.WriteLine("[{0}] {1}: {2}", this.GetType().Name, name, value);
#endif
      }

      return value;
    }

    protected T SetValue<T>(T value, string name)
    {
#if DEBUG
      System.Diagnostics.Debug.WriteLine("[{0}] {1}: {2}", this.GetType().Name, name, value);
#endif

      return value;
    }
  }
}
