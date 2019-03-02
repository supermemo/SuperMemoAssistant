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
// Created On:   2019/02/22 13:14
// Modified On:  2019/02/22 13:14
// Modified By:  Alexis

#endregion




using System.ComponentModel;

namespace SuperMemoAssistant.Services.IO.Keyboard
{
  public class KeyboardHookEventArgs : HandledEventArgs
  {
    #region Constructors

    public KeyboardHookEventArgs(
      LowLevelKeyboardInputEvent keyboardData,
      KeyboardState              keyboardState,
      bool                       ctrl,
      bool                       alt,
      bool                       shift,
      bool                       meta)
    {
      KeyboardData  = keyboardData;
      KeyboardState = keyboardState;
      Ctrl          = ctrl;
      Alt           = alt;
      Shift         = shift;
      Meta          = meta;
    }

    #endregion




    #region Properties & Fields - Public

    public KeyboardState              KeyboardState { get; private set; }
    public LowLevelKeyboardInputEvent KeyboardData  { get; private set; }

    public bool Alt   { get; }
    public bool Ctrl  { get; }
    public bool Shift { get; }
    public bool Meta  { get; }

    #endregion
  }
}
