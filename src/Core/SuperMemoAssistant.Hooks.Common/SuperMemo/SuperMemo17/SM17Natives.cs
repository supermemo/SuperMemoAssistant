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
// Created On:   2019/03/02 18:29
// Modified On:  2019/12/14 20:02
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using Process.NET.Patterns;
using SuperMemoAssistant.SuperMemo.Common;

// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

namespace SuperMemoAssistant.SuperMemo.SuperMemo17
{
  public partial class SM17Natives : SuperMemoNatives
  {
    #region Constructors

    /// <inheritdoc />
    public SM17Natives()
    {
      Globals     = new TGlobals17();
      Application = new TApplication17();
      Control     = new TControl17();
      ElWind      = new TElWind17();
      SMMain      = new TSMMain17();
      Registry    = new TRegistry17();

      MethodsPatterns = new Dictionary<NativeMethod, IMemoryPattern>
      {
        { NativeMethod.ElWdw_GoToElement, ElWind.GoToElementCallSig },
        { NativeMethod.ElWdw_AppendElement, ElWind.AppendElementCallSig },
        { NativeMethod.ElWdw_AddElementFromText, ElWind.AddElementFromTextCallSig },
        { NativeMethod.ElWdw_DeleteCurrentElement, ElWind.DeleteCurrentElementCallSig },
        { NativeMethod.ElWdw_Done, ElWind.DoneCallSig },
        { NativeMethod.ElWdw_SetText, ElWind.SetTextCallSig },
        { NativeMethod.ElWdw_NextElementInLearningQueue, ElWind.ShowNextElementInLearningQueueCallSig },
        { NativeMethod.ElWdw_SetElementState, ElWind.SetElementStateCallSig },
        { NativeMethod.ElWdw_ScheduleInInterval, ElWind.ScheduleInIntervalCallSig },
        { NativeMethod.ElWdw_ExecuteUncommitedRepetition, ElWind.ExecuteUncommittedRepetitionCallSig },
        { NativeMethod.ElWdw_ForceRepetitionExt, ElWind.ForceRepetitionExtCallSig },
        { NativeMethod.ElWdw_RestoreLearningMode, ElWind.RestoreLearningModeCallSig },
      };
    }

    #endregion




    #region Properties Impl - Public

    public override Dictionary<NativeMethod, IMemoryPattern> MethodsPatterns { get; }

    public override TGlobals     Globals     { get; }
    public override TApplication Application { get; }
    public override TControl     Control     { get; }
    public override TElWind      ElWind      { get; }
    public override TSMMain      SMMain      { get; }
    public override TRegistry    Registry    { get; }

    #endregion
  }
}
