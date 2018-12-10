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
// Modified On:  2018/12/10 13:17
// Modified By:  Alexis

#endregion




using System;
using Process.NET.Memory;
using Process.NET.Patterns;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberHidesStaticFromOuterClass

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

      public static readonly IntPtr IgnoreUserConfirmationPtr = new IntPtr(0x00BC0007);

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

      public static readonly IntPtr InstancePtr = new IntPtr(0x00BC00F0);

      // TElWind.LoadedElement
      public static readonly ObjPtr ElementIdPtr = new ObjPtr(InstancePtr,
                                                              0x0D81);
      public static readonly ObjPtr ObjectsPtr = new ObjPtr(InstancePtr,
                                                            0x0F71);
      public static readonly ObjPtr ComponentsDataPtr = new ObjPtr(InstancePtr,
                                                                   0x1101);
      public static readonly ObjPtr RecentGradePtr = new ObjPtr(InstancePtr,
                                                                0x1105);
      public static readonly ObjPtr FocusedComponentPtr = new ObjPtr(InstancePtr,
                                                                     0x11DF);
      public static readonly ObjPtr LearningModePtr = new ObjPtr(InstancePtr,
                                                                 0x11E5);

      //
      // Call addresses (addr = 4 first ? wildcards)

      // TElWind.GoToElement
      public static readonly IMemoryPattern GoToElementCallSig = new DwordCallPattern(
        "E8 ? ? ? ? 80 7D FB 23",
        1);
      // TElWind.PasteElement
      public static readonly IMemoryPattern PasteElementCallSig = new DwordCallPattern(
        "E8 ? ? ? ? EB 05 E8 ? ? ? ? 59 59 5D C3 8B C0",
        1);
      // TElWind.AppendElement
      public static readonly IMemoryPattern AppendElementCallSig = new DwordCallPattern(
        "E8 ? ? ? ? A1 ? ? ? ? 8B 00 8B 4D F8 B2 01",
        1);
      // TElWind.AddElementFromText
      public static readonly IMemoryPattern AddElementFromTextCallSig = new DwordCallPattern(
        "E8 ? ? ? ? 8B 45 FC E8 ? ? ? ? 8D 45 D4",
        1);
      // TElWind.DeleteCurrentElement
      public static readonly IMemoryPattern DeleteCurrentElementCallSig = new DwordCallPattern(
        "E8 ? ? ? ? A1 ? ? ? ? 8B 00 8B 55 F8 E8 ? ? ? ? 84 C0 75 04",
        1);
      // TElWind.GetText
      public static readonly IMemoryPattern GetTextCallSig = new DwordCallPattern(
        "E8 ? ? ? ? 8A 45 F3 84 C0",
        1);
      // TElWind.EnterUpdateLock
      public static readonly IMemoryPattern EnterUpdateLockCallSig = new DwordCallPattern(
        "E8 ? ? ? ? 8D 4D E6",
        1);
      // TElWind.QuitUpdateLock
      public static readonly IMemoryPattern QuitUpdateLockCallSig = new DwordCallPattern(
        "E8 ? ? ? ? C6 45 FA 01 EB 0C",
        1);

      //
      // Static Addresses

      // TElWind.Done
      public static readonly IMemoryPattern DoneSig =
        new DwordFunctionPattern("55 8B EC 81 C4 ? ? ? ? 53 56 57 89 45 FC 33 C0 55 68 ? ? ? ? 64 FF 30 64 89 20 E8");
      // TElWind.PasteArticle
      public static readonly IMemoryPattern PasteArticleSig =
        new DwordFunctionPattern("55 8B EC 51 89 45 FC 33 D2 8B 45 FC E8 ? ? ? ? 84 C0");
      // TElWind.SetText
      public static readonly IMemoryPattern SetTextCallSig =
        new DwordFunctionPattern(
          "55 8B EC 83 C4 F4 89 4D F4 88 55 FB 89 45 FC 8B 45 F4 E8 ? ? ? ? 33 C0 55 68 ? ? ? ? 64 FF 30 64 89 20 8B 4D F4 8A 55 FB 8B 45 FC 8B 80 ? ? ? ? E8 ? ? ? ? 33 C0 5A");

      #endregion




      public static class TComponentData
      {
        #region Constants & Statics

        public const int ComponentDataArrOffset     = 0x003F;
        public const int ComponentDataArrItemLength = 0x0D;

        public static readonly ObjPtr ComponentCountPtr = new ObjPtr(ComponentsDataPtr,
                                                                     0x000A);
        public static readonly ObjPtr IsModifiedPtr = new ObjPtr(ComponentsDataPtr,
                                                                 0x0013);

        // TComponentData.GetType
        public static readonly IMemoryPattern GetTypeCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 3A 45 EF",
          1);
        // TComponentData.GetText
        public static readonly IMemoryPattern GetTextCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 83 7D F4 00 74 25 8B 0D ? ? ? ?",
          1);
        // TComponentData.SetText
        public static readonly IMemoryPattern SetTextCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 0F BE 55 F3",
          1);
        // TComponentData.GetTextItem
        public static readonly IMemoryPattern GetTextRegMemberCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 89 45 E8 8A 55 DE",
          1);
        // TComponentData.SetTextItem
        public static readonly IMemoryPattern SetTextRegMemberCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 8A 55 EF 8B 45 F0",
          1);
        // TComponentData.GetImageItem
        public static readonly IMemoryPattern GetImageRegMemberCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 89 45 F4 8B 45 F4 8B E5 5D C3 90 55 8B EC 83 C4 F4 89 4D F4 88 55 FB 89 45 FC 8B 4D F4 8A 55 FB 8B 45 FC 8B 80 ? ? ? ? E8 ? ? ? ? 8B E5 5D C3 90 55 8B EC 51",
          1);
        // TComponentData.SetImageItem
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

      public static IntPtr InstancePtr = new IntPtr(0x00BBFE80); // TODO: Update to 17.4

      // TSMMain.SelectDefaultConcept
      public static readonly IMemoryPattern SelectDefaultConceptCallSig = new DwordCallPattern(
        "E8 ? ? ? ? B1 01 8B 55 F8 8B 45 FC E8 ? ? ? ? 33 C0",
        1);

      #endregion
    }


    public static class TDatabase
    {
      #region Constants & Statics

      public static IntPtr InstancePtr => new IntPtr(0x00BBFCC0);

      #endregion
    }


    public static class TRegistry
    {
      #region Constants & Statics

      public static readonly ObjPtr TextRegistryInstance = new ObjPtr(TDatabase.InstancePtr,
                                                                      0x4E7);
      public static readonly ObjPtr ImageRegistryInstance = new ObjPtr(TDatabase.InstancePtr,
                                                                       0x4FF);
      public static readonly ObjPtr SoundRegistryInstance = new ObjPtr(TDatabase.InstancePtr,
                                                                       0x503);
      public static readonly ObjPtr VideoRegistryInstance = new ObjPtr(TDatabase.InstancePtr,
                                                                       0x507);
      public static readonly ObjPtr BinaryRegistryInstance = new ObjPtr(TDatabase.InstancePtr,
                                                                        0x513);
      public static readonly ObjPtr TemplateRegistryInstance = new ObjPtr(TDatabase.InstancePtr,
                                                                          0x517);
      public static readonly ObjPtr ConceptRegistryInstance = new ObjPtr(TDatabase.InstancePtr,
                                                                         0x527);

      // TRegistry.AddMember
      public static readonly IMemoryPattern AddMember = new DwordCallPattern(
        "E8 ? ? ? ? 89 45 E8 8D 55 EC",
        1);
      // TRegistry.ImportFile
      public static readonly IMemoryPattern ImportFile = new DwordCallPattern(
        "E8 ? ? ? ? 89 45 D8 8B 4D F4",
        1);

      #endregion
    }
  }
}
