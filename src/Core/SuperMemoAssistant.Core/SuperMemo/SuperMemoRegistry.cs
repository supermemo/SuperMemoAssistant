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
// Created On:   2018/06/01 22:40
// Modified On:  2018/06/01 22:40
// Modified By:  Alexis

#endregion




using System;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Components.Types;
using SuperMemoAssistant.Interop.SuperMemo.Elements;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Types;
using SuperMemoAssistant.Sys;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Components;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Elements;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Types;

namespace SuperMemoAssistant.SuperMemo
{
  public class SuperMemoRegistry : SMMarshalByRefObject, ISuperMemoRegistry
  {
    #region Constants & Statics

    public static SuperMemoRegistry Instance { get; } = new SuperMemoRegistry();

    #endregion




    #region Constructors

    protected SuperMemoRegistry() { }

    #endregion




    #region Properties Impl - Public

    public IElementRegistry   Element   => ElementRegistry.Instance;
    public IComponentRegistry Component => ComponentRegistry.Instance;
    public IConceptRegistry   Concept   => ConceptRegistry.Instance;
    public ITextRegistry      Text      => TextRegistry.Instance;
    public IImageRegistry     Image     => ImageRegistry.Instance;
    public ITemplateRegistry  Template  => TemplateRegistry.Instance;
    public ISoundRegistry     Sound     => SoundRegistry.Instance;
    public IVideoRegistry     Video     => VideoRegistry.Instance;

    #endregion
  }
}
