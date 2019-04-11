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
// Created On:   2019/04/11 19:55
// Modified On:  2019/04/11 20:03
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Content;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Elements.Types
{
  public class ElementWrapper : IElement
  {
    #region Constructors

    public ElementWrapper(IElement element)
    {
      Original = element;
    }

    #endregion




    #region Properties & Fields - Public

    public IElement Original { get; }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public int Id => Original.Id;
    /// <inheritdoc />
    public string Title => Original.Title;
    /// <inheritdoc />
    public bool Deleted => Original.Deleted;
    /// <inheritdoc />
    public ElementType Type => Original.Type;
    /// <inheritdoc />
    public IComponentGroup ComponentGroup => Original.ComponentGroup;
    /// <inheritdoc />
    public IElement Template => new ElementWrapper(Original.Template);
    /// <inheritdoc />
    public IConcept Concept => Original.Concept;
    /// <inheritdoc />
    public IElement Parent => new ElementWrapper(Original.Parent);
    /// <inheritdoc />
    public IElement FirstChild => new ElementWrapper(Original.FirstChild);
    /// <inheritdoc />
    public IElement LastChild => new ElementWrapper(Original.LastChild);
    /// <inheritdoc />
    public IElement NextSibling => new ElementWrapper(Original.NextSibling);
    /// <inheritdoc />
    public IElement PrevSibling => new ElementWrapper(Original.PrevSibling);
    /// <inheritdoc />
    public int DescendantCount => Original.DescendantCount;
    /// <inheritdoc />
    public int ChildrenCount => Original.ChildrenCount;
    /// <inheritdoc />
    public IEnumerable<IElement> Children => Original.Children.Select(c => new ElementWrapper(c));

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public Task<bool> Display() => Original.Display();

    /// <inheritdoc />
    public Task<bool> MoveTo(IConceptGroup newParent) => Original.MoveTo(newParent);

    /// <inheritdoc />
    public Task<bool> Delete() => Original.Delete();

    /// <inheritdoc />
    public Task<bool> Done() => Original.Done();

    #endregion




    #region Events

    /// <inheritdoc />
    public event Action<SMElementChangedArgs> OnChanged;

    #endregion
  }
}
