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
// Created On:   2019/08/07 15:17
// Modified On:  2019/08/08 11:10
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Anotar.Serilog;
using PropertyChanged;
using SuperMemoAssistant.Interop.SuperMemo.Content;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SMA;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Elements.Types
{
  public abstract class ElementBase : MarshalByRefObject, IElement, INotifyPropertyChanged, IDisposable
  {
    #region Constants & Statics

    //
    // Internal helpers

    public static IReadOnlyDictionary<string, ElementFieldFlags> FieldFlagMapping { get; } = new Dictionary<string, ElementFieldFlags>()
    {
      { nameof(TitleTextId), ElementFieldFlags.Name },
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

#if DEBUG && !DEBUG_IN_PROD
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
    }

    #endregion




    #region Properties & Fields - Public

    public int TitleTextId { get; set; }

    public int TemplateId { get; set; }
    public int ConceptId  { get; set; }

    public int    ComponentPos { get; set; } = -1;
    public byte[] AFactor      { get; set; }

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

    public IComponentGroup ComponentGroup => Core.SM.Registry.Component?[ComponentPos];
    public IElement        Template       => Core.SM.Registry.Element?[TemplateId];
    public IConcept        Concept        => Core.SM.Registry.Concept?[ConceptId];

    public IElement Parent      => Core.SM.Registry.Element?[ParentId];
    public IElement FirstChild  => Core.SM.Registry.Element?[FirstChildId];
    public IElement LastChild   => Core.SM.Registry.Element?[LastChildId];
    public IElement NextSibling => Core.SM.Registry.Element?[NextSiblingId];
    public IElement PrevSibling => Core.SM.Registry.Element?[PrevSiblingId];

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

    public void OnUpdated(ElementFieldFlags flags)
    {
      try
      {
        OnChanged?.Invoke(new SMElementChangedArgs(Core.SM,
                                                   (IElement)this,
                                                   flags));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Error while signaling Element Changed event");
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

    protected void OnComponentModified(SMComponentGroupArgs args)
    {
      try
      {
        OnChanged?.Invoke(new SMElementChangedArgs(Core.SM,
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
        if (ChildrenCount <= 0 || FirstChild == null)
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
    
    /// <summary>
    ///   Raises the <see cref="PropertyChanged" /> event for Property
    ///   <paramref name="propertyName" />. Called by Fody.PropertyChanged
    /// </summary>
    /// <param name="propertyName">The changed property's name</param>
    /// <param name="before">The old value</param>
    /// <param name="after">The new value</param>
    protected virtual void OnPropertyChanged(string propertyName, object before, object after)
    {
#if DEBUG && !DEBUG_IN_PROD
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

    #endregion




    #region Events

    public event Action<SMElementChangedArgs> OnChanged;

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
