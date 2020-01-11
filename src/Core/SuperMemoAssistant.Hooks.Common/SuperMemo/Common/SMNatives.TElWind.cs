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
// Created On:   2019/08/18 14:30
// Modified On:  2020/01/11 13:37
// Modified By:  Alexis

#endregion




using System;
using Process.NET.Execution;
using Process.NET.Memory;
using Process.NET.Patterns;
using Process.NET.Types;

// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

namespace SuperMemoAssistant.SuperMemo.Common
{
  public abstract partial class SuperMemoNatives
  {
    public abstract partial class TElWind
    {
      #region Methods Abs

      public abstract IntPtr InstancePtr         { get; }
      public abstract ObjPtr ElementIdPtr        { get; } // TElWind.LoadedElement
      public abstract ObjPtr ObjectsPtr          { get; }
      public abstract ObjPtr ComponentsDataPtr   { get; }
      public abstract ObjPtr RecentGradePtr      { get; }
      public abstract ObjPtr FocusedComponentPtr { get; }
      public abstract ObjPtr LearningModePtr     { get; }

      public abstract IMemoryPattern GoToElementCallSig                    { get; }
      public abstract IMemoryPattern PasteElementCallSig                   { get; }
      public abstract IMemoryPattern AppendElementCallSig                  { get; }
      public abstract IMemoryPattern AddElementFromTextCallSig             { get; }
      public abstract IMemoryPattern DeleteCurrentElementCallSig           { get; }
      public abstract IMemoryPattern GetTextCallSig                        { get; }
      public abstract IMemoryPattern EnterUpdateLockCallSig                { get; }
      public abstract IMemoryPattern QuitUpdateLockCallSig                 { get; }
      public abstract IMemoryPattern ShowNextElementInLearningQueueCallSig { get; }
      public abstract IMemoryPattern SetElementStateCallSig                { get; }
      public abstract IMemoryPattern ExecuteUncommittedRepetitionCallSig   { get; }
      public abstract IMemoryPattern ScheduleInIntervalCallSig             { get; }
      public abstract IMemoryPattern DoneCallSig                           { get; }
      public abstract IMemoryPattern PasteArticleCallSig                   { get; }
      public abstract IMemoryPattern SetTextCallSig                        { get; }
      public abstract IMemoryPattern ForceRepetitionExtCallSig             { get; }
      public abstract IMemoryPattern RestoreLearningModeCallSig            { get; }

      public abstract Procedure<Action<IntPtr, bool, DelphiUTF16String>> EnterUpdateLock { get; }
      public abstract Procedure<Action<IntPtr, bool>>                    QuitUpdateLock  { get; }

      public abstract TComponentData Components { get; }

      #endregion
    }
  }
}
