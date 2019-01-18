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
// Created On:   2019/01/16 14:54
// Modified On:  2019/01/16 17:28
// Modified By:  Alexis

#endregion




using System.Drawing;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Interop.SuperMemo.Content.Layout.Legacy.Grid;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Layout.Legacy.Builders
{
  public static class LayoutBuilder
  {
    #region Constants & Statics

    private const string ComponentsSkeleton = @"ComponentNo={0}
{1}";
    private static readonly Rectangle CorsFull = new Rectangle(100,
                                                               100,
                                                               9780,
                                                               9600);

    #endregion




    #region Methods

    public static LayoutBase Stack(bool            orientation,
                                   ContentTypeFlag acceptedContent,
                                   Rectangle?      rootCors = null)
    {
      return null;
    }

    public static LayoutGrid Grid(ContentTypeFlag acceptedContent,
                                  Rectangle?      rootCors = null)
    {
      return new LayoutGrid(acceptedContent,
                            rootCors ?? CorsFull);
    }

    public static LayoutBase Auto(Rectangle? rootCors = null)
    {
      return null;
    }

    #endregion
  }
}
