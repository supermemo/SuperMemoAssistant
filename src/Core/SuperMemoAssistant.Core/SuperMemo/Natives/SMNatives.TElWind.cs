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

#endregion




// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.SuperMemo.Natives
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.Runtime.InteropServices;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows;
  using Anotar.Serilog;
  using Extensions;
  using Interop.SuperMemo.Content.Controls;
  using Interop.SuperMemo.Elements.Models;
  using Interop.SuperMemo.Registry.Members;
  using Interop.SuperMemo.Registry.Models;
  using Process.NET.Execution;
  using Process.NET.Memory;
  using Process.NET.Native.Types;
  using Process.NET.Patterns;
  using Process.NET.Types;
  using SMA;

  public partial class SMNatives
  {
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
    public partial class TElWind
    {
      #region Constructors

      public TElWind(NativeData nativeData)
      {
        // TElWind object pointer
        InstancePtr = new IntPtr(nativeData.Pointers[NativePointer.ElWdw_InstancePtr]);

        // Object pointers
        ElementIdPtr        = new ObjPtr(InstancePtr, nativeData.Pointers[NativePointer.ElWdw_ElementIdPtr]);
        ObjectsPtr          = new ObjPtr(InstancePtr, nativeData.Pointers[NativePointer.ElWdw_ObjectsPtr]);
        ComponentsDataPtr   = new ObjPtr(InstancePtr, nativeData.Pointers[NativePointer.ElWdw_ComponentsDataPtr]);
        RecentGradePtr      = new ObjPtr(InstancePtr, nativeData.Pointers[NativePointer.ElWdw_RecentGradePtr]);
        FocusedComponentPtr = new ObjPtr(InstancePtr, nativeData.Pointers[NativePointer.ElWdw_FocusedComponentPtr]);
        LearningModePtr     = new ObjPtr(InstancePtr, nativeData.Pointers[NativePointer.ElWdw_LearningModePtr]);

        // Memory patterns
        EnterUpdateLockCallSig = nativeData.GetMemoryPattern(NativeMethod.ElWdw_EnterUpdateLock);
        QuitUpdateLockCallSig  = nativeData.GetMemoryPattern(NativeMethod.ElWdw_QuitUpdateLock);

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
        Components = new TComponentData(ComponentsDataPtr, nativeData);
      }

      #endregion




      #region Properties & Fields - Public

      //
      // Object pointers

      public IntPtr InstancePtr { get; }

      public ObjPtr ElementIdPtr        { get; } // TElWind.LoadedElement
      public ObjPtr ObjectsPtr          { get; }
      public ObjPtr ComponentsDataPtr   { get; } // ComponentData:TComponentData ... or ... call TComponentData.GetCors
      public ObjPtr RecentGradePtr      { get; }
      public ObjPtr FocusedComponentPtr { get; } // TElWind.TheCurrentComponent
      public ObjPtr LearningModePtr     { get; }


      public IMemoryPattern EnterUpdateLockCallSig { get; }
      public IMemoryPattern QuitUpdateLockCallSig  { get; }


      // TElWind.EnterUpdateLock
      public Procedure<Action<IntPtr, bool, DelphiUTF16String>> EnterUpdateLock { get; }

      // TElWind.QuitUpdateLock
      public Procedure<Action<IntPtr, bool>> QuitUpdateLock { get; }

      public TComponentData Components { get; }

      #endregion




      #region Methods

      // TElWind.NewElement
      public bool GoToElement(IntPtr elementWdwPtr, int elementId)
      {
        try
        {
          return NativeMethod.ElWdw_GoToElement.ExecuteOnMainThread(
            elementWdwPtr,
            elementId) == 1;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public bool PasteElement(IntPtr elementWdwPtr)
      {
        try
        {
          return NativeMethod.ElWdw_PasteElement.ExecuteOnMainThread(
            elementWdwPtr) == 1;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public int AppendElement(IntPtr elementWdwPtr, ElementType elementType)
      {
        try
        {
          return NativeMethod.ElWdw_AppendElement.ExecuteOnMainThread(
            elementWdwPtr,
            (byte)elementType,
            0 /* ?? */);
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return -1;
        }
      }

      [SuppressMessage("Style", "RCS1001:Add braces (when expression spans over multiple lines).",
                       Justification = "<Pending>")]
      [SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits",
                       Justification = "<Pending>")]
      [SuppressMessage("Usage", "VSTHRD104:Offer async methods", Justification = "<Pending>")]
      [SuppressMessage("AsyncUsage", "AsyncFixer04:A disposable object used in a fire & forget async call",
                       Justification = "<Pending>")]
      public int GenerateExtract(IntPtr      elementWdwPtr,
                                 ElementType elementType,
                                 bool        memorize                  = true,
                                 bool        askUserToScheduleInterval = false)
      {
        using (var cts = new CancellationTokenSource(5000))
          try
          {
            var waitForElemIdTask = Core.SM.Registry.Element.WaitForNextCreatedElementAsync(cts.Token);

            NativeMethod.ElWdw_GenerateExtract.ExecuteOnMainThread(
              elementWdwPtr,
              (byte)elementType,
              memorize,
              askUserToScheduleInterval);

            return waitForElemIdTask.Result;
          }
          catch (AggregateException aggEx) when (aggEx.InnerException is TaskCanceledException)
          {
            LogTo.Warning("Couldn't find element id for GenerateExtract");

            return -1;
          }
          catch (Exception ex)
          {
            LogTo.Error(ex, "Native method call threw an exception.");
            cts.Cancel();

            return -1;
          }
      }

      [SuppressMessage("Style", "RCS1001:Add braces (when expression spans over multiple lines).",
                       Justification = "<Pending>")]
      [SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits",
                       Justification = "<Pending>")]
      [SuppressMessage("Usage", "VSTHRD104:Offer async methods", Justification = "<Pending>")]
      [SuppressMessage("AsyncUsage", "AsyncFixer04:A disposable object used in a fire & forget async call",
                       Justification = "<Pending>")]
      public int GenerateCloze(IntPtr elementWdwPtr, bool memorize = true, bool askUserToScheduleInterval = false)
      {
        using (var cts = new CancellationTokenSource(5000))
          try
          {
            var waitForElemIdTask = Core.SM.Registry.Element.WaitForNextCreatedElementAsync(cts.Token);

            NativeMethod.ElWdw_GenerateClozeDeletion.ExecuteOnMainThread(
              elementWdwPtr,
              memorize,
              askUserToScheduleInterval);

            return waitForElemIdTask.Result;
          }
          catch (AggregateException aggEx) when (aggEx.InnerException is TaskCanceledException)
          {
            LogTo.Warning("Couldn't find element id for GenerateExtract");

            return -1;
          }
          catch (Exception ex)
          {
            LogTo.Error(ex, "Native method call threw an exception.");
            cts.Cancel();

            return -1;
          }
      }

      public bool SetElementFromDescription(IntPtr elementWdwPtr, string elementDesc)
      {
        try
        {
          return NativeMethod.ElWdw_AddElementFromText.ExecuteOnMainThread(
            elementWdwPtr,
            new DelphiUTF16String(elementDesc)) > 0;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public bool NextRepetition(IntPtr elementWdwPtr)
      {
        try
        {
          return NativeMethod.ElWdw_NextRepetitionClick.ExecuteOnMainThread(
            elementWdwPtr) == 1;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public bool ForwardButtonClick(IntPtr elementWdwPtr)
      {
        try
        {
          return NativeMethod.ElWdw_ForwardButtonClick.ExecuteOnMainThread(
            elementWdwPtr) == 1;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }
      public bool BackButtonClick(IntPtr elementWdwPtr)
      {
        try
        {
          return NativeMethod.ElWdw_BackButtonClick.ExecuteOnMainThread(
            elementWdwPtr) == 1;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public bool DeleteCurrentElement(IntPtr elementWdwPtr)
      {
        try
        {
          return NativeMethod.ElWdw_DeleteCurrentElement.ExecuteOnMainThread(
            elementWdwPtr) == 1;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      [SuppressMessage("Microsoft.Performance", "CA1801")]
      public string GetText(IntPtr elementWdwPtr, IControl control)
      {
        return null;

        // TODO: Add out parameters to Process.NET
        //try
        //{
        //  var ret = new DelphiUString(8000);

        //  GetTextMethod(ElementWdwPtr.Read<IntPtr>(),
        //                control.Id + 1,
        //                ret);

        //  return ret.Text;
        //}
        //catch (Exception ex)
        //{
        //  return null;
        //}
      }

      public bool ApplyTemplate(IntPtr elementWdwPtr, int templateId, TemplateUseMode templateUseMode)
      {
        try
        {
          NativeMethod.ElWdw_NewTemplate.ExecuteOnMainThread(
            elementWdwPtr,
            templateId,
            (int)templateUseMode);

          return true;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public bool ShowNextElementInLearningQueue(IntPtr elementWdwPtr)
      {
        try
        {
          NativeMethod.ElWdw_NextElementInLearningQueue.ExecuteOnMainThread(
            elementWdwPtr);

          return true;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public bool SetElementState(IntPtr elementWdwPtr, ElementDisplayState state)
      {
        try
        {
          NativeMethod.ElWdw_SetElementState.ExecuteOnMainThread(
            elementWdwPtr,
            (int)state);

          return true;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public bool AppendComment(IntPtr databasePtr, int elementId, string comment)
      {
        try
        {
          NativeMethod.Database_AppendComment.ExecuteOnMainThread(
            databasePtr,
            elementId,
            new DelphiUTF16String(comment));

          return true;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public bool SetPriority(int elementId, double priority)
      {
        try
        {
          NativeMethod.Priority_SetPriority.ExecuteOnMainThread(
            elementId,
            (Int64)priority);

          return true;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public bool SetTitle(IntPtr databasePtr, int elementId, string title)
      {
        try
        {
          NativeMethod.Database_SetTitle.ExecuteOnMainThread(
            databasePtr,
            elementId,
            new DelphiUTF16String(title));

          return true;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public bool SetGrade(IntPtr elementWdwPtr, int grade)
      {
        try
        {
          NativeMethod.ElWdw_SetGrade.ExecuteOnMainThread(
            elementWdwPtr,
            (int)grade);

          return true;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public string GetElementAsText(IntPtr elementWdwPtr)
      {
        try
        {
          dynamic ret = "";
          if (NativeMethod.ElWdw_GetElementAsText.ExecuteOnMainThreadWithOutParameter(out ret, elementWdwPtr) == 0)
          {
            return null;
          }
          return (string)ret;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return null;
        }
      }

      public bool BeginLearning(IntPtr elementWdwPtr, int learningMode)
      {
        try
        {
          NativeMethod.ElWdw_BeginLearning.ExecuteOnMainThread(elementWdwPtr, learningMode);
          return true;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public float GetElementPriority(int elementNumber)
      {
        throw new NotImplementedException();
        /* TODO this doesn't work because there's no way to get the ST0 float value
        try
        {
          var priority = NativeMethod.TPriority_GetElementPriority.ExecuteOnMainThread(elementNumber);

          return priority;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return 0;
        }*/
      }

      public bool PostponeRepetition(IntPtr elementWdwPtr, int interval)
      {
        try
        {
          NativeMethod.PostponeRepetition.ExecuteOnMainThread(
            elementWdwPtr,
            interval);

          return true;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public bool ForceRepetitionAndResume(IntPtr elementWdwPtr,
                                           int    interval,
                                           bool   adjustPriority)
      {
        try
        {
          NativeMethod.ForceRepetitionAndResume.ExecuteOnMainThread(
            elementWdwPtr,
            interval,
            adjustPriority);

          return true;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public int AppendAndAddElementFromText(IntPtr      elementWdwPtr,
                                             ElementType elementType,
                                             string      elementDesc)
      {
        try
        {
          return NativeMethod.AppendAndAddElementFromText.ExecuteOnMainThread(
            elementWdwPtr,
            (byte)elementType,
            new DelphiUTF16String(elementDesc));
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return -1;
        }
      }

      public bool DismissElement(IntPtr elementWdwPtr, int elNo)
      {
        try
        {
          return NativeMethod.ElWdw_DismissElement.ExecuteOnMainThread(
            elementWdwPtr,
            elNo,
            0) == 1;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      // Static addresses
      // _Unit193.TElWind.DoneClick
      // 00A0E29C       call        00A20A58
      public bool Done(IntPtr elementWdwPtr)
      {
        try
        {
          return NativeMethod.ElWdw_Done.ExecuteOnMainThread(
            elementWdwPtr) == 1;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public bool PasteArticle(IntPtr elementWdwPtr)
      {
        try
        {
          return NativeMethod.ElWdw_PasteArticle.ExecuteOnMainThread(
            elementWdwPtr) == 1;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public bool SetText(IntPtr   elementWdwPtr,
                          IControl control,
                          string   text)
      {
        try
        {
          //SetTextMethod(ElementWdwPtr.Read<IntPtr>(),
          //              control.Id + 1,
          //              new DelphiUString(text));

          //return true;

          return NativeMethod.ElWdw_SetText.ExecuteOnMainThread(
            elementWdwPtr,
            control.Id + 1,
            new DelphiUTF16String(text)) == 1;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      public bool ForceRepetitionExt(IntPtr elementWdwPtr,
                                     int    interval,
                                     bool   adjustPriority)
      {
        try
        {
          NativeMethod.ElWdw_ForceRepetitionExt.ExecuteOnMainThread(
            elementWdwPtr,
            interval,
            adjustPriority);

          return true;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }

      #endregion
    }
  }
}
