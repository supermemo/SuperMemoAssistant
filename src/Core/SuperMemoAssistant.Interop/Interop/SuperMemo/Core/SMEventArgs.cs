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
// Modified On:  2019/01/19 01:23
// Modified By:  Alexis

#endregion




using System;
using System.Diagnostics;
using SuperMemoAssistant.Interop.SuperMemo.Content;
using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Core
{
  /// <summary>Base event, contains a SM management instance</summary>
  [Serializable]
  public class SMEventArgs : EventArgs
  {
    #region Constructors

    public SMEventArgs(ISuperMemo smMgmt)
    {
      SMMgmt = smMgmt;
    }

    #endregion




    #region Properties & Fields - Public

    public ISuperMemo SMMgmt { get; set; }

    #endregion
  }

  /// <summary>SuperMemo App Process-related events, contains a definition of its Process</summary>
  [Serializable]
  public class SMProcessArgs : SMEventArgs
  {
    #region Constructors

    public SMProcessArgs(ISuperMemo smMgmt,
                         Process   process)
      : base(smMgmt)
    {
      Process = process;
    }

    #endregion




    #region Properties & Fields - Public

    public Process Process { get; }

    #endregion
  }

  /// <summary>Element-related events</summary>
  [Serializable]
  public class SMDisplayedElementChangedArgs : SMEventArgs
  {
    #region Constructors

    public SMDisplayedElementChangedArgs(ISuperMemo smMgmt,
                                         IElement   newElement,
                                         IElement   oldElement)
      : base(smMgmt)
    {
      NewElement = newElement;
      OldElement = oldElement;
    }

    #endregion




    #region Properties & Fields - Public

    public IElement NewElement { get; }
    public IElement OldElement { get; }

    #endregion
  }

  /// <summary>Element-related events</summary>
  [Serializable]
  public class SMElementArgs : SMEventArgs
  {
    #region Constructors

    public SMElementArgs(ISuperMemo smMgmt,
                         IElement   element)
      : base(smMgmt)
    {
      Element = element;
    }

    #endregion




    #region Properties & Fields - Public

    public IElement Element { get; }

    #endregion
  }

  /// <summary>Element change-related events</summary>
  [Serializable]
  public class SMElementChangedArgs : SMEventArgs
  {
    #region Constructors

    public SMElementChangedArgs(ISuperMemo        smMgmt,
                                IElement          element,
                                ElementFieldFlags changedFields)
      : base(smMgmt)
    {
      Element       = element;
      ChangedFields = changedFields;
    }

    #endregion




    #region Properties & Fields - Public

    public IElement          Element       { get; }
    public ElementFieldFlags ChangedFields { get; }

    #endregion
  }

  /// <summary>Registry member-related events</summary>
  [Serializable]
  public class SMRegistryArgs<T> : SMEventArgs
    where T : IRegistryMember
  {
    #region Constructors

    public SMRegistryArgs(ISuperMemo smMgmt,
                          T          member)
      : base(smMgmt)
    {
      Member = member;
    }

    #endregion




    #region Properties & Fields - Public

    public T Member { get; }

    #endregion
  }

  /// <summary>Component-related events</summary>
  [Serializable]
  public class SMComponentChangedArgs : SMEventArgs
  {
    #region Constructors

    public SMComponentChangedArgs(ISuperMemo          smMgmt,
                                  IComponent          component,
                                  ComponentFieldFlags changedFields)
      : base(smMgmt)
    {
      Component     = component;
      ChangedFields = changedFields;
    }

    #endregion




    #region Properties & Fields - Public

    public IComponent          Component     { get; }
    public ComponentFieldFlags ChangedFields { get; }

    #endregion
  }

  /// <summary>Component group-related events</summary>
  [Serializable]
  public class SMComponentGroupArgs : SMEventArgs
  {
    #region Constructors

    public SMComponentGroupArgs(ISuperMemo      smMgmt,
                                IComponentGroup componentGroup)
      : base(smMgmt)
    {
      ComponentGroup = componentGroup;
    }

    #endregion




    #region Properties & Fields - Public

    public IComponentGroup ComponentGroup { get; }

    #endregion
  }
}
