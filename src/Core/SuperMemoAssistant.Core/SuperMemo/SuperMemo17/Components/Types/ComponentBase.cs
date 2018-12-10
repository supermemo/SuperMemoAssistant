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
// Created On:   2018/06/01 14:13
// Modified On:  2018/12/10 13:01
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Components.Models;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Components.Types
{
  public abstract class ComponentBase : MarshalByRefObject
  {
    #region Properties & Fields - Non-Public

    //
    // Internal helpers

    protected Dictionary<string, ComponentFieldFlags> FieldFlagMapping { get; } = new Dictionary<string, ComponentFieldFlags>()
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

    #endregion




    #region Constructors

    protected ComponentBase(short   left,
                            short   top,
                            short   right,
                            short   bottom,
                            AtFlags displayAt)
    {
#if DEBUG
      //System.Diagnostics.Debug.WriteLine("[{0}] Creating component", new[] { this.GetType().Name });
#endif

      Left = SetValue(left,
                      nameof(Left));
      Top = SetValue(top,
                     nameof(Top));
      Right = SetValue(right,
                       nameof(Right));
      Bottom = SetValue(bottom,
                        nameof(Bottom));
      DisplayAt = SetValue(displayAt,
                           nameof(DisplayAt));
    }

    #endregion




    #region Properties & Fields - Public

    public short   Left      { get; set; }
    public short   Top       { get; set; }
    public short   Right     { get; set; }
    public short   Bottom    { get; set; }
    public AtFlags DisplayAt { get; set; }

    #endregion




    #region Methods

    protected void Update(short               left,
                          short               top,
                          short               right,
                          short               bottom,
                          AtFlags             displayAt,
                          ComponentFieldFlags flags)
    {
#if DEBUG
      //System.Diagnostics.Debug.WriteLine("[{0}] Updating component", new[] { this.GetType().Name });
#endif

      Left = SetValue(Left,
                      left,
                      nameof(Left),
                      ref flags);
      Top = SetValue(Top,
                     top,
                     nameof(Top),
                     ref flags);
      Right = SetValue(Right,
                       right,
                       nameof(Right),
                       ref flags);
      Bottom = SetValue(Bottom,
                        bottom,
                        nameof(Bottom),
                        ref flags);
      DisplayAt = SetValue(DisplayAt,
                           displayAt,
                           nameof(DisplayAt),
                           ref flags);
    }

    protected T SetValue<T>(T                       oldValue,
                            T                       value,
                            string                  name,
                            ref ComponentFieldFlags flag,
                            Action<T, T>            onChangedAction = null)
    {
      if (Equals(oldValue,
                 value))
      {
        ComponentFieldFlags newFlag = FieldFlagMapping.SafeGet(name,
                                                               ComponentFieldFlags.None);

        if (newFlag != ComponentFieldFlags.None)
          flag |= newFlag;

        onChangedAction?.Invoke(oldValue,
                                value);

#if DEBUG
        System.Diagnostics.Debug.WriteLine("[{0}] {1}: {2}",
                                           GetType().Name,
                                           name,
                                           value);
#endif
      }

      return value;
    }

    protected T SetValue<T>(T      value,
                            string name)
    {
#if DEBUG
      System.Diagnostics.Debug.WriteLine("[{0}] {1}: {2}",
                                         GetType().Name,
                                         name,
                                         value);
#endif

      return value;
    }

    #endregion
  }
}
