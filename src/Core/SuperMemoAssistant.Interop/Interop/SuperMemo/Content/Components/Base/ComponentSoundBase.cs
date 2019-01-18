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
// Created On:   2019/01/16 17:37
// Modified On:  2019/01/16 17:39
// Modified By:  Alexis

#endregion




using System.Drawing;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Components.Base
{
  public abstract class ComponentSoundBase : IComponentSound
  {
    #region Properties Impl - Public

    public Size MinSize { get; } = new Size(100,
                                            100);

    #endregion




    #region Methods Abs

    public abstract short          Left          { get; }
    public abstract short          Top           { get; }
    public abstract short          Width         { get; }
    public abstract short          Height        { get; }
    public abstract AtFlags        DisplayAt     { get; }
    public abstract ISound         Sound         { get; }
    public abstract AtFlags        PlayAt        { get; }
    public abstract uint           ExtractStart  { get; }
    public abstract uint           ExtractStop   { get; }
    public abstract bool           IsContinuous  { get; }
    public abstract MediaPanelType Panel         { get; }
    public abstract Color          Color         { get; }
    public abstract TextAlignment  TextAlignment { get; }

    #endregion
  }
}
