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
// Modified On:  2018/06/21 12:07
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Windows;
using Anotar.Serilog;
using mshtml;
using Process.NET.Memory;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Components.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.UI.Element;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Components.Controls;
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

    protected IPointer ElementWdwPtr       { get; set; }
    protected IPointer ElementIdPtr        { get; set; }
    protected IPointer CurrentConceptIdPtr { get; set; }
    protected IPointer CurrentRootIdPtr    { get; set; }
    protected IPointer CurrentHookIdPtr    { get; set; }

    //protected AutomationElement ContentPane { get; set; }
    
    protected NativeFunc<bool, IntPtr>  PasteArticleFunc         { get; set; }
    protected NativeFunc<bool, IntPtr>  PasteElementFunc         { get; set; }
    protected NativeAction<IntPtr, int> SetDefaultConceptMethod { get; set; }
    protected NativeAction<IntPtr, int> GoToMethod              { get; set; }
    protected NativeAction<IntPtr>      DoneMethod              { get; set; }

    #endregion




    #region Constructors

    public ElementWdw()
    {
      SMA.Instance.OnSMStartedEvent += OnSMStartedEvent;
      SMA.Instance.OnSMStoppedEvent += OnSMStoppedEvent;
    }

    #endregion




    #region Methods Impl

    //protected override void OnWindowOpened(AutomationElement elem,
    //                                       EventId           eventId)
    //{
    //  base.OnWindowOpened(elem,
    //                      eventId);

    //  ContentPane = Window.FindFirstChild(c => c.ByClassName("TScrollBox"));

    //  //contentPane.RegisterStructureChangedEvent(
    //  //  TreeScope.Children,
    //  //  OnStructureChanged
    //  //);

    //  RefreshComponents();
    //}

    //protected override void OnWindowClosed(AutomationElement elem,
    //                                       EventId           eventId)
    //{
    //  base.OnWindowClosed(elem,
    //                      eventId);
    //}

    public bool SetCurrentConcept(int conceptId)
    {
      var elem = SMA.Instance.Registry.Element[conceptId];

      if (elem == null || elem.Deleted || elem is IConceptGroup == false)
        return false;

      throw new NotImplementedException(); // SetDefaultConcept is actually a TSMMain method

      return SetDefaultConceptMethod(
        ElementWdwPtr.Read<IntPtr>(),
        conceptId,
        SMProcess.ThreadFactory.MainThread);
    }

    public bool GoToElement(int elementId)
    {
      var elem = SMA.Instance.Registry.Element[elementId];

      if (elem == null || elem.Deleted)
        return false;

      return GoToMethod(ElementWdwPtr.Read<IntPtr>(),
                        elementId,
                        SMProcess.ThreadFactory.MainThread);
    }

    public bool PasteArticle()
    {
      return PasteArticleFunc(ElementWdwPtr.Read<IntPtr>(),
                              SMProcess.ThreadFactory.MainThread);
    }

    public bool PasteElement()
    {
      return PasteElementFunc(ElementWdwPtr.Read<IntPtr>(),
                              SMProcess.ThreadFactory.MainThread);
    }

    public bool Done()
    {
      return DoneMethod(ElementWdwPtr.Read<IntPtr>(),
                        SMProcess.ThreadFactory.MainThread);
    }

    #endregion




    #region Methods

    private void OnSMStartedEvent(object        sender,
                                  SMProcessArgs e)
    {
      ElementWdwPtr = SMProcess[SMNatives.TElWind.InstancePtr];
      ElementWdwPtr.RegisterValueChangedEventHandler<int>(OnWindowCreated);
    }

    private bool OnWindowCreated()
    {
      if (ElementWdwPtr.Read<int>() == 0)
        return false;

      var funcScanner = new NativeFuncScanner(SMProcess,
                                              Process.NET.Native.Types.CallingConventions.Register);

      ElementIdPtr        = SMProcess[SMNatives.TElWind.ElementIdPtr];
      CurrentConceptIdPtr = SMProcess[SMNatives.Globals.CurrentConceptIdPtr];
      CurrentRootIdPtr    = SMProcess[SMNatives.Globals.CurrentRootIdPtr];
      CurrentHookIdPtr    = SMProcess[SMNatives.Globals.CurrentHookIdPtr];

      PasteArticleFunc = funcScanner.GetNativeFunc<bool, IntPtr>(SMNatives.TElWind.PasteArticleSig);
      PasteElementFunc = funcScanner.GetNativeFunc<bool, IntPtr>(SMNatives.TElWind.PasteElementCallSig);
      GoToMethod       = funcScanner.GetNativeAction<IntPtr, int>(SMNatives.TElWind.GoToElementCallSig);
      DoneMethod       = funcScanner.GetNativeAction<IntPtr>(SMNatives.TElWind.DoneSig);

      ElementIdPtr.RegisterValueChangedEventHandler<int>(OnElementChangedInternal);

      OnElementChanged(new SMElementArgs(SMA.Instance,
                                         CurrentElement));

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

    protected bool OnElementChangedInternal()
    {
      _controlGroup?.Dispose();
      _controlGroup = null;

      RequiresRefresh = true;

      try
      {
        OnElementChanged?.Invoke(new SMElementArgs(SMA.Instance,
                                                   CurrentElement));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Error while notifying OnElementChanged");
      }

      return false;
    }

    #endregion



    //protected void OnStructureChanged(AutomationElement   _,
    //                                  StructureChangeType changeType,
    //                                  int[]               values)
    //{
    //  if (values.Length != 2)
    //    return;

    //  switch (changeType)
    //  {
    //    case StructureChangeType.ChildAdded:
    //      if (RequiresRefresh == false)
    //        _htmlDocuments.Clear();

    //      RequiresRefresh = true;

    //      var component = UIAuto.FromHandle(new IntPtr(values[1]));
    //      ProcessComponent(component);
    //      break;
    //  }
    //}

    //protected IEnumerable<IHTMLDocument2> RefreshComponents()
    //{
    //  if (SMProcess.Native.HasExited)
    //    return new List<IHTMLDocument2>();

    //  foreach (var comp in ContentPane.FindAllChildren())
    //    ProcessComponent(comp);

    //  RequiresRefresh = false;

    //  return _htmlDocuments;
    //}

    //protected void ProcessComponent(AutomationElement comp)
    //{
    //  switch (comp.ClassName)
    //  {
    //    case "Shell Embedding": // IE
    //      var            ieServer   = comp.FindFirstDescendant(c => c.ByClassName("Internet Explorer_Server"));
    //      var            ieHwnd     = ieServer.FrameworkAutomationElement.NativeWindowHandle.Value;
    //      IHTMLDocument2 ieDocument = IEComHelper.GetDocumentFromHwnd(ieHwnd);

    //      _htmlDocuments.Add(ieDocument);
    //      break;

    //    case "TRichEdit": // RTF
    //      break;
    //  }
    //}
    
    protected ControlGroup _controlGroup = null;



    #region Properties & Fields

    protected bool RequiresRefresh { get; set; } = false;

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
    public event Action<SMElementArgs> OnElementChanged;


    //
    // IWdwBase Implt

    /// <inheritdoc />
    public override string WindowClass => SMConst.UI.ElementWindowClassName;

    #endregion
  }
}
