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
// Modified On:  2019/01/15 17:54
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using Anotar.Serilog;
using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
using SuperMemoAssistant.Interop.SuperMemo.Content.Components.Base;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.SuperMemo.Common.Content.Components;

namespace SuperMemoAssistant.SuperMemo.Common.Content
{
  public class ComponentGroup : ComponentGroupBase
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

    public override IComponent this[int idx] => (IComponent)ComponentsInternal.ElementAtOrDefault(idx);
    public override IEnumerable<IComponent> Components => ComponentsInternal.Select(c => (IComponent)c).ToList();
    public override int                     Count      => ComponentsInternal.Count;
    public override int                     Offset     { get; }

    #endregion




    #region Methods Impl

    protected override T GetFirstComponent<T>()
    {
      if (ComponentsInternal == null || ComponentsInternal.Count == 0)
        return null;

      return ComponentsInternal.FirstOrDefault(c => c is T) as T;
    }

    #endregion




    #region Methods

    internal void AddComponent(ComponentBase component)
    {
      ComponentsInternal.Add(component);
    }

    internal void Update(ComponentGroup cGroup)
    {
      ComponentsInternal = cGroup.ComponentsInternal;

      try
      {
        OnChanged?.Invoke(new SMComponentGroupArgs(Core.SM, this));
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

    public override event Action<SMComponentGroupArgs> OnChanged;

    #endregion
  }
}
