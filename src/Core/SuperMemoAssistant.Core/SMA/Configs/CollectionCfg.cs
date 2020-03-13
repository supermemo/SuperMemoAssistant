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
// Created On:   2019/04/16 14:03
// Modified On:  2019/04/19 14:17
// Modified By:  Alexis

#endregion




using System.ComponentModel;
using Forge.Forms.Annotations;
using SuperMemoAssistant.Sys.ComponentModel;

namespace SuperMemoAssistant.SMA.Configs
{
  [Form(Mode = DefaultFields.None)]
  public class CollectionCfg : INotifyPropertyChangedEx
  {
    #region Properties & Fields - Public

    [Field(Name = "Collapse Element window Title bar")]
    public bool CollapseElementWdwTitleBar { get; set; }

    [Field(Name = "Collapse KT window Title bar")]
    public bool CollapseKnowledgeTreeWdwTitleBar { get; set; }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public bool IsChanged { get; set; }

    #endregion




    #region Events

    /// <inheritdoc />
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
