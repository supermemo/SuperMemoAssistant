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
// Modified On:  2018/11/26 00:03
// Modified By:  Alexis

#endregion




using Process.NET;
using SuperMemoAssistant.Interop.SuperMemo.Components.Types;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Types;
using SuperMemoAssistant.Interop.SuperMemo.UI.Element;
using SuperMemoAssistant.Interop.SuperMemo.UI.ElementData;
using SuperMemoAssistant.Interop.SuperMemo.UI.MainBar;

namespace SuperMemoAssistant.Interop.SuperMemo
{
  public interface ISuperMemo
  {
    SMAppVersion AppVersion { get; }
    SMCollection Collection { get; }
    IProcess     SMProcess  { get; }

    bool IgnoreUserConfirmation { get; set; }

    ISuperMemoRegistry Registry { get; }
    ISuperMemoUI       UI       { get; }
  }

  public interface ISuperMemoRegistry
  {
    IElementRegistry   Element   { get; }
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
    IMainBarWdw     MainBarWindow     { get; }
    IElementWdw     ElementWindow     { get; }
    IElementDataWdw ElementDataWindow { get; }
  }
}
