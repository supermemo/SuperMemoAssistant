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
// Created On:   2018/05/19 14:15
// Modified On:  2019/01/24 14:07
// Modified By:  Alexis

#endregion




using System.Runtime.InteropServices;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Files
{
  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 29)]
  public unsafe struct InfComponentsHtml17
  {
    public       byte  unknown1;
    public       short left;
    public       short top;
    public       short width;
    public       short height;
    public       byte  displayAt;
    public fixed byte  unknown2[5];
    public       byte  isFullHtml;
    public fixed byte  unknown3[2];
    public       int   registryId;
    public fixed byte  unknown4[7];
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 35)]
  public unsafe struct InfComponentsText17
  {
    public       byte  unknown1;
    public       short left;
    public       short top;
    public       short width;
    public       short height;
    public       byte  displayAt;
    public fixed byte  unknown2[8];
    public       int   registryId;
    public       byte  textAlignment; //0: Left, 1: Center, 2: Right
    public       byte  colorRed;
    public       byte  colorGreen;
    public       byte  colorBlue;
    public fixed byte  unknown4[9];
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 30)]
  public unsafe struct InfComponentsRtf17
  {
    public       byte  unknown1;
    public       short left;
    public       short top;
    public       short width;
    public       short height;
    public       byte  displayAt;
    public fixed byte  unknown2[8];
    public       int   registryId;
    public       byte  unknown3;
    public       byte  colorRed;
    public       byte  colorGreen;
    public       byte  colorBlue;
    public fixed byte  unknown4[4];
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 35)]
  public unsafe struct InfComponentsSpelling17
  {
    public       byte  unknown1;
    public       short left;
    public       short top;
    public       short width;
    public       short height;
    public       byte  displayAt;
    public fixed byte  unknown2[8];
    public       int   registryId;
    public fixed byte  unknown3[2];
    public       byte  colorRed;
    public       byte  colorGreen;
    public       byte  colorBlue;
    public fixed byte  unknown4[8];
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 26)]
  public unsafe struct InfComponentsImage17
  {
    public       byte  unknown1;
    public       short left;
    public       short top;
    public       short width;
    public       short height;
    public       byte  displayAt;
    public fixed byte  unknown2[8];
    public       int   registryId;
    public       byte  stretchType;
    public fixed byte  unknown3[3];
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 49)]
  public unsafe struct InfComponentsSound17
  {
    public       byte  unknown1;
    public       short left;
    public       short top;
    public       short width;
    public       short height;
    public       byte  displayAt;
    public fixed byte  unknown2[8];
    public       int   registryId;
    public fixed byte  unknown3[8];
    public       byte  textAlignment;
    public       byte  unknown4;
    public       byte  colorRed;
    public       byte  colorGreen;
    public       byte  colorBlue;
    public       byte  unknown5;
    public       uint  extractStart;
    public       uint  extractStop;
    public       bool  isContinuous;
    public fixed byte  unknown6[2];
    public       byte  playAt;
    public       byte  panel;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 32)]
  public unsafe struct InfComponentsVideo17
  {
    public       byte  unknown1;
    public       short left;
    public       short top;
    public       short width;
    public       short height;
    public       byte  displayAt;
    public fixed byte  unknown2[4];
    public       bool  isContinuous;
    public       bool  isFullScreen;
    public fixed byte  unknown3[2];
    public       int   registryId;
    public       byte  unknown4;
    public       uint  extractStart;
    public       uint  extractStop;
    public       byte  panel;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 28)]
  public unsafe struct InfComponentsShape17
  {
    public       byte  unknown1;
    public       short left;
    public       short top;
    public       short width;
    public       short height;
    public       byte  displayAt;
    public fixed byte  unknown2[18];
  }
}
