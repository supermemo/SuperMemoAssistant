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
// Modified On:  2020/03/04 16:03
// Modified By:  Alexis

#endregion




using System;
using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace SuperMemoAssistant.Extensions
{
  public static class TextBoxEx
  {
    #region Methods

    public static string[] SplitLines(this TextBox tb)
    {
      return tb.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
    }

    public static string[] SplitLines(this TextBox tb, out int count)
    {
      var res = tb.SplitLines();
      count = res.Length;

      return res;
    }

    public static bool IsScrolledToEnd(this TextBox tb)
    {
      return tb.VerticalOffset > tb.ExtentHeight - tb.ViewportHeight - tb.FontSize;
    }

    #endregion
  }
}
