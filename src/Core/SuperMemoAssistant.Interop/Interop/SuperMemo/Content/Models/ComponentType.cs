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
// Created On:   2018/07/27 12:55
// Modified On:  2018/12/13 13:05
// Modified By:  Alexis

#endregion




using System;

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Models
{
  [Serializable]
  public enum ComponentType
  {
    Text                  = 0x00,
    Spelling              = 0x01,
    Image                 = 0x02,
    Sound                 = 0x03,
    Video                 = 0x04,
    ShapeEllipse          = 0x05,
    ShapeRectangle        = 0x06,
    ShapeRoundedRectangle = 0x07,
    Unused1               = 0x08,
    Unused2               = 0x09,
    Script                = 0x0A,
    Binary                = 0x0B,
    Rtf                   = 0x0C,
    Html                  = 0x0D,
    OLE                   = 0x0E,
    Unknown               = 0xFFFF,
  }
}
