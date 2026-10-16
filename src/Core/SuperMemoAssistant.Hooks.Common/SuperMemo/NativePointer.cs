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
// Created On:   2020/01/11 15:53
// Modified On:  2022/12/17 16:00
// Modified By:  - Alexis
//               - Ki

#endregion




// ReSharper disable InconsistentNaming

using System;

namespace SuperMemoAssistant.SuperMemo
{
  [Serializable]
  public enum NativePointer
  {
    ElWdw_InstancePtr,
    ElWdw_ElementIdPtr,
    ElWdw_ObjectsPtr,
    ElWdw_ComponentsDataPtr,
    ElWdw_RecentGradePtr,
    ElWdw_FocusedComponentPtr,
    ElWdw_LearningModePtr,
    SMMain_InstancePtr,
    Database_InstancePtr,
    Contents_InstancePtr,
    Registry_FileSpaceInstance,
    Registry_TextRegistryInstance,
    Registry_CommentRegistryInstance,
    Registry_ImageRegistryInstance,
    Registry_SoundRegistryInstance,
    Registry_VideoRegistryInstance,
    Registry_BinaryRegistryInstance,
    Registry_TemplateRegistryInstance,
    Registry_ConceptRegistryInstance,
    Globals_LimitChildrenCountPtr,
    Globals_CurrentConceptGroupIdPtr,
    Globals_CurrentRootIdPtr,
    Globals_CurrentHookIdPtr,
    Globals_CurrentConceptIdPtr,
    Globals_IgnoreUserConfirmationPtr,
    FileSpace_EmptySlotsOffset,
    FileSpace_AllocatedSlotsOffset,
    ElWdw_ComponentData_ComponentDataArrOffset,
    ElWdw_ComponentData_ComponentDataArrItemLength,
    ElWdw_ComponentData_ComponentCountPtr,
    ElWdw_ComponentData_IsModifiedPtr,
    Control_ParentOffset,
    Control_WindowProcOffset,
    Control_HandleOffset,
    Control_LeftOffset,
    Control_TopOffset,
    Control_WidthOffset,
    Control_HeightOffset,
    Queue_SizeOffset,
    Application_InstancePtr,
    Application_OnMessageOffset,
  }
}
