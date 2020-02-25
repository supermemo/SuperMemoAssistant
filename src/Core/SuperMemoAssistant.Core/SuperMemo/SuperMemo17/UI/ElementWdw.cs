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
// Modified On:  2020/02/17 21:40
// Modified By:  Alexis

#endregion




using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Anotar.Serilog;
using Process.NET.Memory;
using Process.NET.Types;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Learning;
using SuperMemoAssistant.Interop.SuperMemo.UI.Element;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.SuperMemo.Common.Content.Controls;
using SuperMemoAssistant.SuperMemo.Common.UI;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.UI
{
  public class ElementWdw : WdwBase, IElementWdw
  {
    #region Properties & Fields - Non-Public

    protected ControlGroup _controlGroup = null;

    protected int LastElementId { get; set; }

    protected IPointer SMMainWdwPtr             { get; set; }
    protected IPointer ElementWdwPtr            { get; set; }
    protected IPointer ElementIdPtr             { get; set; }
    protected IPointer LimitChildrenCountPtr    { get; set; }
    protected IPointer CurrentConceptIdPtr      { get; set; }
    protected IPointer CurrentConceptGroupIdPtr { get; set; }
    protected IPointer CurrentRootIdPtr         { get; set; }
    protected IPointer CurrentHookIdPtr         { get; set; }
    protected IPointer LearningModePtr          { get; set; }

    #endregion




    #region Constructors

    public ElementWdw()
    {
      IsAvailable = false;

      Core.SMA.OnSMStartedEvent += OnSMStartedEvent;
      Core.SMA.OnSMStoppedEvent += OnSMStoppedEvent;
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
      catch (ApplicationException ex)
      {
        LogTo.Warning(ex, "Windows API call failed (ActivateWindow).");
        return false;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Windows API call threw an exception.");
        return false;
      }
    }

    public bool SetCurrentConcept(int conceptId)
    {
      var concept = Core.SM.Registry.Concept[conceptId];

      if (concept == null || concept.Empty)
        return false;

      // SetDefaultConcept is actually a TSMMain method
      var success = Core.Natives.SMMain.SelectDefaultConcept(SMMainWdwPtr.Read<IntPtr>(), conceptId);

      return success && CurrentConceptId == conceptId;
    }

    public bool GoToElement(int elementId)
    {
      try
      {
        var elem = Core.SM.Registry.Element[elementId];

        if (elem == null || elem.Deleted)
          return false;

        return Core.Natives.ElWind.GoToElement(ElementWdwPtr.Read<IntPtr>(),
                                               elementId);
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read ElementWdwPtr");
        return false;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool PasteArticle()
    {
      try
      {
        return Core.Natives.ElWind.PasteArticle(ElementWdwPtr.Read<IntPtr>());
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read ElementWdwPtr");
        return false;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool PasteElement()
    {
      try
      {
        return Core.Natives.ElWind.PasteElement(ElementWdwPtr.Read<IntPtr>());
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read ElementWdwPtr");
        return false;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
        return false;
      }
    }

    public int AppendElement(ElementType elementType)
    {
      try
      {
        return Core.Natives.ElWind.AppendElement(ElementWdwPtr.Read<IntPtr>(),
                                                 elementType);
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read ElementWdwPtr");
        return -1;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
        return -1;
      }
    }

    public bool AddElementFromText(string elementDesc)
    {
      try
      {
        return Core.Natives.ElWind.SetElementFromDescription(
          ElementWdwPtr.Read<IntPtr>(),
          elementDesc);
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read ElementWdwPtr");
        return false;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
        return false;
      }
    }

    public int GenerateExtract(ElementType elementType, bool memorize = true, bool askUserToScheduleInterval = false)
    {
      try
      {
        return Core.Natives.ElWind.GenerateExtract(
          ElementWdwPtr.Read<IntPtr>(),
          elementType,
          memorize,
          askUserToScheduleInterval);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Native method call threw an exception.");
        return -1;
      }
    }

    public int GenerateCloze(bool memorize = true, bool askUserToScheduleInterval = false)
    {
      try
      {
        return Core.Natives.ElWind.GenerateCloze(
          ElementWdwPtr.Read<IntPtr>(),
          memorize,
          askUserToScheduleInterval);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Native method call threw an exception.");
        return -1;
      }
    }

    public bool Delete()
    {
      try
      {
        return Core.Natives.ElWind.DeleteCurrentElement(ElementWdwPtr.Read<IntPtr>());
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read ElementWdwPtr");
        return false;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool Done()
    {
      try
      {
        return Core.Natives.ElWind.Done(ElementWdwPtr.Read<IntPtr>());
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read ElementWdwPtr");
        return false;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool NextElementInLearningQueue()
    {
      try
      {
        return Core.Natives.ElWind.ShowNextElementInLearningQueue(ElementWdwPtr.Read<IntPtr>());
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read ElementWdwPtr");
        return false;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool SetElementState(int state)
    {
      try
      {
        return Core.Natives.ElWind.SetElementState(ElementWdwPtr.Read<IntPtr>(),
                                                   state);
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read ElementWdwPtr");
        return false;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool PostponeRepetition(int interval)
    {
      try
      {
        return Core.Natives.ElWind.PostponeRepetition(ElementWdwPtr.Read<IntPtr>(),
                                                      interval);
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read ElementWdwPtr");
        return false;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool ForceRepetition(int  interval,
                                bool adjustPriority)
    {
      try
      {
        return Core.Natives.ElWind.ForceRepetitionExt(ElementWdwPtr.Read<IntPtr>(),
                                                      interval,
                                                      adjustPriority);
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read ElementWdwPtr");
        return false;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool ForceRepetitionAndResume(int  interval,
                                         bool adjustPriority)
    {
      try
      {
        Core.Natives.ElWind.ForceRepetitionAndResume(ElementWdwPtr.Read<IntPtr>(),
                                                     interval,
                                                     adjustPriority);

        return true;
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read ElementWdwPtr");
        return false;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
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
        return Core.Natives.ElWind.AppendAndAddElementFromText(ElementWdwPtr.Read<IntPtr>(),
                                                               elementType,
                                                               elementDesc);
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read ElementWdwPtr");
        return -1;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
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

        return Core.Natives.ElWind.SetText(ElementWdwPtr.Read<IntPtr>(),
                                           control,
                                           text);
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read ElementWdwPtr");
        return false;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
        return false;
      }
    }

    public string GetText(IControl control)
    {
      try
      {
        return Core.Natives.ElWind.GetText(ElementWdwPtr.Read<IntPtr>(),
                                           control);
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read ElementWdwPtr");
        return null;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
        return null;
      }
    }

    public bool EnterSMUpdateLock()
    {
      try
      {
        Core.Natives.ElWind.EnterUpdateLock.Invoke(ElementWdwPtr.Read<IntPtr>(),
                                                   true,
                                                   new DelphiUTF16String(1));

        return true;
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read ElementWdwPtr");
        return false;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
        return false;
      }
    }

    public bool QuitSMUpdateLock()
    {
      try
      {
        Core.Natives.ElWind.QuitUpdateLock.Invoke(ElementWdwPtr.Read<IntPtr>(),
                                                  true);

        return true;
      }
      catch (Win32Exception ex)
      {
        LogTo.Warning(ex, "Failed to read ElementWdwPtr");
        return false;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "SM internal method call threw an exception.");
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

    private async Task OnSMStartedEvent(object        sender,
                                        SMProcessArgs e)
    {
      LogTo.Debug($"Initializing {GetType().Name}");

      await Task.Run(() =>
      {
        SMMainWdwPtr  = SMProcess[Core.Natives.SMMain.InstancePtr];
        ElementWdwPtr = SMProcess[Core.Natives.ElWind.InstancePtr];
        ElementWdwPtr.RegisterValueChangedEventHandler<int>(OnWindowCreated);
      });
    }

    private void OnSMStoppedEvent(object        sender,
                                  SMProcessArgs e)
    {
      LogTo.Debug($"Cleaning up {GetType().Name}");

      ElementIdPtr?.Dispose();

      SMMainWdwPtr             = null;
      ElementWdwPtr            = null;
      ElementIdPtr             = null;
      CurrentConceptIdPtr      = null;
      CurrentConceptGroupIdPtr = null;
      CurrentRootIdPtr         = null;
      CurrentHookIdPtr         = null;
      LearningModePtr          = null;

      LogTo.Debug($"Cleaning up {GetType().Name}... Done");
    }

    private bool OnWindowCreated(byte[] newVal)
    {
      if (ElementWdwPtr.Read<int>() == 0)
        return false;

      ElementIdPtr             = SMProcess[Core.Natives.ElWind.ElementIdPtr];
      LimitChildrenCountPtr    = SMProcess[Core.Natives.Globals.LimitChildrenCountPtr];
      CurrentConceptIdPtr      = SMProcess[Core.Natives.Globals.CurrentConceptIdPtr];
      CurrentConceptGroupIdPtr = SMProcess[Core.Natives.Globals.CurrentConceptGroupIdPtr];
      CurrentRootIdPtr         = SMProcess[Core.Natives.Globals.CurrentRootIdPtr];
      CurrentHookIdPtr         = SMProcess[Core.Natives.Globals.CurrentHookIdPtr];
      LearningModePtr          = SMProcess[Core.Natives.ElWind.LearningModePtr];

      ElementIdPtr.RegisterValueChangedEventHandler<int>(OnElementChangedInternal);

      LastElementId = CurrentElementId;

      // TODO: ??? This somehow gets delayed and causes all sorts of troubles
      //OnElementChanged?.Invoke(new SMDisplayedElementChangedArgs(SMA.Instance,
      //                                                   CurrentElement,
      //                                                   null));

      IsAvailable = true;

      OnAvailable?.Invoke();

      return true;
    }

    protected bool OnElementChangedInternal(byte[] newVal)
    {
      int newElementId;

      try
      {
        newElementId = BitConverter.ToInt32(newVal, 0);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Failed to convert bytes to int 32.");
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
            lastElement = Core.SM.Registry.Element[LastElementId];

          if (currentElement == null)
            currentElement = Core.SM.Registry.Element[newElementId];
        } while ((DateTime.Now - startTime).TotalMilliseconds < 800
          && (LastElementId > 0 && lastElement == null || currentElement == null));

        LastElementId = newElementId;

        OnElementChanged?.InvokeRemote(
          nameof(OnElementChanged),
          new SMDisplayedElementChangedArgs(Core.SM,
                                            currentElement,
                                            lastElement),
          h => OnElementChanged -= h
        );
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Error while notifying OnElementChanged");
      }

      return false;
    }

    #endregion




    #region Events

    public override event Action OnAvailable;

    #endregion




    #region Properties Impl

    //
    // IElementWdw Impl

    /// <inheritdoc />
    public IControlGroup ControlGroup => _controlGroup ?? (_controlGroup = new ControlGroup(SMProcess));

    /// <inheritdoc />
    public int CurrentElementId => ElementIdPtr?.Read<int>() ?? 0;
    /// <inheritdoc />
    public IElement CurrentElement => Core.SM.Registry.Element?[CurrentElementId];

    public short LimitChildrenCount    => LimitChildrenCountPtr.Read<short>();
    public int   CurrentConceptGroupId => CurrentConceptGroupIdPtr.Read<int>();
    public int   CurrentConceptId      => CurrentConceptIdPtr.Read<int>();
    public int CurrentRootId
    {
      get => CurrentRootIdPtr.Read<int>();
      set => CurrentRootIdPtr.Write<int>(0, value);
    }
    public int CurrentHookId
    {
      get => CurrentHookIdPtr.Read<int>();
      set => CurrentHookIdPtr.Write<int>(0, value);
    }
    public LearningMode CurrentLearningMode => (LearningMode)LearningModePtr.Read<int>();

    /// <inheritdoc />
    public event Action<SMDisplayedElementChangedArgs> OnElementChanged;


    //
    // IWdwBase Impl

    /// <inheritdoc />
    protected override IntPtr WindowHandle =>
      SMProcess.Memory.Read<IntPtr>(new IntPtr(ElementWdwPtr.Read<int>() + Core.Natives.Control.HandleOffset));
    /// <inheritdoc />
    public override string WindowClass => SMConst.UI.ElementWindowClassName;

    #endregion
  }
}
