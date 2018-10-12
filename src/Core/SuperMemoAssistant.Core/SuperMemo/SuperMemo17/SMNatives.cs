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
// Created On:   2018/06/07 17:38
// Modified On:  2018/06/20 19:20
// Modified By:  Alexis

#endregion




using System;
using Process.NET.Memory;
using Process.NET.Patterns;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17
{
  public static class SMNatives
  {
    public static class Globals
    {
      #region Constants & Statics

      public static readonly IntPtr CurrentConceptIdPtr = new IntPtr(0x00BBCDD0);
      public static readonly IntPtr CurrentRootIdPtr    = new IntPtr(0x00BBCDD4);
      public static readonly IntPtr CurrentHookIdPtr    = new IntPtr(0x00BBCDD8);

      #endregion
    }


    public static class TControl
    {
      #region Constants & Statics

      public static int ParentOffset     = 0x0038;
      public static int WindowProcOffset = 0x0040;
      public static int HandleOffset     = 0x0270;

      public static int LeftOffset   = 0x0048;
      public static int TopOffset    = 0x004C;
      public static int WidthOffset  = 0x0050;
      public static int HeightOffset = 0x0054;

      #endregion
    }


    public static class TElWind
    {
      #region Constants & Statics

      public static IntPtr InstancePtr = new IntPtr(0x00BC00F0);

      public static readonly ObjPtr ElementIdPtr = new ObjPtr(InstancePtr,
                                                              0x0DDD);
      public static readonly ObjPtr ObjectsPtr = new ObjPtr(InstancePtr,
                                                            0x0F71);
      public static readonly ObjPtr ComponentsDataPtr = new ObjPtr(InstancePtr,
                                                                   0x1101);
      public static readonly ObjPtr FocusedComponentPtr = new ObjPtr(InstancePtr,
                                                                     0x11DF);

      // Address = 4 first ? wildcards
      public static readonly IMemoryPattern GoToElementCallSig = new DwordCallPattern(
        "E8 ? ? ? ? 80 7D FB 23",
        1);
      public static readonly IMemoryPattern PasteElementCallSig = new DwordCallPattern(
        "E8 ? ? ? ? EB 05 E8 ? ? ? ? 59 59 5D C3 8B C0",
        1);
      public static readonly IMemoryPattern DeleteCurrentElementCallSig = new DwordCallPattern(
        "E8 ? ? ? ? A1 ? ? ? ? 8B 00 8B 55 F8 E8 ? ? ? ? 84 C0 75 04",
        1);

      // Static Addresses
      public static readonly IMemoryPattern DoneSig =
        new DwordFunctionPattern("55 8B EC 81 C4 ? ? ? ? 53 56 57 89 45 FC 33 C0 55 68 ? ? ? ? 64 FF 30 64 89 20 E8");
      public static readonly IMemoryPattern PasteArticleSig =
        new DwordFunctionPattern("55 8B EC 51 89 45 FC 33 D2 8B 45 FC E8 ? ? ? ? 84 C0");

      #endregion




      public static class TComponentData
      {
        #region Constants & Statics

        public static readonly int ComponentDataArrOffset     = 0x003F;
        public static readonly int ComponentDataArrItemLength = 0x0D;

        public static readonly ObjPtr ComponentCountPtr = new ObjPtr(ComponentsDataPtr,
                                                                     0x000A);
        public static readonly ObjPtr IsModifiedPtr = new ObjPtr(ComponentsDataPtr,
                                                                 0x0013);

        public static readonly IMemoryPattern GetTypeCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 3A 45 EF",
          1);
        public static readonly IMemoryPattern GetTextCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 83 7D F4 00 74 25 8B 0D ? ? ? ?",
          1);
        public static readonly IMemoryPattern SetTextCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 0F BE 55 F3",
          1);
        // GetTextItem
        public static readonly IMemoryPattern GetTextRegMemberCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 89 45 E8 8A 55 DE",
          1);
        // SetTextItem
        public static readonly IMemoryPattern SetTextRegMemberCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 8A 55 EF 8B 45 F0",
          1);
        // GetImageItem
        public static readonly IMemoryPattern GetImageRegMemberCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 89 45 F4 8B 45 F4 8B E5 5D C3 90 55 8B EC 83 C4 F4 89 4D F4 88 55 FB 89 45 FC 8B 4D F4 8A 55 FB 8B 45 FC 8B 80 ? ? ? ? E8 ? ? ? ? 8B E5 5D C3 90 55 8B EC 51",
          1);
        // SetImageItem
        public static readonly IMemoryPattern SetImageRegMemberCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 88 45 F3 EB 4A",
          1);

        #endregion




        #region Methods

        public static ObjPtr ComponentTypePtr(int idx) => new ObjPtr(FocusedComponentPtr,
                                                                     ComponentDataArrOffset + idx * ComponentDataArrItemLength);

        #endregion
      }
    }


    public static class TSMMain
    {
      #region Constants & Statics

      public static IntPtr InstancePtr = new IntPtr(0x00BBFE80);

      // Address = 4 first ? wildcards
      public static readonly IMemoryPattern SelectDefaultConceptCallSig = new DwordCallPattern(
        "E8 ? ? ? ? B1 01 8B 55 F8 8B 45 FC E8 ? ? ? ? 33 C0",
        1);

      #endregion
    }
  }
}
