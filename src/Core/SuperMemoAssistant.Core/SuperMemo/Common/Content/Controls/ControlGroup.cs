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
// Created On:   2019/05/08 19:51
// Modified On:  2020/01/12 10:24
// Modified By:  Alexis

#endregion




using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PluginManager.Interop.Sys;
using Process.NET;
using Process.NET.Memory;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.SMA;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Controls
{
  public class ControlGroup : PerpetualMarshalByRefObject, IControlGroup, IDisposable
  {
    #region Constants & Statics

    private const string DisposedException = "ControlGroup is Disposed";

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
      _componentDataAddr = smProcess[Core.Natives.ElWind.ComponentsDataPtr].Read<int>();

      CountPtr            = smProcess[Core.Natives.ElWind.Components.ComponentCountPtr];
      FocusedControlNoPtr = smProcess[Core.Natives.ElWind.FocusedComponentPtr];
      IsModifiedPtr       = smProcess[Core.Natives.ElWind.Components.IsModifiedPtr];
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
    public bool IsModified
    {
      get => IsModifiedPtr?.Read<bool>() ?? throw new InvalidOperationException(DisposedException);
      set => IsModifiedPtr?.Write<bool>(0,
                                        value);
    }

    #endregion




    #region Methods Impl

    public IControlHtml GetFirstHtmlControl()
    {
      return GetFirstControl<IControlHtml>();
    }

    public IControlImage GetFirstImageControl()
    {
      return GetFirstControl<IControlImage>();
    }

    public IControlRtf GetFirstRtfControl()
    {
      return GetFirstControl<IControlRtf>();
    }

    //public IControlSound GetFirstSoundControl()
    //{
    //  return GetFirstControl<IControlSound>();
    //}

    //public IControlSpelling GetFirstSpellingControl()
    //{
    //  return GetFirstControl<IControlSpelling>();
    //}

    public IControlText GetFirstTextControl()
    {
      return GetFirstControl<IControlText>();
    }

    //public IControlVideo GetFirstVideoControl()
    //{
    //  return GetFirstControl<IControlVideo>();
    //}

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

    private T GetFirstControl<T>()
      where T : class, IControl
    {
      return this.FirstOrDefault(c => c is T) as T;
    }

    private IControl GetControl(int idx)
    {
      if (IsDisposed)
        throw new InvalidOperationException(DisposedException);

      if (idx < 0 || idx >= Count)
        return null;

      byte smCompType = _smProcess.Memory.Read<byte>(
        new IntPtr(_componentDataAddr
                   + Core.Natives.ElWind.Components.ComponentDataArrOffset
                   + Core.Natives.ElWind.Components.ComponentDataArrItemLength * idx)
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
                                          ComponentType.Binary,
                                          this);

        case 0x0C:
          return new ControlRtf(idx,
                                this);

        case 0x0D:
          return new ControlHtml(idx,
                                 this);

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

      // TODO: Move to SMInject
      //return GetTextFunc(new IntPtr(_componentDataAddr),
      //                   control.Id + 1,
      //                   _smProcess.ThreadFactory.MainThread);

      throw new NotImplementedException();
    }

    public bool SetText(IControl control,
                        string   text)
    {
      if (IsDisposed)
        throw new InvalidOperationException(DisposedException);

      return Core.Natives.ElWind.Components.SetText(new IntPtr(_componentDataAddr),
                                                    control,
                                                    text);
    }

    public int GetTextRegMember(IControl control)
    {
      if (IsDisposed)
        throw new InvalidOperationException(DisposedException);

      return Core.Natives.ElWind.Components.GetTextRegMember(new IntPtr(_componentDataAddr),
                                                             control);
    }

    public bool SetTextRegMember(IControl control,
                                 int      member)
    {
      if (IsDisposed)
        throw new InvalidOperationException(DisposedException);

      if (Core.SM.Registry.Text[member] == null)
        return false;

      return Core.Natives.ElWind.Components.SetTextRegMember(new IntPtr(_componentDataAddr),
                                                             control,
                                                             member);
    }

    public int GetImageRegMember(IControl control)
    {
      if (IsDisposed)
        throw new InvalidOperationException(DisposedException);

      return Core.Natives.ElWind.Components.GetImageRegMember(new IntPtr(_componentDataAddr),
                                                              control);
    }

    public bool SetImageRegMember(IControl control,
                                  int      member)
    {
      if (IsDisposed)
        throw new InvalidOperationException(DisposedException);

      if (Core.SM.Registry.Image[member] == null)
        return false;

      return Core.Natives.ElWind.Components.SetImageRegMember(new IntPtr(_componentDataAddr),
                                                              control,
                                                              member);
    }

    #endregion
  }
}
