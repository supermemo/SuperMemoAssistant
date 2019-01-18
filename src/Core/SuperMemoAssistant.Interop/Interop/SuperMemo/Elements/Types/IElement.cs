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
// Created On:   2018/07/27 12:55
// Modified On:  2019/01/01 18:09
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Content;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Elements.Types
{
  public interface IElement
  {
    int    Id      { get; }
    string Title   { get; }
    bool   Deleted { get; }

    ElementType Type { get; }

    IComponentGroup ComponentGroup { get; }
    IElement        Template       { get; }
    IConcept        Concept        { get; }


    //
    // Knowledge Tree

    IElement Parent      { get; }
    IElement FirstChild  { get; }
    IElement LastChild   { get; }
    IElement NextSibling { get; }
    IElement PrevSibling { get; }

    int DescendantCount { get; }
    int ChildrenCount   { get; }

    IEnumerable<IElement> Children { get; }

    //
    // Helpers

    /// <summary>
    ///   Conveniency method. Will run UI automation to execute action. Display this Element in
    ///   the Element Window.
    /// </summary>
    /// <returns>Waitable task yielding success result of the operation</returns>
    Task<bool> Display();

    /// <summary>
    ///   Conveniency method. Will run UI automation to execute action. Move this element (and
    ///   descendants) under <paramref name="newParent" />. Only works with concepts (No UI automation
    ///   possible on TreeViews).
    /// </summary>
    /// <param name="newParent"></param>
    /// <returns>Waitable task yielding success result of the operation</returns>
    Task<bool> MoveTo(IConceptGroup newParent);

    /// <summary>
    ///   Conveniency method. Will run UI automation to execute action. WARNING: Use with
    ///   caution ! Delete this element AND ALL ITS CHILDREN.
    /// </summary>
    /// <returns>Waitable task yielding success result of the operation</returns>
    Task<bool> Delete();

    /// <summary>
    ///   Conveniency method. Will run UI automation to execute action. Remove content only,
    ///   from this element only, and dismiss it from learning queue. Descendants are left untouched.
    /// </summary>
    /// <returns>Waitable task yielding success result of the operation</returns>
    Task<bool> Done();


    //
    // Events

    event Action<SMElementChangedArgs> OnChanged;
  }
}
