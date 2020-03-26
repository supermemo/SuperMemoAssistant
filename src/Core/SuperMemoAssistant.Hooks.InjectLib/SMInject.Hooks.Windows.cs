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
// Modified On:  2020/03/13 13:29
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using EasyHook;

namespace SuperMemoAssistant.Hooks.InjectLib
{
  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class SMInject
  {
    #region Constants & Statics

    public const int SwShowNormal = 1;

    private LocalHook _showWindowHook;

    #endregion




    #region Methods

    //
    // Setup windows related hooks

    /// <summary>
    /// Creates the hooks
    /// </summary>
    /// <returns>Created hooks to be unloaded at shutdown</returns>
    private IEnumerable<LocalHook> InstallWindowHooks()
    {
      _showWindowHook = LocalHook.Create(
        LocalHook.GetProcAddress("user32.dll", "ShowWindow"),
        new Win32.CreateShowWindowDlg(ShowWindow_Hooked),
        this
      );
      _showWindowHook.ThreadACL.SetExclusiveACL(new[] { 0 });

      return Array.Empty<LocalHook>();
    }

    /// <summary>
    /// Cancel SuperMemo's splash screen then unload the hook
    /// </summary>
    /// <param name="inHwnd">The window handle</param>
    /// <param name="inNCmdShow">The display mode</param>
    /// <returns></returns>
    private bool ShowWindow_Hooked(IntPtr inHwnd, int inNCmdShow)
    {
      if (inNCmdShow == SwShowNormal)
      {
        _showWindowHook.Dispose();
        _showWindowHook = null;

        return true;
      }

      return Win32.ShowWindow(inHwnd, inNCmdShow);
    }

    #endregion
  }
}
