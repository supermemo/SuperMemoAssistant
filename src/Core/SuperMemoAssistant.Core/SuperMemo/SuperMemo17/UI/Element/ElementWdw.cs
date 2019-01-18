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
// Modified On:  2018/12/27 21:29
// Modified By:  Alexis

#endregion




using System;
using Anotar.Serilog;
using Process.NET.Memory;
using Process.NET.Types;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
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

    #endregion




    #region Constructors

    public ElementWdw()
    {
      SMA.Instance.OnSMStartedEvent += OnSMStartedEvent;
      SMA.Instance.OnSMStoppedEvent += OnSMStoppedEvent;
    }

    #endregion




    #region Methods Impl

    public bool ActivateWindow()
    {
      try
      {
        Window.Activate();

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

        /*ret = GoToMethod(ElementWdwPtr.Read<IntPtr>(),
                         elementId,
                         SMProcess.ThreadFactory.MainThread);*/

        ret = SMA.Instance.SMMgmt.ExecuteOnMainThread(NativeMethod.ElWdwGoToElement,
                                                      ElementWdwPtr.Read<IntPtr>(),
                                                      elementId) == 1;

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
        //return PasteArticleFunc(ElementWdwPtr.Read<IntPtr>(),
        //                        SMProcess.ThreadFactory.MainThread);

        return SMA.Instance.SMMgmt.ExecuteOnMainThread(NativeMethod.ElWdwPasteArticle,
                                                       ElementWdwPtr.Read<IntPtr>()) == 1;
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
        //return PasteElementFunc(ElementWdwPtr.Read<IntPtr>(),
        //                        SMProcess.ThreadFactory.MainThread);

        return SMA.Instance.SMMgmt.ExecuteOnMainThread(NativeMethod.ElWdwPasteElement,
                                                       ElementWdwPtr.Read<IntPtr>()) == 1;
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
        //return AppendElementFunc(ElementWdwPtr.Read<IntPtr>(),
        //                         (byte)elementType,
        //                         0, // ??
        //                         SMProcess.ThreadFactory.MainThread);

        return SMA.Instance.SMMgmt.ExecuteOnMainThread(NativeMethod.ElWdwAppendElement,
                                                       ElementWdwPtr.Read<IntPtr>(),
                                                       (byte)elementType,
                                                       0 /* ?? */);
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
        //return AddElementFromTextFunc(ElementWdwPtr.Read<IntPtr>(),
        //                              new DelphiUString(elementDesc),
        //                              SMProcess.ThreadFactory.MainThread) > 0;

        return SMA.Instance.SMMgmt.ExecuteOnMainThread(NativeMethod.ElWdwAddElementFromText,
                                                       ElementWdwPtr.Read<IntPtr>(),
                                                       new DelphiUString(elementDesc)) > 0;
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
        //return DeleteMethod(ElementWdwPtr.Read<IntPtr>(),
        //                    SMProcess.ThreadFactory.MainThread);

        return SMA.Instance.SMMgmt.ExecuteOnMainThread(NativeMethod.ElWdwDeleteCurrentElement,
                                                       ElementWdwPtr.Read<IntPtr>()) == 1;
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
        //return DoneMethod(ElementWdwPtr.Read<IntPtr>(),
        //                  SMProcess.ThreadFactory.MainThread);

        return SMA.Instance.SMMgmt.ExecuteOnMainThread(NativeMethod.ElWdwDone,
                                                       ElementWdwPtr.Read<IntPtr>()) == 1;
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

    public int AppendAndAddElementFromText(ElementType elementType,
                                           string      elementDesc)
    {
      try
      {
        return SMA.Instance.SMMgmt.ExecuteOnMainThread(NativeMethod.AppendAndAddElementFromText,
                                                       ElementWdwPtr.Read<IntPtr>(),
                                                       (byte)elementType,
                                                       new DelphiUString(elementDesc));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return -1;
      }
    }

    public bool SetText(IControl control,
                        string   text)
    {
      try
      {
        //SetTextMethod(ElementWdwPtr.Read<IntPtr>(),
        //              control.Id + 1,
        //              new DelphiUString(text));

        //return true;

        return SMA.Instance.SMMgmt.ExecuteOnMainThread(NativeMethod.ElWdwSetText,
                                                       ElementWdwPtr.Read<IntPtr>(),
                                                       control.Id + 1,
                                                       new DelphiUString(text)) == 1;
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

    public bool NextElementInLearningQueue()
    {
      try
      {
        SMA.Instance.SMMgmt.ExecuteOnMainThread(NativeMethod.ElWdwNextElementInLearningQueue,
                                                ElementWdwPtr.Read<IntPtr>());

        return true;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool SetElementState(int state)
    {
      try
      {
        SMA.Instance.SMMgmt.ExecuteOnMainThread(NativeMethod.ElWdwSetElementState,
                                                ElementWdwPtr.Read<IntPtr>(),
                                                state);

        return true;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool PostponeRepetition(int interval)
    {
      try
      {
        SMA.Instance.SMMgmt.ExecuteOnMainThread(NativeMethod.PostponeRepetition,
                                                ElementWdwPtr.Read<IntPtr>(),
                                                interval);

        return true;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool ForceRepetition(int  interval,
                                bool adjustPriority)
    {
      try
      {
        SMA.Instance.SMMgmt.ExecuteOnMainThread(NativeMethod.ElWdwForceRepetitionExt,
                                                ElementWdwPtr.Read<IntPtr>(),
                                                interval,
                                                adjustPriority);

        return true;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool ForceRepetitionAndResume(int  interval,
                                         bool adjustPriority)
    {
      try
      {
        SMA.Instance.SMMgmt.ExecuteOnMainThread(NativeMethod.ForceRepetitionAndResume,
                                                ElementWdwPtr.Read<IntPtr>(),
                                                interval,
                                                adjustPriority);

        return true;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool EnterSMUpdateLock()
    {
      try
      {
        SM17Natives.Instance.ElWind.EnterUpdateLock.Invoke(
          ElementWdwPtr.Read<IntPtr>(),
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
        SM17Natives.Instance.ElWind.QuitUpdateLock.Invoke(
          ElementWdwPtr.Read<IntPtr>(),
          true
          );

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
      ElementWdwPtr = SMProcess[SM17Natives.TElWind.InstancePtr];
      ElementWdwPtr.RegisterValueChangedEventHandler<int>(OnWindowCreated);
    }

    private bool OnWindowCreated(byte[] newVal)
    {
      if (ElementWdwPtr.Read<int>() == 0)
        return false;
      
      ElementIdPtr        = SMProcess[SM17Natives.TElWind.ElementIdPtr];
      CurrentConceptIdPtr = SMProcess[SM17Natives.Globals.CurrentConceptIdPtr];
      CurrentRootIdPtr    = SMProcess[SM17Natives.Globals.CurrentRootIdPtr];
      CurrentHookIdPtr    = SMProcess[SM17Natives.Globals.CurrentHookIdPtr];

      ElementIdPtr.RegisterValueChangedEventHandler<int>(OnElementChangedInternal);

      LastElementId = CurrentElementId;

      // TODO: ??? This somehow gets delayed and causes all sorts of troubles
      //OnElementChanged?.Invoke(new SMDisplayedElementChangedArgs(SMA.Instance,
      //                                                   CurrentElement,
      //                                                   null));

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
    protected override IntPtr WindowHandle =>
      SMProcess.Memory.Read<IntPtr>(new IntPtr(ElementWdwPtr.Read<int>() + SM17Natives.TControl.HandleOffset));
    /// <inheritdoc />
    public override string WindowClass => SMConst.UI.ElementWindowClassName;

    #endregion
  }
}
