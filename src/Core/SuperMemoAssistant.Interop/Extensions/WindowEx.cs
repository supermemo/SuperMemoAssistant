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
// Created On:   2018/10/26 19:53
// Modified On:  2018/10/26 19:54
// Modified By:  Alexis

#endregion




using System.Linq;
using System.Windows;

namespace SuperMemoAssistant.Extensions
{
  public static class WindowEx
  {
    #region Methods

    public static void ShowAndActivate(this Window wdw)
    {
      wdw.Show();
      wdw.ForceActivate();
    }

    public static bool IsWindowOpen<T>(string name = "") where T : Window
    {
      return string.IsNullOrEmpty(name)
        ? Application.Current.Windows.OfType<T>().Any()
        : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
    }

    public static void ForceActivate(this Window wdw)
    {
      if (wdw.WindowState == WindowState.Minimized)
      {
        wdw.WindowState = WindowState.Normal;
      }

      wdw.Activate();
      wdw.Topmost = true;  // important
      wdw.Topmost = false; // important
      wdw.Focus();         // important
    }

    #endregion
  }
}
