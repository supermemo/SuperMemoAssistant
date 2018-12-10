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
// Created On:   2018/09/04 19:49
// Modified On:  2018/12/04 14:27
// Modified By:  Alexis

#endregion




using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace SuperMemoAssistant.Sys
{
  public class ClipboardSnapshot : IDisposable
  {
    #region Properties & Fields - Non-Public

    private IDataObject Content { get; set; }

    #endregion




    #region Constructors

    public ClipboardSnapshot()
    {
      Content = Clipboard.GetDataObject();
    }

    /// <inheritdoc />
    public void Dispose()
    {
      int retries = 5;

      while (retries-- > 0)
        try
        {
          // Race conditions might happen.
          // System.Runtime.InteropServices.COMException (0x800401D0): OpenClipboard Failed (Exception from HRESULT: 0x800401D0 (CLIPBRD_E_CANT_OPEN))
          Clipboard.SetDataObject(Content);
          break;
        }
        catch (COMException)
        {
          Thread.Sleep(10);
        }
    }

    #endregion
  }
}
