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

#endregion




namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Elements.Types
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Diagnostics.CodeAnalysis;
  using System.Globalization;
  using Anotar.Serilog;
  using Extensions;
  using Interop.SuperMemo.Content;
  using Interop.SuperMemo.Core;
  using Interop.SuperMemo.Elements.Models;
  using Interop.SuperMemo.Elements.Types;
  using Interop.SuperMemo.Registry.Members;
  using Newtonsoft.Json;
  using PluginManager.Interop.Sys;
  using PropertyChanged;
  using SMA;
  using Sys.Converters.Json;

  [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<Pending>")]
  [SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification          = "<Pending>")]
  public abstract class ElementBase : PerpetualMarshalByRefObject, IElement, INotifyPropertyChanged, IDisposable
  {
    #region Constants & Statics

    //
    // Internal helpers

    public static IReadOnlyDictionary<string, ElementFieldFlags> FieldFlagMapping { get; } = new Dictionary<string, ElementFieldFlags>()
    {
      { nameof(TitleTextId), ElementFieldFlags.Name },
      { nameof(CommentId), ElementFieldFlags.Comment },
      { nameof(Deleted), ElementFieldFlags.Deleted },
      { nameof(TemplateId), ElementFieldFlags.Template },
      { nameof(ConceptId), ElementFieldFlags.Concept },
      { nameof(ComponentPos), ElementFieldFlags.Components },
      { nameof(AFactor), ElementFieldFlags.AFactor },
      { nameof(ParentId), ElementFieldFlags.Parent },
      { nameof(FirstChildId), ElementFieldFlags.FirstChild },
      { nameof(LastChildId), ElementFieldFlags.LastChild },
      { nameof(NextSiblingId), ElementFieldFlags.NextSibling },
      { nameof(PrevSiblingId), ElementFieldFlags.PrevSibling },
      { nameof(DescendantCount), ElementFieldFlags.DescendantCount },
      { nameof(ChildrenCount), ElementFieldFlags.ChildrenCount },
    };

    #endregion




    #region Properties & Fields - Non-Public

    protected abstract string TypeName { get; }

    #endregion




    #region Constructors

    protected ElementBase(int id)
    {
      Id = id;

#if DEBUG_REGISTRIES
      LogTo.Debug("[{0} {1}] Creating",
                  GetType().Name,
                  Id);
#endif
    }

    public void Dispose()
    {
      // TODO: Clear events handlers
      // TODO: Call this method appropriately

      // OnComponentPosChanged(ComponentPos, -1);
      ComponentPos = -1;

      GC.SuppressFinalize(this);
    }

    #endregion




    #region Properties & Fields - Public

    public int TitleTextId { get; set; }
    public int CommentId { get; set; }

    public int TemplateId { get; set; }
    public int ConceptId  { get; set; }

    public int ComponentPos { get; set; } = -1;

    public byte[] AFactor { get; set; }

    public int ParentId      { get; set; }
    public int FirstChildId  { get; set; }
    public int LastChildId   { get; set; }
    public int NextSiblingId { get; set; }
    public int PrevSiblingId { get; set; }

    #endregion




    #region Properties Impl - Public

    public int  Id      { get; set; }
    public bool Deleted { get; set; }

    public int DescendantCount { get; set; }
    public int ChildrenCount   { get; set; }

    public string Title => Core.SM.Registry.Text?[TitleTextId]?.Name;
    public string Comment => Core.SM.Registry.Comment?[CommentId]?.Name;

    [JsonConverter(typeof(IComponentGroupToComponentGroupIdJsonConverter))]
    public IComponentGroup ComponentGroup => Core.SM.Registry.Component?[ComponentPos];
    [JsonConverter(typeof(ITemplateToTemplateIdJsonConverter))]
    public ITemplate Template => Core.SM.Registry.Template?[TemplateId];
    [JsonConverter(typeof(IConceptToConceptIdJsonConverter))]
    public IConcept Concept => Core.SM.Registry.Concept?[ConceptId];

    [JsonConverter(typeof(IElementToElementIdJsonConverter))]
    public IElement Parent => Core.SM.Registry.Element?[ParentId];
    [JsonIgnore]
    public IElement FirstChild => Core.SM.Registry.Element?[FirstChildId];
    [JsonIgnore]
    public IElement LastChild => Core.SM.Registry.Element?[LastChildId];
    [JsonIgnore]
    public IElement NextSibling => Core.SM.Registry.Element?[NextSiblingId];
    [JsonIgnore]
    public IElement PrevSibling => Core.SM.Registry.Element?[PrevSiblingId];

    [JsonProperty(ItemConverterType = typeof(IElementToElementIdJsonConverter))]
    public IEnumerable<IElement> Children => EnumerateChildren();

    #endregion




    #region Methods Impl

    public override string ToString()
    {
      return $"({Id}|{TypeName[0]}) {Title}";
    }

    public string ToJson()
    {
      return this.Serialize(Formatting.Indented);
    }

    public bool Delete()
    {
      throw new NotImplementedException();
    }

    public bool Display()
    {
      return Core.SM.UI.ElementWdw.GoToElement(Id);
    }

    public bool Done()
    {
      throw new NotImplementedException();
    }

    public bool MoveTo(IElement newParent)
    {
      throw new NotImplementedException();
    }

    #endregion




    #region Methods

    public void OnUpdated(ElementFieldFlags flags)
    {
      try
      {
        OnChanged?.Invoke(new SMElementChangedEventArgs(Core.SM,
                                                        (IElement)this,
                                                        flags));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Error while signaling Element Changed event");
      }
    }

    [SuppressPropertyChangedWarnings]
    protected void OnComponentPosChanged(int oldCompPos,
                                         int newCompPos)
    {
      if (Id <= 0 || Core.SM.Registry.Component == null)
        return;

      if (oldCompPos >= 0)
      {
        var comp = Core.SM.Registry.Component[oldCompPos];

        if (comp != null)
          comp.OnChanged -= OnComponentModified;
      }


      if (newCompPos >= 0)
      {
        var comp = Core.SM.Registry.Component[newCompPos];

        if (comp != null)
          comp.OnChanged += OnComponentModified;
      }
    }

    protected void OnComponentModified(SMComponentGroupEventArgs eventArgs)
    {
      try
      {
        OnChanged?.Invoke(new SMElementChangedEventArgs(Core.SM,
                                                        (IElement)this,
                                                        ElementFieldFlags.Components));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Error while signaling Component Changed event");
      }
    }

    public IEnumerable<IElement> EnumerateChildren()
    {
      try
      {
        List<IElement> ret = new(ChildrenCount);

        if (ChildrenCount <= 0 || FirstChild == null)
          return ret;

        var itEl = FirstChild;

        do
        {
          if (itEl.Deleted == false)
            ret.Add(itEl);

          itEl = itEl.NextSibling;
        } while (itEl != null);

        return ret;
      }
      catch (Exception)
      {
        return new List<IElement>();
      }

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

    /// <summary>
    ///   Raises the <see cref="PropertyChanged" /> event for Property <paramref name="propertyName" />. Called by
    ///   Fody.PropertyChanged
    /// </summary>
    /// <param name="propertyName">The changed property's name</param>
    /// <param name="before">The old value</param>
    /// <param name="after">The new value</param>
    protected virtual void OnPropertyChanged(string propertyName, object before, object after)
    {
#if DEBUG_REGISTRIES
      LogTo.Debug("[{0} {1}] {2}: {3}",
                  GetType().Name,
                  Id,
                  propertyName,
                  after);
#endif

      if (propertyName is nameof(ComponentPos))
        OnComponentPosChanged((int)before, (int)after);

      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion




    #region Methods Abs

    public abstract ElementType Type { get; }

    /// <inheritdoc/>
    /// <remarks>GitHub issue: https://github.com/supermemo/SuperMemoAssistant/issues/216</remarks>
    public string UniqueId => Id.ToString(CultureInfo.InvariantCulture);

    #endregion




    #region Events

    public event Action<SMElementChangedEventArgs> OnChanged;

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
