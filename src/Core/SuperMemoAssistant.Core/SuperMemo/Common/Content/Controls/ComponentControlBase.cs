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
// Created On:   2018/06/20 19:20
// Modified On:  2018/08/31 14:02
// Modified By:  Alexis

#endregion




using System;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Controls
{
  /// <summary>
  /// Set as non-abstract to act as a generic control until all control types are implemented.
  /// </summary>
  public class ComponentControlBase : MarshalByRefObject, IControl
  {
    #region Properties & Fields - Non-Public

    protected readonly ControlGroup _group;

    #endregion




    #region Constructors

    public ComponentControlBase(int           id,
                                ComponentType type,
                                ControlGroup  group)
    {
      _group = group;
      Id     = id;
      Type   = type;
    }

    #endregion




    #region Properties Impl - Public

    public int Id { get; }

    public ComponentType Type { get; }

    public IControlGroup ControlGroup => _group;

    #endregion
  }
}
