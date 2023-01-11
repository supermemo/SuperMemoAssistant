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
// Created On:   2020/01/13 16:38
// Modified On:  2022/12/17 21:01
// Modified By:  - Alexis
//               - Ki

#endregion




using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.SuperMemo.Common.Elements;
using SuperMemoAssistant.SuperMemo.Common.Extensions;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Elements.Types;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;
using SuperMemoAssistant.Sys.SparseClusteredArray;

#if DEBUG_REGISTRIES
using Anotar.Serilog;
#endif


namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Elements
{
  internal class ElementRegistryUpdater17 : IElementRegistryUpdater
  {
    #region Properties & Fields - Non-Public

    private readonly Func<int, ElementBase>                 _getElementFunc;
    private readonly Func<int, ElementType, ElementBase>    _createFunc;
    private readonly Action<ElementBase>                    _onElementCreatedCallback;
    private readonly Action<ElementBase, ElementFieldFlags> _onElementUpdatedCallback;

    #endregion




    #region Constructors

    public ElementRegistryUpdater17(
      Func<int, ElementBase>                 getElementFunc,
      Func<int, ElementType, ElementBase>    createFunc,
      Action<ElementBase>                    onElementCreatedCallback,
      Action<ElementBase, ElementFieldFlags> onElementUpdatedCallback)
    {
      _getElementFunc           = getElementFunc;
      _createFunc               = createFunc;
      _onElementCreatedCallback = onElementCreatedCallback;
      _onElementUpdatedCallback = onElementUpdatedCallback;
    }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public void CommitFromFiles(SMCollection collection)
    {
      using (Stream cttStream = File.OpenRead(collection.GetInfoFilePath(SMConst.Files.ContentsFileName)))
      using (Stream elStream = File.OpenRead(collection.GetInfoFilePath(SMConst.Files.ElementsInfoFileName)))
      {
        var cttElems = cttStream.StreamToStruct<InfContentsElem17, InfContentsElem17>(
          InfContentsElem17.SizeOfContentsElem,
          s => s
        );

        var elElems = elStream.StreamToStruct<InfElementsElemContainer17, InfElementsElem17>(
          InfElementsElem17.SizeOfElementsElem,
          e => new InfElementsElemContainer17(e)
        );

        foreach (int id in cttElems.Keys.Union(elElems.Keys))
          Commit(id,
                 cttElems.SafeGet(id),
                 elElems.SafeGet(id));
      }
    }

    /// <inheritdoc />
    public void CommitFromMemory(SparseClusteredArray<byte> contentsSCA, SparseClusteredArray<byte> elementsSCA)
    {
      var cttElems = new Dictionary<int, InfContentsElem17>();
      var elElems  = new Dictionary<int, InfElementsElemContainer17>();

      foreach (var cttStream in contentsSCA.GetStreams())
        cttStream.StreamToStruct<InfContentsElem17, InfContentsElem17>(
          InfContentsElem17.SizeOfContentsElem,
          e => e,
          cttElems
        );

      foreach (var elStream in elementsSCA.GetStreams())
        elStream.StreamToStruct<InfElementsElemContainer17, InfElementsElem17>(
          InfElementsElem17.SizeOfElementsElem,
          e => new InfElementsElemContainer17(e),
          elElems
        );

      foreach (int id in cttElems.Keys)
        Commit(id,
               cttElems.SafeGet(id),
               null);

      foreach (int id in elElems.Keys)
        Commit(id,
               null,
               elElems.SafeGet(id));
    }

    #endregion




    #region Methods

    protected virtual void SetupElement(
      ElementBase                elem,
      InfContentsElem17          cttElem,
      InfElementsElemContainer17 elElem)
    {
      elem.TitleTextId = elElem._elem.titleTextId;
      elem.CommentId   = elElem._elem.commentId;
      elem.Deleted     = cttElem != null && cttElem.deleted != 0;

      elem.TemplateId = elElem._elem.templateId;
      elem.ConceptId  = elElem._elem.conceptId;

      // elem.OnComponentPosChanged(-1, ComponentPos);
      elem.ComponentPos = elElem._elem.componPos;
      // elem.AFactor = elElem._elem.AFactor;

      if (cttElem != null)
      {
        elem.ParentId      = cttElem.parentId;
        elem.FirstChildId  = cttElem.firstChildId;
        elem.LastChildId   = cttElem.lastChildId;
        elem.NextSiblingId = cttElem.nextSiblingId;
        elem.PrevSiblingId = cttElem.prevSiblingId;

        elem.DescendantCount = cttElem.descendantCount;
        elem.ChildrenCount   = cttElem.childrenCount;
      }
    }

    protected virtual ElementFieldFlags UpdateElement(
      ElementBase                elem,
      InfContentsElem17          cttElem,
      InfElementsElemContainer17 elElem)
    {
#if DEBUG_REGISTRIES
      LogTo.Debug("[{0} {1}] Updating",
                  elem.GetType().Name,
                  elem.Id);
#endif

      ElementFieldFlags flags = ElementFieldFlags.None;

      void UpdateFlag(object sender, PropertyChangedEventArgs e)
      {
        var fieldFlag = ElementBase.FieldFlagMapping.SafeRead(e.PropertyName);

        flags |= fieldFlag;
      }

      elem.PropertyChanged += UpdateFlag;

      if (elElem != null)
      {
        elem.TitleTextId = elElem._elem.titleTextId;
        elem.CommentId   = elElem._elem.commentId;

        elem.TemplateId = elElem._elem.templateId;
        elem.ConceptId  = elElem._elem.conceptId;

        elem.ComponentPos = elElem._elem.componPos;
        // elem.AFactor = elElem._elem.AFactor
      }

      if (cttElem != null)
      {
        elem.Deleted = cttElem.deleted != 0;

        elem.ParentId      = cttElem.parentId;
        elem.FirstChildId  = cttElem.firstChildId;
        elem.LastChildId   = cttElem.lastChildId;
        elem.NextSiblingId = cttElem.nextSiblingId;
        elem.PrevSiblingId = cttElem.prevSiblingId;

        elem.DescendantCount = cttElem.descendantCount;
        elem.ChildrenCount   = cttElem.childrenCount;
      }

      elem.PropertyChanged -= UpdateFlag;

      elem.OnUpdated(flags);

      return flags;
    }

    protected virtual void Commit(int                        id,
                                  InfContentsElem17          cttElem,
                                  InfElementsElemContainer17 elElem)
    {
      var el = _getElementFunc(id);

      if (el != null)
      {
        var flags = UpdateElement(el, cttElem, elElem);

        _onElementUpdatedCallback(el, flags);
      }

      else
      {
        el = _createFunc(id, (ElementType)elElem._elem.elementType);

        SetupElement(el, cttElem, elElem);

        _onElementCreatedCallback(el);
      }
    }

    #endregion
  }
}
