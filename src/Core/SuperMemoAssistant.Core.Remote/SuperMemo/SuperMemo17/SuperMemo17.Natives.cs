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
// Created On:   2019/01/04 21:15
// Modified On:  2019/01/04 21:23
// Modified By:  Alexis

#endregion




using System;
using Process.NET.Execution;
using Process.NET.Memory;
using Process.NET.Native.Types;
using Process.NET.Patterns;
using Process.NET.Types;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17
{
  public class SM17Natives
  {
    #region Constants & Statics

    public static SM17Natives Instance { get; private set; }

    #endregion




    #region Constructors

    public SM17Natives()
    {
      Instance = this;
    }

    #endregion




    #region Properties & Fields - Public

    public TElWind   ElWind   { get; set; }
    public TSMMain   SMMain   { get; set; }
    public TRegistry Registry { get; set; }

    #endregion




    public static class Globals
    {
      #region Constants & Statics

      public static readonly IntPtr CurrentConceptIdPtr = new IntPtr(0x00BBCDD0);
      public static readonly IntPtr CurrentRootIdPtr    = new IntPtr(0x00BBCDD4);
      public static readonly IntPtr CurrentHookIdPtr    = new IntPtr(0x00BBCDD8);

      public static readonly IntPtr IgnoreUserConfirmationPtr = new IntPtr(0x00BC0007);

      #endregion
    }


    public static class TApplication
    {
      #region Constants & Statics

      public const           int    OnMessageOffset          = 0x110;
      public static readonly IntPtr TApplicationInstanceAddr = new IntPtr(0x00ACF3D4);
      public static readonly ObjPtr TApplicationOnMessagePtr = new ObjPtr(TApplicationInstanceAddr,
                                                                          OnMessageOffset);

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


    public class TElWind
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
      // Call signatures

      // Call addresses (addr = 4 first ? wildcards)
      public static readonly IMemoryPattern GoToElementCallSig = new DwordCallPattern(
        "E8 ? ? ? ? 80 7D FB 23",
        1);
      public static readonly IMemoryPattern PasteElementCallSig = new DwordCallPattern(
        "E8 ? ? ? ? EB 05 E8 ? ? ? ? 59 59 5D C3 8B C0",
        1);
      public static readonly IMemoryPattern AppendElementCallSig = new DwordCallPattern(
        "E8 ? ? ? ? A1 ? ? ? ? 8B 00 8B 4D F8 B2 01",
        1);
      public static readonly IMemoryPattern AddElementFromTextCallSig = new DwordCallPattern(
        "E8 ? ? ? ? 8B 45 FC E8 ? ? ? ? 8D 45 D4",
        1);
      public static readonly IMemoryPattern DeleteCurrentElementCallSig = new DwordCallPattern(
        "E8 ? ? ? ? A1 ? ? ? ? 8B 00 8B 55 F8 E8 ? ? ? ? 84 C0 75 04",
        1);
      public static readonly IMemoryPattern GetTextCallSig = new DwordCallPattern(
        "E8 ? ? ? ? 8A 45 F3 84 C0",
        1);
      public static readonly IMemoryPattern EnterUpdateLockCallSig = new DwordCallPattern(
        "E8 ? ? ? ? 8D 4D E6",
        1);
      public static readonly IMemoryPattern QuitUpdateCallSig = new DwordCallPattern(
        "E8 ? ? ? ? C6 45 FA 01 EB 0C",
        1);

      // Static addresses
      public static readonly IMemoryPattern DoneCallSig = new DwordFunctionPattern(
        "55 8B EC 81 C4 ? ? ? ? 53 56 57 89 45 FC 33 C0 55 68 ? ? ? ? 64 FF 30 64 89 20 E8");
      public static readonly IMemoryPattern PasteArticleCallSig = new DwordFunctionPattern(
        "55 8B EC 51 89 45 FC 33 D2 8B 45 FC E8 ? ? ? ? 84 C0");
      public static readonly IMemoryPattern SetTextCallSig = new DwordFunctionPattern(
        "55 8B EC 83 C4 F4 89 4D F4 88 55 FB 89 45 FC 8B 45 F4 E8 ? ? ? ? 33 C0 55 68 ? ? ? ? 64 FF 30 64 89 20 8B 4D F4 8A 55 FB 8B 45 FC 8B 80 ? ? ? ? E8 ? ? ? ? 33 C0 5A");

      #endregion




      #region Properties & Fields - Public
      /*
      // TElWind.AddElementFromText
      public Procedure<Func<IntPtr, DelphiUString, int>> AddElementFromText { get; } = new Procedure<Func<IntPtr, DelphiUString, int>>(
        "AddElement",
        CallingConventions.Register,
        AddElementFromTextCallSig
      );
      // TElWind.AppendElement
      public Procedure<Func<IntPtr, byte, byte, int>> AppendElement { get; } = new Procedure<Func<IntPtr, byte, byte, int>>(
        "AppendElement",
        CallingConventions.Register,
        AppendElementCallSig
      );

      // TElWind.DeleteCurrentElement
      public Procedure<Action<IntPtr>> DeleteCurrentElement { get; } = new Procedure<Action<IntPtr>>(
        "DeleteCurrentElement",
        CallingConventions.Register,
        DeleteCurrentElementCallSig
      );
      // TElWind.Done
      public Procedure<Action<IntPtr>> Done { get; } = new Procedure<Action<IntPtr>>(
        "Done",
        CallingConventions.Register,
        DoneCallSig
      );
      */
      // TElWind.EnterUpdateLock
      public Procedure<Action<IntPtr, bool, DelphiUString>> EnterUpdateLock { get; } = new Procedure<Action<IntPtr, bool, DelphiUString>>(
        "EnterUpdateLock",
        CallingConventions.Register,
        EnterUpdateLockCallSig
      );
      // TElWind.QuitUpdateLock
      public Procedure<Action<IntPtr, bool>> QuitUpdateLock { get; } = new Procedure<Action<IntPtr, bool>>(
        "QuitUpdateLock",
        CallingConventions.Register,
        QuitUpdateCallSig
      );
      /*
      // TElWind.GetText
      public Procedure<Action<int, DelphiUString, IntPtr>> GetText { get; } = new Procedure<Action<int, DelphiUString, IntPtr>>(
        "GetText",
        CallingConventions.Register,
        GetTextCallSig
      );
      // TElWind.SetText
      public Procedure<Action<int, DelphiUString, IntPtr>> SetText { get; } = new Procedure<Action<int, DelphiUString, IntPtr>>(
        "SetText",
        CallingConventions.Register,
        SetTextCallSig
      );

      // TElWind.GoToElement
      public Procedure<Action<IntPtr, int>> GoToElement { get; } = new Procedure<Action<IntPtr, int>>(
        "GoToElement",
        CallingConventions.Register,
        GoToElementCallSig
      );

      // TElWind.PasteArticle
      public Procedure<Action<IntPtr, bool>> PasteArticle { get; } = new Procedure<Action<IntPtr, bool>>(
        "PasteArticle",
        CallingConventions.Register,
        PasteArticleCallSig
      );
      // TElWind.PasteElement
      public Procedure<Func<IntPtr, bool>> PasteElement { get; } = new Procedure<Func<IntPtr, bool>>(
        "PasteElement",
        CallingConventions.Register,
        PasteElementCallSig
      );
      */
      public TComponentData Components { get; set; }

      #endregion




      public class TComponentData
      {
        #region Constants & Statics

        public const int ComponentDataArrOffset     = 0x003F;
        public const int ComponentDataArrItemLength = 0x0D;

        public static readonly ObjPtr ComponentCountPtr = new ObjPtr(ComponentsDataPtr,
                                                                     0x000A);
        public static readonly ObjPtr IsModifiedPtr = new ObjPtr(ComponentsDataPtr,
                                                                 0x0013);


        //
        // Call sigs
        /*
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
          */
        #endregion




        #region Properties & Fields - Public
        /*
        public Procedure<Func<int, IntPtr, int>> GetImageRegMember { get; } = new Procedure<Func<int, IntPtr, int>>(
          "GetImageRegMember",
          CallingConventions.Register,
          GetImageRegMemberCallSig
        );

        public Procedure<Func<IntPtr, int, string>> GetText { get; } = new Procedure<Func<IntPtr, int, string>>(
          "GetText",
          CallingConventions.Register,
          GetTextCallSig
        );

        public Procedure<Func<int, IntPtr, int>> GetTextRegMember { get; } = new Procedure<Func<int, IntPtr, int>>(
          "GetTextRegMember",
          CallingConventions.Register,
          GetTextRegMemberCallSig
        );
        public Procedure<Action<IntPtr, int, int>> SetImageRegMember { get; } = new Procedure<Action<IntPtr, int, int>>(
          "SetImageRegMember",
          CallingConventions.Register,
          SetImageRegMemberCallSig
        );
        public Procedure<Action<IntPtr, int, DelphiUString>> SetText { get; } = new Procedure<Action<IntPtr, int, DelphiUString>>(
          "SetText",
          CallingConventions.Register,
          SetTextCallSig
        );
        public Procedure<Action<int, int, IntPtr>> SetTextRegMember { get; } = new Procedure<Action<int, int, IntPtr>>(
          "SetText",
          CallingConventions.Register,
          SetTextRegMemberCallSig
        );
        */
        #endregion




        #region Methods

        public static ObjPtr ComponentTypePtr(int idx) => new ObjPtr(FocusedComponentPtr,
                                                                     ComponentDataArrOffset + idx * ComponentDataArrItemLength);

        #endregion
      }
    }


    public class TSMMain
    {
      #region Constants & Statics

      public static IntPtr InstancePtr = new IntPtr(0x00BBFE80);

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


    public class TRegistry
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
      public static readonly IMemoryPattern AddMemberCallSig = new DwordCallPattern(
        "E8 ? ? ? ? 89 45 E8 8D 55 EC",
        1);
      // TRegistry.ImportFile
      public static readonly IMemoryPattern ImportFileCallSig = new DwordCallPattern(
        "E8 ? ? ? ? 89 45 D8 8B 4D F4",
        1);

      #endregion




      #region Properties & Fields - Public

      public Procedure<Func<IntPtr, DelphiUString, int>> AddMember { get; } = new Procedure<Func<IntPtr, DelphiUString, int>>(
        "AddMember",
        CallingConventions.Register,
        AddMemberCallSig
      );
      public Procedure<Func<IntPtr, DelphiUString, DelphiUString, int>> ImportFile { get; } =
        new Procedure<Func<IntPtr, DelphiUString, DelphiUString, int>>(
          "ImportFile",
          CallingConventions.Register,
          ImportFileCallSig
        );

      #endregion
    }
  }
}
