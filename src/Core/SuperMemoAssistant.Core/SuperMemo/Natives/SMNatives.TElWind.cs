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
// Created On:   2020/01/11 15:02
// Modified On:  2020/01/12 15:01
// Modified By:  Alexis

#endregion




using System;
using System.Threading;
using System.Threading.Tasks;
using Anotar.Serilog;
using Process.NET.Execution;
using Process.NET.Memory;
using Process.NET.Native.Types;
using Process.NET.Patterns;
using Process.NET.Types;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.SMA;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.SuperMemo.Natives
{
  public partial class SMNatives
  {
    public partial class TElWind
    {
      #region Constructors

      public TElWind(NativeData nativeData)
      {
        // TElWind object pointer
        InstancePtr = new IntPtr(nativeData.Pointers[NativePointers.ElWdw_InstancePtr]);

        // Object pointers
        ElementIdPtr        = new ObjPtr(InstancePtr, nativeData.Pointers[NativePointers.ElWdw_ElementIdPtr]);
        ObjectsPtr          = new ObjPtr(InstancePtr, nativeData.Pointers[NativePointers.ElWdw_ObjectsPtr]);
        ComponentsDataPtr   = new ObjPtr(InstancePtr, nativeData.Pointers[NativePointers.ElWdw_ComponentsDataPtr]);
        RecentGradePtr      = new ObjPtr(InstancePtr, nativeData.Pointers[NativePointers.ElWdw_RecentGradePtr]);
        FocusedComponentPtr = new ObjPtr(InstancePtr, nativeData.Pointers[NativePointers.ElWdw_FocusedComponentPtr]);
        LearningModePtr     = new ObjPtr(InstancePtr, nativeData.Pointers[NativePointers.ElWdw_LearningModePtr]);

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

      public int GenerateExtract(IntPtr elementWdwPtr, ElementType elementType, bool memorize = true, bool askUserToScheduleInterval = false)
      {
        using (var cts = new CancellationTokenSource(5000))
          try
          {
            var waitForElemIdTask = Core.SM.Registry.Element.WaitForNextCreatedElement(cts.Token);

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

      public int GenerateCloze(IntPtr elementWdwPtr, bool memorize = true, bool askUserToScheduleInterval = false)
      {
        using (var cts = new CancellationTokenSource(5000))
          try
          {
            var waitForElemIdTask = Core.SM.Registry.Element.WaitForNextCreatedElement(cts.Token);
            
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

      public bool SetElementState(IntPtr elementWdwPtr, int state)
      {
        try
        {
          NativeMethod.ElWdw_SetElementState.ExecuteOnMainThread(
            elementWdwPtr,
            state);

          return true;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
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
