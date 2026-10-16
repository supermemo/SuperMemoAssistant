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
// Modified On:  2019/08/08 11:19
// Modified By:  Alexis

#endregion




using PluginManager.Interop.Sys;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
using SuperMemoAssistant.Interop.SuperMemo.Elements;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Types;
using SuperMemoAssistant.SuperMemo.Common.Content;
using SuperMemoAssistant.SuperMemo.Common.Elements;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Elements;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Types;

namespace SuperMemoAssistant.SuperMemo
{
  public class SuperMemoRegistryCore : SuperMemoRegistry
  {
    #region Constructors

    public SuperMemoRegistryCore()
    {
      base.Element   = Element   = new ElementRegistry17();
      base.Binary    = Binary    = new BinaryRegistry17();
      base.Component = Component = new ComponentRegistry();
      base.Concept   = Concept   = new ConceptRegistry17();
      base.Text      = Text      = new TextRegistry17();
      base.Comment   = Comment   = new CommentRegistry17();
      base.Image     = Image     = new ImageRegistry17();
      base.Template  = Template  = new TemplateRegistry17();
      base.Sound     = Sound     = new SoundRegistry17();
      base.Video     = Video     = new VideoRegistry17();
    }

    #endregion




    #region Properties & Fields - Public

    public new ElementRegistryBase Element   { get; }
    public new BinaryRegistry17    Binary    { get; }
    public new ComponentRegistry   Component { get; }
    public new ConceptRegistry17   Concept   { get; }
    public new TextRegistry17      Text      { get; }
    public new CommentRegistry17   Comment   { get; }
    public new ImageRegistry17     Image     { get; }
    public new TemplateRegistry17  Template  { get; }
    public new SoundRegistry17     Sound     { get; }
    public new VideoRegistry17     Video     { get; }

    #endregion
  }

  public class SuperMemoRegistry : PerpetualMarshalByRefObject, ISuperMemoRegistry
  {
    #region Constructors

    protected SuperMemoRegistry() { }

    #endregion




    #region Properties Impl - Public

    public IElementRegistry   Element   { get; protected set; }
    public IBinaryRegistry    Binary    { get; protected set; }
    public IComponentRegistry Component { get; protected set; }
    public IConceptRegistry   Concept   { get; protected set; }
    public ITextRegistry      Text      { get; protected set; }
    public ICommentRegistry   Comment   { get; protected set; }
    public IImageRegistry     Image     { get; protected set; }
    public ITemplateRegistry  Template  { get; protected set; }
    public ISoundRegistry     Sound     { get; protected set; }
    public IVideoRegistry     Video     { get; protected set; }

    #endregion
  }
}
