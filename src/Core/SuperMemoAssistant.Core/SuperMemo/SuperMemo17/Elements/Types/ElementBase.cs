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
// Created On:   2018/06/01 14:12
// Modified On:  2019/01/01 18:11
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Components;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Elements.Types
{
  public abstract class ElementBase : MarshalByRefObject, IElement, IDisposable
  {
    #region Constants & Statics

    //
    // Internal helpers

    protected static IReadOnlyDictionary<string, ElementFieldFlags> FieldFlagMapping { get; } = new Dictionary<string, ElementFieldFlags>()
    {
      { "TextId", ElementFieldFlags.Name },
      { "Deleted", ElementFieldFlags.Deleted },
      { "TemplateId", ElementFieldFlags.Template },
      { "ConceptId", ElementFieldFlags.Concept },
      { "ComponentPos", ElementFieldFlags.Components },
      { "AFactor", ElementFieldFlags.AFactor },
      { "ParentId", ElementFieldFlags.Parent },
      { "FirstChildId", ElementFieldFlags.FirstChild },
      { "LastChildId", ElementFieldFlags.LastChild },
      { "NextSiblingId", ElementFieldFlags.NextSibling },
      { "PrevSiblingId", ElementFieldFlags.PrevSibling },
      { "DescendantCount", ElementFieldFlags.DescendantCount },
      { "ChildrenCount", ElementFieldFlags.ChildrenCount },
    };

    #endregion




    #region Properties & Fields - Non-Public

    protected abstract string TypeName { get; }

    #endregion




    #region Constructors

    protected ElementBase(int             id,
                          InfContentsElem cttElem,
                          InfElementsElem elElem)
    {
      Id = id;

#if DEBUG && !DEBUG_IN_PROD
      System.Diagnostics.Debug.WriteLine("[{0} {1}] Creating",
                                         GetType().Name,
                                         Id);
#endif

      TitleTextId = SetValue(elElem.titleTextId,
                             nameof(TitleTextId));
      Deleted = SetValue(cttElem.deleted != 0,
                         nameof(Deleted));

      TemplateId = SetValue(elElem.templateId,
                            nameof(TemplateId));
      ConceptId = SetValue(elElem.conceptId,
                           nameof(ConceptId));

      ComponentPos = SetValue(elElem.componPos,
                              nameof(ComponentPos));
      OnComponentPosChanged(-1,
                            ComponentPos);
      //AFactor = SetDbg(elElem.AFactor, nameof(AFactor));

      ParentId = SetValue(cttElem.parentId,
                          nameof(ParentId));
      FirstChildId = SetValue(cttElem.firstChildId,
                              nameof(FirstChildId));
      LastChildId = SetValue(cttElem.lastChildId,
                             nameof(LastChildId));
      NextSiblingId = SetValue(cttElem.nextSiblingId,
                               nameof(NextSiblingId));
      PrevSiblingId = SetValue(cttElem.prevSiblingId,
                               nameof(PrevSiblingId));

      DescendantCount = SetValue(cttElem.descendantCount,
                                 nameof(DescendantCount));
      ChildrenCount = SetValue(cttElem.childrenCount,
                               nameof(ChildrenCount));
    }

    public void Dispose()
    {
      // TODO: Clear events handlers
      // TODO: Call this method appropriately

      OnComponentPosChanged(ComponentPos,
                            -1);
    }

    #endregion




    #region Properties & Fields - Public

    public int TitleTextId { get; protected set; }

    public int TemplateId { get; protected set; }
    public int ConceptId  { get; protected set; }

    public int    ComponentPos { get; protected set; }
    public byte[] AFactor      { get; protected set; }

    public int ParentId      { get; protected set; }
    public int FirstChildId  { get; protected set; }
    public int LastChildId   { get; protected set; }
    public int NextSiblingId { get; protected set; }
    public int PrevSiblingId { get; protected set; }

    #endregion




    #region Properties Impl - Public

    public int  Id      { get; protected set; }
    public bool Deleted { get; protected set; }

    public int DescendantCount { get; protected set; }
    public int ChildrenCount   { get; protected set; }

    public string Title => SMA.Instance.Registry.Text?[TitleTextId]?.Name;

    public IComponentGroup ComponentGroup => SMA.Instance.Registry.Component?[ComponentPos];
    public IElement        Template       => SMA.Instance.Registry.Element?[TemplateId];
    public IConcept        Concept        => SMA.Instance.Registry.Concept?[ConceptId];

    public IElement Parent      => SMA.Instance.Registry.Element?[ParentId];
    public IElement FirstChild  => SMA.Instance.Registry.Element?[FirstChildId];
    public IElement LastChild   => SMA.Instance.Registry.Element?[LastChildId];
    public IElement NextSibling => SMA.Instance.Registry.Element?[NextSiblingId];
    public IElement PrevSibling => SMA.Instance.Registry.Element?[PrevSiblingId];

    public IEnumerable<IElement> Children => EnumerateChildren();

    #endregion




    #region Methods Impl

    public override string ToString()
    {
      return $"({Id}|{TypeName[0]}) {Title}";
    }

    public Task<bool> Delete()
    {
      throw new NotImplementedException();
    }

    public Task<bool> Display()
    {
      throw new NotImplementedException();
    }

    public Task<bool> Done()
    {
      throw new NotImplementedException();
    }

    public Task<bool> MoveTo(IConceptGroup newParent)
    {
      throw new NotImplementedException();
    }

    #endregion




    #region Methods

    public ElementFieldFlags Update(InfContentsElem? cttElem,
                                    InfElementsElem? elElem)
    {
#if DEBUG && !DEBUG_IN_PROD
      System.Diagnostics.Debug.WriteLine("[{0} {1}] Updating",
                                         GetType().Name,
                                         Id);
#endif

      // TODO: Set/Clear events handlers on component change
      ElementFieldFlags flags = ElementFieldFlags.None;

      if (elElem != null)
      {
        TitleTextId = SetValue(TitleTextId,
                               elElem.Value.titleTextId,
                               nameof(TitleTextId),
                               ref flags);

        TemplateId = SetValue(TemplateId,
                              elElem.Value.templateId,
                              nameof(TemplateId),
                              ref flags);
        ConceptId = SetValue(ConceptId,
                             elElem.Value.conceptId,
                             nameof(ConceptId),
                             ref flags);

        ComponentPos = SetValue(ComponentPos,
                                elElem.Value.componPos,
                                nameof(ComponentPos),
                                ref flags,
                                OnComponentPosChanged);
        //AFactor = SetDbg(elElem.Value.AFactor, nameof(AFactor));
      }

      if (cttElem != null)
      {
        Deleted = SetValue(Deleted,
                           cttElem.Value.deleted != 0,
                           nameof(Deleted),
                           ref flags);

        ParentId = SetValue(ParentId,
                            cttElem.Value.parentId,
                            nameof(ParentId),
                            ref flags);
        FirstChildId = SetValue(FirstChildId,
                                cttElem.Value.firstChildId,
                                nameof(FirstChildId),
                                ref flags);
        LastChildId = SetValue(LastChildId,
                               cttElem.Value.lastChildId,
                               nameof(LastChildId),
                               ref flags);
        NextSiblingId = SetValue(NextSiblingId,
                                 cttElem.Value.nextSiblingId,
                                 nameof(NextSiblingId),
                                 ref flags);
        PrevSiblingId = SetValue(PrevSiblingId,
                                 cttElem.Value.prevSiblingId,
                                 nameof(PrevSiblingId),
                                 ref flags);

        DescendantCount = SetValue(DescendantCount,
                                   cttElem.Value.descendantCount,
                                   nameof(DescendantCount),
                                   ref flags);
        ChildrenCount = SetValue(ChildrenCount,
                                 cttElem.Value.childrenCount,
                                 nameof(ChildrenCount),
                                 ref flags);
      }

      try
      {
        OnChanged?.Invoke(new SMElementChangedArgs(SMA.Instance,
                                                   (IElement)this,
                                                   flags));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Error while signaling Element Changed event");
      }

      return flags;
    }

    protected void OnComponentPosChanged(int oldCompPos,
                                         int newCompPos)
    {
      if (SMA.Instance.Registry.Component == null)
        return;

      if (oldCompPos >= 0)
      {
        var comp = SMA.Instance.Registry.Component[oldCompPos];

        if (comp != null)
          comp.OnChanged -= OnComponentChanged;
      }


      if (newCompPos >= 0)
      {
        var comp = SMA.Instance.Registry.Component[newCompPos];

        if (comp != null)
          comp.OnChanged += OnComponentChanged;
      }
    }

    protected void OnComponentChanged(SMComponentGroupArgs args)
    {
      try
      {
        OnChanged?.Invoke(new SMElementChangedArgs(SMA.Instance,
                                                   (IElement)this,
                                                   ElementFieldFlags.Components));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Error while signaling Component Changed event");
      }
    }

    public IEnumerable<IElement> EnumerateChildren()
    {
      List<IElement> ret = new List<IElement>();

      try
      {
        if (ChildrenCount <= 0)
          return ret;

        IElement itEl = FirstChild;

        do
        {
          if (itEl.Deleted == false)
            ret.Add(itEl);

          itEl = itEl.NextSibling;
        } while (itEl != null);
      }
      catch (Exception)
      {
        return new List<IElement>();
      }

      return ret;

      /*
      if (ChildrenCount <= 0)
        yield return null;

      IElement itEl = FirstChild;

      do
      {
        yield return itEl;
      } while (itEl.NextSibling != null);
      */
    }

    protected T SetValue<T>(T                     oldValue,
                            T                     value,
                            string                name,
                            ref ElementFieldFlags flag,
                            Action<T, T>          onChangedAction = null)
    {
      bool changed = Equals(oldValue,
                            value) == false;

      return SetValue(changed,
                      oldValue,
                      value,
                      name,
                      ref flag,
                      onChangedAction);
    }

    protected char[] SetValue(char[]                 oldValue,
                              char[]                 value,
                              string                 name,
                              ref ElementFieldFlags  flag,
                              Action<char[], char[]> onChangedAction = null)
    {
      bool changed;

      if (oldValue != null && value != null)
        changed = oldValue.SequenceEqual(value) == false;

      else
        changed = Equals(oldValue,
                         value) == false;

      return SetValue(changed,
                      oldValue,
                      value,
                      name,
                      ref flag,
                      onChangedAction);
    }

    protected T SetValue<T>(bool                  changed,
                            T                     oldValue,
                            T                     value,
                            string                name,
                            ref ElementFieldFlags flag,
                            Action<T, T>          onChangedAction = null)
    {
      if (changed)
      {
        ElementFieldFlags newFlag = FieldFlagMapping.SafeGet(name,
                                                             ElementFieldFlags.None);

        if (newFlag != ElementFieldFlags.None)
          flag |= newFlag;

        onChangedAction?.Invoke(oldValue,
                                value);

#if DEBUG && !DEBUG_IN_PROD
        System.Diagnostics.Debug.WriteLine("[{0} {1}] {2}: {3}",
                                           GetType().Name,
                                           Id,
                                           name,
                                           value);
#endif
      }

      return value;
    }

    protected T SetValue<T>(T      value,
                            string name)
    {
#if DEBUG && !DEBUG_IN_PROD
      System.Diagnostics.Debug.WriteLine("[{0} {1}] {2}: {3}",
                                         GetType().Name,
                                         Id,
                                         name,
                                         value);
#endif

      return value;
    }

    #endregion




    #region Methods Abs
    
    public abstract ElementType Type { get; }

    #endregion




    #region Events

    public event Action<SMElementChangedArgs> OnChanged;

    #endregion
  }
}
