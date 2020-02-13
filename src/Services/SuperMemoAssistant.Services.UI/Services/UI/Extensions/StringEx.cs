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
// Modified On:  2020/02/03 10:19
// Modified By:  Alexis

#endregion




using System.Threading.Tasks;
using Forge.Forms;

namespace SuperMemoAssistant.Services.UI.Extensions
{
  public static class StringEx
  {
    #region Methods

    public static Task ErrorMsgBox(this string msg)
    {
      return Show.Window().For(new Alert(msg, "Error"));
    }

    public static Task WarningMsgBox(this string msg)
    {
      return Show.Window().For(new Alert(msg, "Warning"));
    }

    public static Task InfoMsgBox(this string msg)
    {
      return Show.Window().For(new Alert(msg, "Information"));
    }

    public static Task MsgBox(this string msg, string title)
    {
      return Show.Window().For(new Alert(msg, title));
    }

    #endregion
  }
}
