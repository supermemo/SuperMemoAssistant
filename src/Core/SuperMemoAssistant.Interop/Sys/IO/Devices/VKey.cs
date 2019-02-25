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
// Created On:   2018/06/01 14:25
// Modified On:  2019/01/25 23:40
// Modified By:  Alexis

#endregion




using System;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace SuperMemoAssistant.Sys.IO.Devices
{
  [Serializable]
  public enum VKey
  {
    KEY_0         = 0x30, //0 key 
    KEY_1         = 0x31, //1 key 
    KEY_2         = 0x32, //2 key 
    KEY_3         = 0x33, //3 key 
    KEY_4         = 0x34, //4 key 
    KEY_5         = 0x35, //5 key 
    KEY_6         = 0x36, //6 key 
    KEY_7         = 0x37, //7 key 
    KEY_8         = 0x38, //8 key 
    KEY_9         = 0x39, //9 key
    KEY_MINUS     = 0xBD, // - key
    KEY_PLUS      = 0xBB, // + key
    KEY_A         = 0x41, //A key 
    KEY_B         = 0x42, //B key 
    KEY_C         = 0x43, //C key 
    KEY_D         = 0x44, //D key 
    KEY_E         = 0x45, //E key 
    KEY_F         = 0x46, //F key 
    KEY_G         = 0x47, //G key 
    KEY_H         = 0x48, //H key 
    KEY_I         = 0x49, //I key 
    KEY_J         = 0x4A, //J key 
    KEY_K         = 0x4B, //K key 
    KEY_L         = 0x4C, //L key 
    KEY_M         = 0x4D, //M key 
    KEY_N         = 0x4E, //N key 
    KEY_O         = 0x4F, //O key 
    KEY_P         = 0x50, //P key 
    KEY_Q         = 0x51, //Q key 
    KEY_R         = 0x52, //R key 
    KEY_S         = 0x53, //S key 
    KEY_T         = 0x54, //T key 
    KEY_U         = 0x55, //U key 
    KEY_V         = 0x56, //V key 
    KEY_W         = 0x57, //W key 
    KEY_X         = 0x58, //X key 
    KEY_Y         = 0x59, //Y key 
    KEY_Z         = 0x5A, //Z key 
    KEY_LBUTTON   = 0x01, //Left mouse button 
    KEY_RBUTTON   = 0x02, //Right mouse button 
    KEY_CANCEL    = 0x03, //Control-break processing 
    KEY_MBUTTON   = 0x04, //Middle mouse button (three-button mouse) 
    KEY_BACK      = 0x08, //BACKSPACE key 
    KEY_TAB       = 0x09, //TAB key 
    KEY_CLEAR     = 0x0C, //CLEAR key 
    KEY_RETURN    = 0x0D, //ENTER key 
    KEY_SHIFT     = 0x10, //SHIFT key 
    KEY_CONTROL   = 0x11, //CTRL key 
    KEY_MENU      = 0x12, //ALT key 
    KEY_PAUSE     = 0x13, //PAUSE key 
    KEY_CAPITAL   = 0x14, //CAPS LOCK key 
    KEY_ESCAPE    = 0x1B, //ESC key 
    KEY_SPACE     = 0x20, //SPACEBAR 
    KEY_PRIOR     = 0x21, //PAGE UP key 
    KEY_NEXT      = 0x22, //PAGE DOWN key 
    KEY_END       = 0x23, //END key 
    KEY_HOME      = 0x24, //HOME key 
    KEY_LEFT      = 0x25, //LEFT ARROW key 
    KEY_UP        = 0x26, //UP ARROW key 
    KEY_RIGHT     = 0x27, //RIGHT ARROW key 
    KEY_DOWN      = 0x28, //DOWN ARROW key 
    KEY_SELECT    = 0x29, //SELECT key 
    KEY_PRINT     = 0x2A, //PRINT key 
    KEY_EXECUTE   = 0x2B, //EXECUTE key 
    KEY_SNAPSHOT  = 0x2C, //PRINT SCREEN key 
    KEY_INSERT    = 0x2D, //INS key 
    KEY_DELETE    = 0x2E, //DEL key 
    KEY_HELP      = 0x2F, //HELP key 
    KEY_NUMPAD0   = 0x60, //Numeric keypad 0 key 
    KEY_NUMPAD1   = 0x61, //Numeric keypad 1 key 
    KEY_NUMPAD2   = 0x62, //Numeric keypad 2 key 
    KEY_NUMPAD3   = 0x63, //Numeric keypad 3 key 
    KEY_NUMPAD4   = 0x64, //Numeric keypad 4 key 
    KEY_NUMPAD5   = 0x65, //Numeric keypad 5 key 
    KEY_NUMPAD6   = 0x66, //Numeric keypad 6 key 
    KEY_NUMPAD7   = 0x67, //Numeric keypad 7 key 
    KEY_NUMPAD8   = 0x68, //Numeric keypad 8 key 
    KEY_NUMPAD9   = 0x69, //Numeric keypad 9 key 
    KEY_SEPARATOR = 0x6C, //Separator key 
    KEY_SUBTRACT  = 0x6D, //Subtract key 
    KEY_DECIMAL   = 0x6E, //Decimal key 
    KEY_DIVIDE    = 0x6F, //Divide key 
    KEY_F1        = 0x70, //F1 key 
    KEY_F2        = 0x71, //F2 key 
    KEY_F3        = 0x72, //F3 key 
    KEY_F4        = 0x73, //F4 key 
    KEY_F5        = 0x74, //F5 key 
    KEY_F6        = 0x75, //F6 key 
    KEY_F7        = 0x76, //F7 key 
    KEY_F8        = 0x77, //F8 key 
    KEY_F9        = 0x78, //F9 key 
    KEY_F10       = 0x79, //F10 key 
    KEY_F11       = 0x7A, //F11 key 
    KEY_F12       = 0x7B, //F12 key 
    KEY_SCROLL    = 0x91, //SCROLL LOCK key 
    KEY_LSHIFT    = 0xA0, //Left SHIFT key 
    KEY_RSHIFT    = 0xA1, //Right SHIFT key 
    KEY_LCONTROL  = 0xA2, //Left CONTROL key 
    KEY_RCONTROL  = 0xA3, //Right CONTROL key 
    KEY_LMENU     = 0xA4, //Left MENU key 
    KEY_RMENU     = 0xA5, //Right MENU key 
    KEY_COMMA     = 0xBC, //, key
    KEY_PERIOD    = 0xBE, //. key
    KEY_PLAY      = 0xFA, //Play key 
    KEY_ZOOM      = 0xFB, //Zoom key 
    NULL          = 0x0,
  }
}
