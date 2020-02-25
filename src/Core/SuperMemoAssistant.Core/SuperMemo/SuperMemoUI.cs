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
// Created On:   2019/03/02 18:29
// Modified On:  2019/07/22 12:48
// Modified By:  Alexis

#endregion




using PluginManager.Interop.Sys;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.UI.Element;
using SuperMemoAssistant.SuperMemo.SuperMemo17.UI;

namespace SuperMemoAssistant.SuperMemo
{
  public class SuperMemoUICore : SuperMemoUI
  {
    #region Constructors

    public SuperMemoUICore()
    {
      base.ElementWdw = ElementWdw = new ElementWdw();
    }

    #endregion




    #region Properties & Fields - Public

    public new ElementWdw ElementWdw { get; }

    #endregion
  }

  public class SuperMemoUI : PerpetualMarshalByRefObject, ISuperMemoUI
  {
    #region Constructors

    protected SuperMemoUI() { }

    #endregion




    #region Properties Impl - Public

    public IElementWdw ElementWdw { get; protected set; }

    #endregion
  }
}
