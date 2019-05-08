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
// Created On:   2018/05/08 02:16
// Modified On:  2018/12/10 12:52
// Modified By:  Alexis

#endregion




using System;
using System.Runtime.InteropServices;
using mshtml;

// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.Sys.COM.InternetExplorer
{
  public static class IEComHelper
  {
    #region Methods

    public static IHTMLDocument2 GetDocumentFromHwnd(IntPtr hWnd)
    {
      IHTMLDocument2 document = null;
      int            lngMsg   = RegisterWindowMessage("WM_HTML_GETOBJECT");

      if (lngMsg != 0)
      {
        SendMessageTimeout(hWnd,
                           lngMsg,
                           0,
                           0,
                           SMTO_ABORTIFHUNG,
                           1000,
                           out int lRes);

        if (lRes != 0)
          ObjectFromLresult(lRes,
                            ref GUID_IHTMLDocument,
                            0,
                            ref document);
      }

      return document;
    }

    #endregion




    #region API CALLS

    [DllImport("user32.dll",
      EntryPoint = "RegisterWindowMessageA")]
    public static extern int RegisterWindowMessage(string lpString);

    [DllImport("user32.dll",
      EntryPoint = "SendMessageTimeoutA")]
    public static extern int SendMessageTimeout(IntPtr  hwnd,
                                                int     msg,
                                                int     wParam,
                                                int     lParam,
                                                int     fuFlags,
                                                int     uTimeout,
                                                out int lpdwResult);

    [DllImport("OLEACC.dll")]
    public static extern int ObjectFromLresult(int                lResult,
                                               ref Guid           riid,
                                               int                wParam,
                                               ref IHTMLDocument2 ppvObject);


    public const  int  SMTO_ABORTIFHUNG   = 0x2;
    public static Guid GUID_IHTMLDocument = new Guid("626FC520-A41E-11CF-A731-00A0C9082637");

    #endregion
  }
}
