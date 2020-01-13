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
// Modified On:  2020/01/12 15:02
// Modified By:  Alexis

#endregion




using System;
using Anotar.Serilog;
using Process.NET.Memory;
using Process.NET.Types;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.SMA;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.SuperMemo.Natives
{
  public partial class SMNatives
  {
    public partial class TElWind
    {
      public class TComponentData
      {
        #region Constructors

        public TComponentData(ObjPtr componentsDataPtr, NativeData nativeData)
        {
          ComponentDataArrOffset     = nativeData.Pointers[NativePointers.ElWdw_ComponentData_ComponentDataArrOffset];
          ComponentDataArrItemLength = nativeData.Pointers[NativePointers.ElWdw_ComponentData_ComponentDataArrItemLength];

          ComponentCountPtr = new ObjPtr(componentsDataPtr, nativeData.Pointers[NativePointers.ElWdw_ComponentData_ComponentCountPtr]);
          IsModifiedPtr     = new ObjPtr(componentsDataPtr, nativeData.Pointers[NativePointers.ElWdw_ComponentData_IsModifiedPtr]);
        }

        #endregion




        #region Properties & Fields - Public

        public int ComponentDataArrOffset     { get; }
        public int ComponentDataArrItemLength { get; }

        public ObjPtr ComponentCountPtr { get; }
        public ObjPtr IsModifiedPtr     { get; }

        #endregion




        #region Methods

        public bool SetText(IntPtr   componentDataPtr,
                            IControl control,
                            string   text)
        {
          try
          {
            return NativeMethod.TCompData_SetText.ExecuteOnMainThread(
              componentDataPtr,
              control.Id + 1,
              new DelphiUTF16String(text), Core.Hook) == 1;
          }
          catch (Exception ex)
          {
            LogTo.Error(ex, "Native method call threw an exception.");
            return false;
          }
        }

        // TComponentData.GetTextItem
        public int GetTextRegMember(IntPtr   componentDataPtr,
                                    IControl control)
        {
          try
          {
            return NativeMethod.TCompData_GetTextRegMember.ExecuteOnMainThread(
              componentDataPtr,
              control.Id + 1);
          }
          catch (Exception ex)
          {
            LogTo.Error(ex, "Native method call threw an exception.");
            return -1;
          }
        }

        // TComponentData.SetTextItem
        public bool SetTextRegMember(IntPtr   componentDataPtr,
                                     IControl control,
                                     int      member)
        {
          try
          {
            return NativeMethod.TCompData_SetTextRegMember.ExecuteOnMainThread(
              componentDataPtr,
              control.Id + 1,
              member) == 1;
          }
          catch (Exception ex)
          {
            LogTo.Error(ex, "Native method call threw an exception.");
            return false;
          }
        }

        // TComponentData.GetImageItem
        public int GetImageRegMember(IntPtr   componentDataPtr,
                                     IControl control)
        {
          try
          {
            return NativeMethod.TCompData_GetImageRegMember.ExecuteOnMainThread(
              componentDataPtr,
              control.Id + 1);
          }
          catch (Exception ex)
          {
            LogTo.Error(ex, "Native method call threw an exception.");
            return -1;
          }
        }

        // TComponentData.SetImageItem
        public bool SetImageRegMember(IntPtr   componentDataPtr,
                                      IControl control,
                                      int      member)
        {
          try
          {
            return NativeMethod.TCompData_SetImageRegMember.ExecuteOnMainThread(
              componentDataPtr,
              control.Id + 1,
              member) == 1;
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
}
