using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Elements.Types
{
  public abstract class ElementBase : MarshalByRefObject, IDisposable
  {
    public int Id { get; protected set; }
    public int TitleTextId { get; protected set; }
    public bool Deleted { get; protected set; }

    public int TemplateId { get; protected set; }
    public int ConceptId { get; protected set; }

    public int ComponentPos { get; protected set; }
    public char[] AF { get; protected set; }

    public int ParentId { get; protected set; }
    public int FirstChildId { get; protected set; }
    public int LastChildId { get; protected set; }
    public int NexSiblingId { get; protected set; }
    public int PrevSiblingId { get; protected set; }

    public int DescendantCount { get; protected set; }
    public int ChildrenCount { get; protected set; }



    protected ElementBase(int id, InfContentsElem cttElem, InfElementsElem elElem)
    {
      Id = id;

#if DEBUG
      System.Diagnostics.Debug.WriteLine("[{0} {1}] Creating", this.GetType().Name, Id);
#endif

      TitleTextId = SetValue(elElem.titleTextId, nameof(TitleTextId));
      Deleted = SetValue(cttElem.deleted != 0, nameof(Deleted));

      TemplateId = SetValue(elElem.templateId, nameof(TemplateId));
      ConceptId = SetValue(elElem.conceptId, nameof(ConceptId));

      ComponentPos = SetValue(elElem.componPos, nameof(ComponentPos));
      OnComponentPosChanged(-1, ComponentPos);
      //AF = SetDbg(elElem.AF, nameof(AF));

      ParentId = SetValue(cttElem.parentId, nameof(ParentId));
      FirstChildId = SetValue(cttElem.firstChildId, nameof(FirstChildId));
      LastChildId = SetValue(cttElem.lastChildId, nameof(LastChildId));
      NexSiblingId = SetValue(cttElem.nextSiblingId, nameof(NexSiblingId));
      PrevSiblingId = SetValue(cttElem.prevSiblingId, nameof(PrevSiblingId));

      DescendantCount = SetValue(cttElem.descendantCount, nameof(DescendantCount));
      ChildrenCount = SetValue(cttElem.childrenCount, nameof(ChildrenCount));
    }

    public void Dispose()
    {
      // TODO: Clear events handlers
      // TODO: Call this method appropriately

      OnComponentPosChanged(ComponentPos, -1);
    }

    public ElementFieldFlags Update(InfContentsElem cttElem, InfElementsElem elElem)
    {
#if DEBUG
      System.Diagnostics.Debug.WriteLine("[{0} {1}] Updating", this.GetType().Name, Id);
#endif

      // TODO: Set/Clear events handlers on component change
      ElementFieldFlags flags = ElementFieldFlags.None;

      TitleTextId = SetValue(TitleTextId, elElem.titleTextId, nameof(TitleTextId), ref flags);
      Deleted = SetValue(Deleted, cttElem.deleted != 0, nameof(Deleted), ref flags);

      TemplateId = SetValue(TemplateId, elElem.templateId, nameof(TemplateId), ref flags);
      ConceptId = SetValue(ConceptId, elElem.conceptId, nameof(ConceptId), ref flags);

      ComponentPos = SetValue(ComponentPos, elElem.componPos, nameof(ComponentPos), ref flags, OnComponentPosChanged);
      //AF = SetDbg(elElem.AF, nameof(AF));

      ParentId = SetValue(ParentId, cttElem.parentId, nameof(ParentId), ref flags);
      FirstChildId = SetValue(FirstChildId,cttElem.firstChildId, nameof(FirstChildId), ref flags);
      LastChildId = SetValue(LastChildId, cttElem.lastChildId, nameof(LastChildId), ref flags);
      NexSiblingId = SetValue(NexSiblingId, cttElem.nextSiblingId, nameof(NexSiblingId), ref flags);
      PrevSiblingId = SetValue(PrevSiblingId, cttElem.prevSiblingId, nameof(PrevSiblingId), ref flags);

      DescendantCount = SetValue(DescendantCount, cttElem.descendantCount, nameof(DescendantCount), ref flags);
      ChildrenCount = SetValue(ChildrenCount, cttElem.childrenCount, nameof(ChildrenCount), ref flags);

      try
      {
        OnChanged?.Invoke(new SMElementChangedArgs(SMA.Instance, (IElement)this, flags));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Error while signaling Element Changed event");
      }

      return flags;
    }

    protected void OnComponentPosChanged(int oldCompPos, int newCompPos)
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
        OnChanged?.Invoke(new SMElementChangedArgs(SMA.Instance, (IElement)this, ElementFieldFlags.Components));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Error while signaling Component Changed event");
      }
    }

    public string Title => SMA.Instance.Registry.Text?[TitleTextId]?.Name;

    public IElement Template => SMA.Instance.Registry.Element?[TemplateId];
    public IConcept Concept => SMA.Instance.Registry.Concept?[ConceptId];

    public IElement Parent => SMA.Instance.Registry.Element?[ParentId];
    public IElement FirstChild => SMA.Instance.Registry.Element?[FirstChildId];
    public IElement LastChild => SMA.Instance.Registry.Element?[LastChildId];
    public IElement NexSibling => SMA.Instance.Registry.Element?[NexSiblingId];
    public IElement PrevSibling => SMA.Instance.Registry.Element?[PrevSiblingId];


    public Task<bool> Delete()
    {
      throw new System.NotImplementedException();
    }

    public Task<bool> Display()
    {
      throw new System.NotImplementedException();
    }

    public Task<bool> Done()
    {
      throw new System.NotImplementedException();
    }

    public Task<bool> MoveTo(IConceptGroup newParent)
    {
      throw new System.NotImplementedException();
    }



    //
    // Events

    public event Action<SMElementChangedArgs> OnChanged;



    //
    // Internal helpers

    protected Dictionary<string, ElementFieldFlags> FieldFlagMapping = new Dictionary<string, ElementFieldFlags>()
    {
      { "TextId", ElementFieldFlags.Name },
      { "Deleted", ElementFieldFlags.Deleted },
      { "TemplateId", ElementFieldFlags.Template },
      { "ConceptId", ElementFieldFlags.Concept },
      { "ComponentPos", ElementFieldFlags.Components },
      { "AF", ElementFieldFlags.AFactor },
      { "ParentId", ElementFieldFlags.Parent },
      { "FirstChildId", ElementFieldFlags.FirstChild },
      { "LastChildId", ElementFieldFlags.LastChild },
      { "NexSiblingId", ElementFieldFlags.NextSibling },
      { "PrevSiblingId", ElementFieldFlags.PrevSibling },
      { "DescendantCount", ElementFieldFlags.DescendantCount },
      { "ChildrenCount", ElementFieldFlags.ChildrenCount },
    };

    protected T SetValue<T>(T oldValue, T value, string name, ref ElementFieldFlags flag, Action<T, T> onChangedAction = null)
    {
      bool changed = Object.Equals(oldValue, value) == false;

      return SetValue(changed, oldValue, value, name, ref flag, onChangedAction);
    }

    protected char[] SetValue(char[] oldValue, char[] value, string name, ref ElementFieldFlags flag, Action<char[], char[]> onChangedAction = null)
    {
      bool changed;

      if (oldValue != null && value != null)
        changed = oldValue.SequenceEqual(value) == false;

      else
        changed = Object.Equals(oldValue, value) == false;

      return SetValue(changed, oldValue, value, name, ref flag, onChangedAction);
    }

    protected T SetValue<T>(bool changed, T oldValue, T value, string name, ref ElementFieldFlags flag, Action<T, T> onChangedAction = null)
    {
      if (changed)
      {
        ElementFieldFlags newFlag = FieldFlagMapping.SafeGet(name, ElementFieldFlags.None);

        if (newFlag != ElementFieldFlags.None)
          flag |= newFlag;

        onChangedAction?.Invoke(oldValue, value);

#if DEBUG
        System.Diagnostics.Debug.WriteLine("[{0} {1}] {2}: {3}", this.GetType().Name, Id, name, value);
#endif
      }

      return value;
    }

    protected T SetValue<T>(T value, string name)
    {
#if DEBUG
      System.Diagnostics.Debug.WriteLine("[{0} {1}] {2}: {3}", this.GetType().Name, Id, name, value);
#endif

      return value;
    }
  }
}
