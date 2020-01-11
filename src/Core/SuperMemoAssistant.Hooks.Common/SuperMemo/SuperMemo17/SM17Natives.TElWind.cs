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
// Created On:   2019/08/09 18:25
// Modified On:  2019/12/14 20:00
// Modified By:  Alexis

#endregion




using System;
using Process.NET.Execution;
using Process.NET.Memory;
using Process.NET.Native.Types;
using Process.NET.Patterns;
using Process.NET.Types;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.SuperMemo.SuperMemo17
{
  public partial class SM17Natives
  {
    public partial class TElWind17 : TElWind
    {
      /// <inheritdoc />
      public TElWind17()
      {
        // TElWind object pointer
        InstancePtr = new IntPtr(0x00BC00F0);

        // Object pointers
        ElementIdPtr        = new ObjPtr(InstancePtr, 0x0D81);
        ObjectsPtr          = new ObjPtr(InstancePtr, 0x0F71);
        ComponentsDataPtr   = new ObjPtr(InstancePtr, 0x1101);
        RecentGradePtr      = new ObjPtr(InstancePtr, 0x1105);
        FocusedComponentPtr = new ObjPtr(InstancePtr, 0x11DF);
        LearningModePtr     = new ObjPtr(InstancePtr, 0x11E5);

        // Call signatures
        GoToElementCallSig                    = new DwordCallPattern("E8 ? ? ? ? 80 7D FB 23", 1);
        PasteElementCallSig                   = new DwordCallPattern("E8 ? ? ? ? EB 05 E8 ? ? ? ? 59 59 5D C3 8B C0", 1);
        AppendElementCallSig                  = new DwordCallPattern("E8 ? ? ? ? A1 ? ? ? ? 8B 00 8B 4D F8 B2 01", 1);
        AddElementFromTextCallSig             = new DwordCallPattern("E8 ? ? ? ? 8B 45 FC E8 ? ? ? ? 8D 45 D4", 1);
        DeleteCurrentElementCallSig           = new DwordCallPattern("E8 ? ? ? ? A1 ? ? ? ? 8B 00 8B 55 F8 E8 ? ? ? ? 84 C0 75 04", 1);
        GetTextCallSig                        = new DwordCallPattern("E8 ? ? ? ? 8A 45 F3 84 C0", 1);
        EnterUpdateLockCallSig                = new DwordCallPattern("E8 ? ? ? ? 8D 4D E6", 1);
        QuitUpdateLockCallSig                 = new DwordCallPattern("E8 ? ? ? ? C6 45 FA 01 EB 0C", 1);
        ShowNextElementInLearningQueueCallSig = new DwordCallPattern("E8 ? ? ? ? 59 5D C3 33 D2 8B 45 FC E8 ? ? ? ? B2 02", 1);
        SetElementStateCallSig                = new DwordCallPattern("E8 ? ? ? ? EB 7C B2 02", 1);
        ExecuteUncommittedRepetitionCallSig    = new DwordCallPattern("E8 ? ? ? ? 8D 45 EB", 1);
        ScheduleInIntervalCallSig             = new DwordCallPattern("E8 ? ? ? ? 80 BD ? ? ? ? ? 75 45 8D 45 8E", 1);

        // Static addresses
        DoneCallSig         = new DwordFunctionPattern("55 8B EC 81 C4 ? ? ? ? 53 56 57 89 45 FC 33 C0 55 68 ? ? ? ? 64 FF 30 64 89 20 E8");
        PasteArticleCallSig = new DwordFunctionPattern("55 8B EC 51 89 45 FC 33 D2 8B 45 FC E8 ? ? ? ? 84 C0");
        SetTextCallSig =
          new DwordFunctionPattern(
            "55 8B EC 83 C4 F4 89 4D F4 88 55 FB 89 45 FC 8B 45 F4 E8 ? ? ? ? 33 C0 55 68 ? ? ? ? 64 FF 30 64 89 20 8B 4D F4 8A 55 FB 8B 45 FC 8B 80 ? ? ? ? E8 ? ? ? ? 33 C0 5A");
        ForceRepetitionExtCallSig  = new DwordFunctionPattern("55 8B EC 81 C4 ? ? ? ? 88 4D F7 89 55 F8 89 45 FC 8D 4D 81");
        RestoreLearningModeCallSig = new DwordFunctionPattern("55 8B EC 51 89 45 FC 8B 45 FC 80 B8 ? ? ? ? ? 75 1E");

        // Procedures
        EnterUpdateLock = new Procedure<Action<IntPtr, bool, DelphiUTF16String>>(
          "EnterUpdateLock",
          CallingConventions.Register,
          EnterUpdateLockCallSig
        );
        QuitUpdateLock = new Procedure<Action<IntPtr, bool>>(
          "QuitUpdateLock",
          CallingConventions.Register,
          QuitUpdateLockCallSig
        );

        // Substructure
        Components = new TComponentData17(this);
      }




      #region Constants & Statics

      //
      // Object pointers

      public override IntPtr InstancePtr { get; }

      public override ObjPtr ElementIdPtr        { get; } // TElWind.LoadedElement
      public override ObjPtr ObjectsPtr          { get; }
      public override ObjPtr ComponentsDataPtr   { get; }
      public override ObjPtr RecentGradePtr      { get; }
      public override ObjPtr FocusedComponentPtr { get; }
      public override ObjPtr LearningModePtr     { get; }


      //
      // Call signatures

      // Call addresses (addr = 4 first ? wildcards)
      public override IMemoryPattern GoToElementCallSig                    { get; }
      public override IMemoryPattern PasteElementCallSig                   { get; }
      public override IMemoryPattern AppendElementCallSig                  { get; }
      public override IMemoryPattern AddElementFromTextCallSig             { get; }
      public override IMemoryPattern DeleteCurrentElementCallSig           { get; }
      public override IMemoryPattern GetTextCallSig                        { get; }
      public override IMemoryPattern EnterUpdateLockCallSig                { get; }
      public override IMemoryPattern QuitUpdateLockCallSig                 { get; }
      public override IMemoryPattern ShowNextElementInLearningQueueCallSig { get; }
      public override IMemoryPattern SetElementStateCallSig                { get; }
      public override IMemoryPattern ExecuteUncommittedRepetitionCallSig    { get; }
      public override IMemoryPattern ScheduleInIntervalCallSig             { get; }

      // Static addresses
      public override IMemoryPattern DoneCallSig                { get; }
      public override IMemoryPattern PasteArticleCallSig        { get; }
      public override IMemoryPattern SetTextCallSig             { get; }
      public override IMemoryPattern ForceRepetitionExtCallSig  { get; }
      public override IMemoryPattern RestoreLearningModeCallSig { get; }

      #endregion




      #region Properties & Fields - Public

#if false
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
#endif
      // TElWind.EnterUpdateLock
      public override Procedure<Action<IntPtr, bool, DelphiUTF16String>> EnterUpdateLock { get; }
      // TElWind.QuitUpdateLock
      public override Procedure<Action<IntPtr, bool>> QuitUpdateLock { get; }
#if false
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
#endif

      public override TComponentData Components { get; }

      #endregion
    }
  }
}
