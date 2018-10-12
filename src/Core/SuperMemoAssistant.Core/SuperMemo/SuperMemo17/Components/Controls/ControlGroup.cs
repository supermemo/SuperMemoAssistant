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
// Created On:   2018/06/20 19:39
// Modified On:  2018/06/21 12:57
// Modified By:  Alexis

#endregion




using System;
using System.Collections;
using System.Collections.Generic;
using Process.NET;
using Process.NET.Extensions;
using Process.NET.Memory;
using SuperMemoAssistant.Interop.SuperMemo.Components.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Components.Models;
using SuperMemoAssistant.Sys;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Components.Controls
{
  public class ControlGroup : SMMarshalByRefObject, IControlGroup, IDisposable
  {
    #region Constants & Statics

    private const string DisposedException = "ControlGroup is Disposed";


    //private static NativeFunc<int, IntPtr, int>      GetTypeFunc             { get; set; }
    private static NativeFunc<string, IntPtr, int>   GetTextFunc             { get; set; }
    private static NativeAction<IntPtr, int, string> SetTextMethod           { get; set; }
    private static NativeFunc<int, IntPtr, int>      GetTextRegMemberFunc    { get; set; }
    private static NativeAction<IntPtr, int, int>    SetTextRegMemberMethod  { get; set; }
    private static NativeFunc<int, IntPtr, int>      GetImageRegMemberFunc   { get; set; }
    private static NativeAction<IntPtr, int, int>    SetImageRegMemberMethod { get; set; }

    #endregion




    #region Properties & Fields - Non-Public

    private readonly  int      _componentDataAddr;
    internal readonly IProcess _smProcess;

    private IPointer CountPtr { get; set; }

    private IPointer IsModifiedPtr { get; set; }

    private IPointer FocusedControlNoPtr { get; set; }

    #endregion




    #region Constructors

    public ControlGroup(IProcess smProcess)
    {
      _smProcess         = smProcess;
      _componentDataAddr = smProcess[SMNatives.TElWind.ComponentsDataPtr].Read<int>();

      CountPtr            = smProcess[SMNatives.TElWind.TComponentData.ComponentCountPtr];
      FocusedControlNoPtr = smProcess[SMNatives.TElWind.FocusedComponentPtr];
      IsModifiedPtr       = smProcess[SMNatives.TElWind.TComponentData.IsModifiedPtr];

      if (GetTextFunc == null)
        SetupNatives(smProcess);
    }


    /// <inheritdoc />
    public void Dispose()
    {
      CountPtr?.Dispose();
      IsModifiedPtr?.Dispose();
      FocusedControlNoPtr?.Dispose();

      CountPtr            = null;
      IsModifiedPtr       = null;
      FocusedControlNoPtr = null;

      IsDisposed = true;
    }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public bool IsDisposed { get; set; }
    /// <inheritdoc />
    public IControl FocusedControl => GetControl(FocusedControlIndex);
    /// <inheritdoc />
    public IControl this[int idx] => GetControl(idx);
    /// <inheritdoc />
    public int Count => CountPtr?.Read<byte>() ?? throw new InvalidOperationException(DisposedException);
    /// <inheritdoc />
    public int FocusedControlIndex => FocusedControlNoPtr?.Read<byte>() - 1 ?? throw new InvalidOperationException(DisposedException);
    /// <inheritdoc />
    public bool IsModified => IsModifiedPtr?.Read<bool>() ?? throw new InvalidOperationException(DisposedException);

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc />
    public IEnumerator<IControl> GetEnumerator()
    {
      if (IsDisposed)
        throw new InvalidOperationException(DisposedException);

      for (int i = 0; i < Count; i++)
        yield return GetControl(i);
    }

    #endregion




    #region Methods

    private IControl GetControl(int idx)
    {
      if (IsDisposed)
        throw new InvalidOperationException(DisposedException);

      if (idx < 0 || idx >= Count)
        return null;

      byte smCompType = _smProcess.Memory.Read<byte>(
        new IntPtr(_componentDataAddr
                   + SMNatives.TElWind.TComponentData.ComponentDataArrOffset
                   + SMNatives.TElWind.TComponentData.ComponentDataArrItemLength * idx)
      );

      switch (smCompType)
      {
        case 0x00:
          return new ControlText(idx,
                                 this);

        case 0x01:
          return new ComponentControlBase(idx,
                                          ComponentType.Spelling,
                                          this);

        case 0x02:
          return new ControlImage(idx,
                                  this);

        case 0x03:
          return new ComponentControlBase(idx,
                                          ComponentType.Sound,
                                          this);

        case 0x04:
          return new ComponentControlBase(idx,
                                          ComponentType.Video,
                                          this);

        case 0x05:
          return new ComponentControlBase(idx,
                                          ComponentType.ShapeEllipse,
                                          this);

        case 0x06:
          return new ComponentControlBase(idx,
                                          ComponentType.ShapeRectangle,
                                          this);

        case 0x07:
          return new ComponentControlBase(idx,
                                          ComponentType.ShapeRoundedRectangle,
                                          this);

        case 0x0A:
          return new ComponentControlBase(idx,
                                          ComponentType.Script,
                                          this);

        case 0x0B:
          return new ComponentControlBase(idx,
                                          ComponentType.External,
                                          this);

        case 0x0C:
          return new ControlRtf(idx,
                                this);

        case 0x0D:
          return new ControlWeb(idx,
                                this,
                                _smProcess.Memory.Read<int>(SMNatives.TElWind.ObjectsPtr,
                                                            4 * idx));

        case 0x0E:
          return new ComponentControlBase(idx,
                                          ComponentType.OLE,
                                          this);

        default:
          return new ComponentControlBase(idx,
                                          ComponentType.Unknown,
                                          this);
      }
    }

    public string GetText(IControl control)
    {
      if (IsDisposed)
        throw new InvalidOperationException(DisposedException);

      return GetTextFunc(new IntPtr(_componentDataAddr),
                         control.Id + 1,
                         _smProcess.ThreadFactory.MainThread);
    }

    public bool SetText(IControl control,
                        string   text)
    {
      if (IsDisposed)
        throw new InvalidOperationException(DisposedException);

      return SetTextMethod(new IntPtr(_componentDataAddr),
                           control.Id + 1,
                           text,
                           _smProcess.ThreadFactory.MainThread);
    }

    public int GetTextRegMember(IControl control)
    {
      if (IsDisposed)
        throw new InvalidOperationException(DisposedException);

      return GetTextRegMemberFunc(new IntPtr(_componentDataAddr),
                                  control.Id + 1,
                                  _smProcess.ThreadFactory.MainThread);
    }

    public bool SetTextRegMember(IControl control,
                                 int      member)
    {
      if (IsDisposed)
        throw new InvalidOperationException(DisposedException);

      if (SMA.Instance.Registry.Text[member] == null)
        return false;

      return SetTextRegMemberMethod(new IntPtr(_componentDataAddr),
                                    control.Id + 1,
                                    member,
                                    _smProcess.ThreadFactory.MainThread);
    }

    public int GetImageRegMember(IControl control)
    {
      if (IsDisposed)
        throw new InvalidOperationException(DisposedException);

      return GetImageRegMemberFunc(new IntPtr(_componentDataAddr),
                                   control.Id + 1,
                                   _smProcess.ThreadFactory.MainThread);
    }

    public bool SetImageRegMember(IControl control,
                                  int      member)
    {
      if (IsDisposed)
        throw new InvalidOperationException(DisposedException);

      if (SMA.Instance.Registry.Image[member] == null)
        return false;

      return SetImageRegMemberMethod(new IntPtr(_componentDataAddr),
                                     control.Id + 1,
                                     member,
                                     _smProcess.ThreadFactory.MainThread);
    }

    private void SetupNatives(IProcess smProcess)
    {
      var funcScanner = new NativeFuncScanner(smProcess,
                                              Process.NET.Native.Types.CallingConventions.Register);

      //GetTypeFunc             = funcScanner.GetNativeFunc<IntPtr, int, int>(SMNatives.TElWind.TComponentData.GetTypeCallSig);
      GetTextFunc             = funcScanner.GetNativeFunc<string, IntPtr, int>(SMNatives.TElWind.TComponentData.GetTextCallSig);
      SetTextMethod           = funcScanner.GetNativeAction<IntPtr, int, string>(SMNatives.TElWind.TComponentData.SetTextCallSig);
      GetTextRegMemberFunc    = funcScanner.GetNativeFunc<int, IntPtr, int>(SMNatives.TElWind.TComponentData.GetTextRegMemberCallSig);
      SetTextRegMemberMethod  = funcScanner.GetNativeAction<IntPtr, int, int>(SMNatives.TElWind.TComponentData.SetTextRegMemberCallSig);
      GetImageRegMemberFunc   = funcScanner.GetNativeFunc<int, IntPtr, int>(SMNatives.TElWind.TComponentData.GetImageRegMemberCallSig);
      SetImageRegMemberMethod = funcScanner.GetNativeAction<IntPtr, int, int>(SMNatives.TElWind.TComponentData.SetImageRegMemberCallSig);
    }

    #endregion
  }
}
