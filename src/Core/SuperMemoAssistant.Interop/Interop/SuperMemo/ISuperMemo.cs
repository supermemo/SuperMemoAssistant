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
// Modified On:  2020/01/13 20:38
// Modified By:  Alexis

#endregion




using System;
using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Types;
using SuperMemoAssistant.Interop.SuperMemo.UI.Element;

namespace SuperMemoAssistant.Interop.SuperMemo
{
  public interface ISuperMemo
  {
    Version      AppVersion { get; }
    SMCollection Collection { get; }

    int ProcessId { get; }

    bool IgnoreUserConfirmation { get; set; }

    ISuperMemoRegistry Registry { get; }
    ISuperMemoUI       UI       { get; }
  }

  public interface ISuperMemoRegistry
  {
    IElementRegistry   Element   { get; }
    IBinaryRegistry    Binary    { get; }
    IComponentRegistry Component { get; }
    IConceptRegistry   Concept   { get; }
    ITextRegistry      Text      { get; }
    IImageRegistry     Image     { get; }
    ISoundRegistry     Sound     { get; }
    IVideoRegistry     Video     { get; }
    ITemplateRegistry  Template  { get; }
  }

  public interface ISuperMemoUI
  {
    IElementWdw ElementWdw { get; }
  }
}
