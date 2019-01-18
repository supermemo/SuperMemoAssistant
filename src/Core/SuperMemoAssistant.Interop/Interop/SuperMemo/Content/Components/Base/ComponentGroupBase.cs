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
// Created On:   2019/01/15 17:47
// Modified On:  2019/01/15 17:50
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using SuperMemoAssistant.Interop.SuperMemo.Core;

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Components.Base
{
  public abstract class ComponentGroupBase : MarshalByRefObject, IComponentGroup
  {
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




    #region Methods Abs

    public abstract IComponent this[int idx] { get; }
    public abstract IEnumerable<IComponent> Components { get; }
    public abstract int                     Count      { get; }
    public abstract int                     Offset     { get; }


    protected abstract T GetFirstComponent<T>()
      where T : class, IComponent;

    #endregion




    #region Events

    public abstract event Action<SMComponentGroupArgs> OnChanged;

    #endregion
  }
}
