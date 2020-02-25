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
// Modified On:  2020/02/21 20:04
// Modified By:  Alexis

#endregion




using Anotar.Serilog;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.SuperMemo.Common.Elements;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Elements.Types;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Elements
{
  public class ElementRegistry17 : ElementRegistryBase
  {
    #region Properties & Fields - Non-Public

    /// <inheritdoc />
    protected override IElementRegistryUpdater Updater { get; }

    #endregion




    #region Constructors

    /// <inheritdoc />
    public ElementRegistry17()
    {
      Updater = new ElementRegistryUpdater17(
        GetInternal,
        CreateInternal,
        OnElementCreatedInternal,
        OnElementUpdatedInternal);
    }

    #endregion




    #region Methods Impl

    protected override ElementBase CreateInternal(
      int         id,
      ElementType elementType)
    {
      switch (elementType)
      {
        case ElementType.Topic:
          return Elements[id] = new Topic(id);

        case ElementType.Item:
          return Elements[id] = new Item(id);

        case ElementType.ConceptGroup:
          return Elements[id] = new ConceptGroup(id);

        case ElementType.Task:
          return Elements[id] = new Task(id);
      }

      LogTo.Warning($"Creating element with unknown type {elementType} for element id {id}");

      return new Topic(id);
    }

    #endregion
  }
}
