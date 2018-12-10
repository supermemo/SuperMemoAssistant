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
// Created On:   2018/05/24 13:11
// Modified On:  2018/12/09 16:00
// Modified By:  Alexis

#endregion




using System;
using Anotar.Serilog;
using Process.NET.Memory;
using Process.NET.Types;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Components.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.UI.Element;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Components.Controls;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Elements;
using SuperMemoAssistant.Sys;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.UI.Element
{
  [InitOnLoad]
  public class ElementWdw : WdwBase, IElementWdw
  {
    #region Constants & Statics

    public static ElementWdw Instance { get; } = new ElementWdw();

    #endregion




    #region Properties & Fields - Non-Public
    
    protected ControlGroup _controlGroup = null;

    protected int LastElementId { get; set; }

    protected IPointer ElementWdwPtr       { get; set; }
    protected IPointer ElementIdPtr        { get; set; }
    protected IPointer CurrentConceptIdPtr { get; set; }
    protected IPointer CurrentRootIdPtr    { get; set; }
    protected IPointer CurrentHookIdPtr    { get; set; }

    protected NativeFunc<bool, IntPtr>               PasteArticleFunc       { get; set; }
    protected NativeFunc<bool, IntPtr>               PasteElementFunc       { get; set; }
    protected NativeFunc<int, IntPtr, byte, byte>    AppendElementFunc      { get; set; }
    protected NativeFunc<int, IntPtr, DelphiUString> AddElementFromTextFunc { get; set; }

    protected NativeAction<IntPtr, int> SetDefaultConceptMethod { get; set; }
    protected NativeAction<IntPtr, int> GoToMethod              { get; set; }
    protected NativeAction<IntPtr>      DeleteMethod            { get; set; }
    protected NativeAction<IntPtr>      DoneMethod              { get; set; }

    protected NativeAction<IntPtr, int, DelphiUString> GetTextMethod { get; set; }
    protected NativeAction<IntPtr, int, DelphiUString> SetTextMethod { get; set; }

    protected NativeAction<IntPtr, bool, DelphiUString> EnterUpdateLockMethod { get; set; }
    protected NativeAction<IntPtr, bool>                QuitUpdateLockMethod  { get; set; }

    #endregion




    #region Constructors

    public ElementWdw()
    {
      SMA.Instance.OnSMStartedEvent += OnSMStartedEvent;
      SMA.Instance.OnSMStoppedEvent += OnSMStoppedEvent;
    }

    #endregion




    #region Methods Impl
    
    public bool FocusWindow()
    {
      try
      {
        Window.Focus();

        return true;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool SetCurrentConcept(int conceptId)
    {
      var elem = SMA.Instance.Registry.Element[conceptId];

      if (elem == null || elem.Deleted || elem is IConceptGroup == false)
        return false;

      throw new NotImplementedException(); // SetDefaultConcept is actually a TSMMain method

      //return SetDefaultConceptMethod(
      //  ElementWdwPtr.Read<IntPtr>(),
      //  conceptId,
      //  SMProcess.ThreadFactory.MainThread);
    }

    public bool GoToElement(int elementId)
    {
      bool ret = false;

      try
      {
        var elem = SMA.Instance.Registry.Element[elementId];

        if (elem == null || elem.Deleted)
          return false;

        ret = GoToMethod(ElementWdwPtr.Read<IntPtr>(),
                         elementId,
                         SMProcess.ThreadFactory.MainThread);

        return ret;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return ret;
      }
    }

    public bool PasteArticle()
    {
      try
      {
        return PasteArticleFunc(ElementWdwPtr.Read<IntPtr>(),
                                SMProcess.ThreadFactory.MainThread);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool PasteElement()
    {
      try
      {
        return PasteElementFunc(ElementWdwPtr.Read<IntPtr>(),
                                SMProcess.ThreadFactory.MainThread);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return false;
      }
    }

    public int AppendElement(ElementType elementType)
    {
      try
      {
        return AppendElementFunc(ElementWdwPtr.Read<IntPtr>(),
                                 (byte)elementType,
                                 0, // ??
                                 SMProcess.ThreadFactory.MainThread);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return -1;
      }
    }

    public bool AddElementFromText(string elementDesc)
    {
      try
      {
        return AddElementFromTextFunc(ElementWdwPtr.Read<IntPtr>(),
                                      new DelphiUString(elementDesc),
                                      SMProcess.ThreadFactory.MainThread) > 0;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool Delete()
    {
      try
      {
        return DeleteMethod(ElementWdwPtr.Read<IntPtr>(),
                            SMProcess.ThreadFactory.MainThread);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool Done()
    {
      try
      {
        return DoneMethod(ElementWdwPtr.Read<IntPtr>(),
                          SMProcess.ThreadFactory.MainThread);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return false;
      }
    }

    #endregion




    #region Methods

    public bool SetText(IControl control,
                        string   text)
    {
      try
      {
        SetTextMethod(ElementWdwPtr.Read<IntPtr>(),
                      control.Id + 1,
                      new DelphiUString(text));

        return true;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return false;
      }
    }

    public string GetText(IControl control)
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

    public bool EnterSMUpdateLock()
    {
      try
      {
        EnterUpdateLockMethod(ElementWdwPtr.Read<IntPtr>(),
                              true,
                              new DelphiUString(1));

        return true;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool QuitSMUpdateLock()
    {
      try
      {
        QuitUpdateLockMethod(ElementWdwPtr.Read<IntPtr>(),
                             true);

        return true;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool EnterSMAUpdateLock()
    {
      return ElementIdPtr.SuspendTimer();
    }

    public bool QuitSMAUpdateLock(bool updateValue = false)
    {
      return ElementIdPtr.RestartTimer(updateValue);
    }

    private void OnSMStartedEvent(object        sender,
                                  SMProcessArgs e)
    {
      ElementWdwPtr = SMProcess[SMNatives.TElWind.InstancePtr];
      ElementWdwPtr.RegisterValueChangedEventHandler<int>(OnWindowCreated);
    }

    private bool OnWindowCreated(byte[] newVal)
    {
      if (ElementWdwPtr.Read<int>() == 0)
        return false;

      var funcScanner = new NativeFuncScanner(SMProcess,
                                              Process.NET.Native.Types.CallingConventions.Register);

      ElementIdPtr        = SMProcess[SMNatives.TElWind.ElementIdPtr];
      CurrentConceptIdPtr = SMProcess[SMNatives.Globals.CurrentConceptIdPtr];
      CurrentRootIdPtr    = SMProcess[SMNatives.Globals.CurrentRootIdPtr];
      CurrentHookIdPtr    = SMProcess[SMNatives.Globals.CurrentHookIdPtr];

      PasteArticleFunc       = funcScanner.GetNativeFunc<bool, IntPtr>(SMNatives.TElWind.PasteArticleSig);
      PasteElementFunc       = funcScanner.GetNativeFunc<bool, IntPtr>(SMNatives.TElWind.PasteElementCallSig);
      AppendElementFunc      = funcScanner.GetNativeFunc<int, IntPtr, byte, byte>(SMNatives.TElWind.AppendElementCallSig);
      AddElementFromTextFunc = funcScanner.GetNativeFunc<int, IntPtr, DelphiUString>(SMNatives.TElWind.AddElementFromTextCallSig);

      GoToMethod   = funcScanner.GetNativeAction<IntPtr, int>(SMNatives.TElWind.GoToElementCallSig);
      DeleteMethod = funcScanner.GetNativeAction<IntPtr>(SMNatives.TElWind.DeleteCurrentElementCallSig);
      DoneMethod   = funcScanner.GetNativeAction<IntPtr>(SMNatives.TElWind.DoneSig);

      GetTextMethod = funcScanner.GetNativeAction<IntPtr, int, DelphiUString>(SMNatives.TElWind.GetTextCallSig);
      SetTextMethod = funcScanner.GetNativeAction<IntPtr, int, DelphiUString>(SMNatives.TElWind.SetTextCallSig);

      EnterUpdateLockMethod = funcScanner.GetNativeAction<IntPtr, bool, DelphiUString>(SMNatives.TElWind.EnterUpdateLockCallSig);
      QuitUpdateLockMethod  = funcScanner.GetNativeAction<IntPtr, bool>(SMNatives.TElWind.QuitUpdateLockCallSig);

      ElementIdPtr.RegisterValueChangedEventHandler<int>(OnElementChangedInternal);

      LastElementId = CurrentElementId;

      // TODO: ??? This somehow gets delayed and causes all sorts of troubles
      //OnElementChanged?.Invoke(new SMDisplayedElementChangedArgs(SMA.Instance,
      //                                                   CurrentElement,
      //                                                   null));

      funcScanner.Cleanup();

      return true;
    }

    private void OnSMStoppedEvent(object        sender,
                                  SMProcessArgs e)
    {
      ElementIdPtr.Dispose();

      ElementWdwPtr       = null;
      ElementIdPtr        = null;
      CurrentConceptIdPtr = null;
      CurrentRootIdPtr    = null;
      CurrentHookIdPtr    = null;

      PasteArticleFunc = null;
      PasteElementFunc = null;
      GoToMethod       = null;
      DoneMethod       = null;
    }

    protected bool OnElementChangedInternal(byte[] newVal)
    {
      int newElementId;

      try
      {
        newElementId = BitConverter.ToInt32(newVal,
                                            0);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Failed to convert bytes to int 32.");
        return false;
      }
      
      return OnElementChangedInternal(newElementId);
    }

    protected bool OnElementChangedInternal(int newElementId)
    {
      if (newElementId <= 0)
        return false;

      try
      {
        _controlGroup?.Dispose();
        _controlGroup = null;

        DateTime startTime   = DateTime.Now;
        IElement lastElement = null, currentElement = null;

        do
        {
          if (LastElementId > 0 && lastElement == null)
            lastElement = SMA.Instance.Registry.Element[LastElementId];

          if (currentElement == null)
            currentElement = ElementRegistry.Instance[newElementId];
        } while ((DateTime.Now - startTime).TotalMilliseconds < 800
          && (LastElementId > 0 && lastElement == null || currentElement == null));

        LastElementId = newElementId;

        OnElementChanged?.Invoke(
          new SMDisplayedElementChangedArgs(SMA.Instance,
                                            currentElement,
                                            lastElement)
        );
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Error while notifying OnElementChanged");
      }

      return false;
    }

    #endregion




    #region Properties Impl

    //
    // IElementWdw Impl

    /// <inheritdoc />
    public IControlGroup ControlGroup => _controlGroup ?? (_controlGroup = new ControlGroup(SMProcess));

    /// <inheritdoc />
    public int CurrentElementId => ElementIdPtr?.Read<int>() ?? 0;
    /// <inheritdoc />
    public IElement CurrentElement => SMA.Instance.Registry.Element?[CurrentElementId];

    public int CurrentConceptId => CurrentConceptIdPtr.Read<int>();
    public int CurrentRootId
    {
      get => CurrentRootIdPtr.Read<int>();
      set => CurrentRootIdPtr.Write<int>(0,
                                         value);
    }
    public int CurrentHookId
    {
      get => CurrentHookIdPtr.Read<int>();
      set => CurrentHookIdPtr.Write<int>(0,
                                         value);
    }

    /// <inheritdoc />
    public event Action<SMDisplayedElementChangedArgs> OnElementChanged;


    //
    // IWdwBase Implt

    /// <inheritdoc />
    public override string WindowClass => SMConst.UI.ElementWindowClassName;

    #endregion
  }
}
