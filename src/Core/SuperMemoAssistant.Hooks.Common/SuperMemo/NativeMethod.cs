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
// Created On:   2020/01/11 14:50
// Modified On:  2022/12/17 12:57
// Modified By:  - Alexis
//               - Ki

#endregion




// ReSharper disable InconsistentNaming
using System;

namespace SuperMemoAssistant.SuperMemo
{
  [Serializable]
  public enum NativeMethod
  {
    ElWdw_GoToElement,
    ElWdw_PasteElement,
    ElWdw_AppendElement,
    ElWdw_GenerateExtract,
    ElWdw_GenerateClozeDeletion,
    ElWdw_AddElementFromText,
    ElWdw_DeleteCurrentElement,
    ElWdw_GetText,
    ElWdw_EnterUpdateLock,
    ElWdw_QuitUpdateLock,
    ElWdw_Done,
    ElWdw_PasteArticle,
    ElWdw_SetText,
    ElWdw_NextElementInLearningQueue,
    ElWdw_SetElementState,
    ElWdw_SetGrade,
    ElWdw_GetElementAsText,
    ElWdw_BeginLearning,
    Database_SetTitle,
    Database_AppendComment,
    Priority_SetPriority,
    TPriority_GetElementPriority,
    ElWdw_ScheduleInInterval,
    ElWdw_ExecuteUncommittedRepetition,
    ElWdw_ForceRepetitionExt,
    ElWdw_RestoreLearningMode,
    ElWdw_NewTemplate,
    ElWdw_NextRepetitionClick,
    ElWdw_DismissElement,
    TCompData_GetType,
    TCompData_GetText,
    TCompData_SetText,
    TCompData_GetTextRegMember,
    TCompData_SetTextRegMember,
    TCompData_GetImageRegMember,
    TCompData_SetImageRegMember,
    TSMMain_SelectDefaultConcept,
    TRegistry_AddMember,
    TRegistry_ImportFile,
    FileSpace_GetTopSlot,
    FileSpace_IsSlotOccupied,
    Queue_Last,
    Queue_GetItem,
    Contents_FindText,
    ElWdw_BackButtonClick,
    ElWdw_ForwardButtonClick,

    // Special handling
    AppendAndAddElementFromText,
    PostponeRepetition,
    ForceRepetitionAndResume,
  }
}
