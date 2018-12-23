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
// Created On:   2018/12/21 02:18
// Modified On:  2018/12/21 02:20
// Modified By:  Alexis

#endregion




using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace SuperMemoAssistant.Services.IO.Devices
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




  #region Enums

  //const int HC_ACTION = 0;

  public enum KeyboardState
  {
    KeyDown    = 0x0100,
    KeyUp      = 0x0101,
    SysKeyDown = 0x0104,
    SysKeyUp   = 0x0105
  }

  #endregion




  [StructLayout(LayoutKind.Sequential)]
  public struct LowLevelKeyboardInputEvent
  {
    /// <summary>A virtual-key code. The code must be a value in the range 1 to 254.</summary>
    public readonly int VirtualCode;

    /// <summary>A hardware scan code for the key.</summary>
    public readonly int HardwareScanCode;

    /// <summary>
    ///   The extended-key flag, event-injected Flags, context code, and transition-state flag.
    ///   This member is specified as follows. An application can use the following values to test the
    ///   keystroke Flags. Testing LLKHF_INJECTED (bit 4) will tell you whether the event was
    ///   injected. If it was, then testing LLKHF_LOWER_IL_INJECTED (bit 1) will tell you whether or
    ///   not the event was injected from a process running at lower integrity level.
    /// </summary>
    public readonly int Flags;

    /// <summary>
    ///   The time stamp stamp for this message, equivalent to what GetMessageTime would return
    ///   for this message.
    /// </summary>
    public readonly int TimeStamp;

    /// <summary>Additional information associated with the message.</summary>
    public readonly IntPtr AdditionalInformation;

    public Key Key => KeyInterop.KeyFromVirtualKey(VirtualCode);
  }
}
