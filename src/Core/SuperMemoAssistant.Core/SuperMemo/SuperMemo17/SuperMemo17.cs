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
// Created On:   2018/05/08 01:24
// Modified On:  2019/01/15 12:35
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using SuperMemoAssistant.Hooks;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Components;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Elements;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Types;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17
{
  public class SM17
    : SuperMemoBase
  {
    #region Constants & Statics

    public const string RE_WindowTitle = "([^\\(]+) \\(SuperMemo 17: (.+)\\)";

    #endregion




    #region Constructors

    /// <summary>SM17 Management interface</summary>
    /// <param name="collection">Target collection to open</param>
    /// <param name="binPath">SuperMemo bin path</param>
    public SM17(SMCollection collection,
                string       binPath)
      : base(collection,
             binPath) { }

    #endregion




    #region Properties Impl - Public

    public override SMAppVersion AppVersion => SMConst.Versions.v17_3;

    #endregion




    #region Methods Impl

    //
    // Init/Hooks-related

    protected override IEnumerable<ISMHookIO> GetIOCallbacks()
    {
      return new ISMHookIO[]
      {
        ElementRegistry.Instance,
        ComponentRegistry.Instance,
        TextRegistry.Instance,
        BinaryRegistry.Instance,
        ConceptRegistry.Instance,
        ImageRegistry.Instance,
        TemplateRegistry.Instance,
        SoundRegistry.Instance,
        VideoRegistry.Instance
      };
    }

    #endregion
  }
}
