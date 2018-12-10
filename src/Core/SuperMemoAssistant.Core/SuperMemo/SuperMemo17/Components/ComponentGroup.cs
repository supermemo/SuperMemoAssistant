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
// Created On:   2018/05/21 17:15
// Modified On:  2018/11/26 11:09
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using Anotar.Serilog;
using SuperMemoAssistant.Interop.SuperMemo.Components;
using SuperMemoAssistant.Interop.SuperMemo.Components.Types;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Components.Types;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Components
{
  public class ComponentGroup : MarshalByRefObject, IComponentGroup
  {
    #region Properties & Fields - Non-Public

    protected List<ComponentBase> ComponentsInternal { get; set; } = new List<ComponentBase>();

    #endregion




    #region Constructors

    public ComponentGroup(int offset)
    {
      Offset = offset;
    }

    #endregion




    #region Properties Impl - Public

    public IComponent this[int idx] => (IComponent)ComponentsInternal.ElementAtOrDefault(idx);
    public IEnumerable<IComponent> Components => ComponentsInternal.Select(c => (IComponent)c).ToList();
    public int                     Count      => ComponentsInternal.Count;
    public int                     Offset     { get; set; }

    #endregion




    #region Methods Impl

    public IComponentHtml GetFirstHtmlComponent()
    {
      return GetFirstComponent<IComponentHtml>();
    }

    public IComponentImage GetFirstImageComponent()
    {
      return GetFirstComponent<IComponentImage>();
    }

    public IComponentRtf GetFirstRtfComponent()
    {
      return GetFirstComponent<IComponentRtf>();
    }

    public IComponentShapeRectangle GetFirstRectangleShapeComponent()
    {
      return GetFirstComponent<IComponentShapeRectangle>();
    }

    public IComponentShapeRoundedRectangle GetFirstRoundedRectangleShapeComponent()
    {
      return GetFirstComponent<IComponentShapeRoundedRectangle>();
    }

    public IComponentShapeEllipse GetFirstEllipseComponent()
    {
      return GetFirstComponent<IComponentShapeEllipse>();
    }

    public IComponentSound GetFirstSoundComponent()
    {
      return GetFirstComponent<IComponentSound>();
    }

    public IComponentSpelling GetFirstSpellingComponent()
    {
      return GetFirstComponent<IComponentSpelling>();
    }

    public IComponentText GetFirstTextComponent()
    {
      return GetFirstComponent<IComponentText>();
    }

    public IComponentVideo GetFirstVideoComponent()
    {
      return GetFirstComponent<IComponentVideo>();
    }

    #endregion




    #region Methods

    private T GetFirstComponent<T>()
      where T : class, IComponent
    {
      if (ComponentsInternal == null || ComponentsInternal.Count == 0)
        return null;

      return ComponentsInternal.FirstOrDefault(c => c is T) as T;
    }

    internal void AddComponent(ComponentBase component)
    {
      ComponentsInternal.Add(component);
    }

    internal void Update(ComponentGroup cGroup)
    {
      ComponentsInternal = cGroup.ComponentsInternal;

      try
      {
        OnChanged?.Invoke(new SMComponentGroupArgs(SMA.Instance,
                                                   this));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Error while signaling ComponentGroup Update");
      }
    }

    #endregion




    #region Events

    // Events

    public event Action<SMComponentGroupArgs> OnChanged;

    #endregion
  }
}
