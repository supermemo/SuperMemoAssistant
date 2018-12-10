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
// Created On:   2018/06/21 12:26
// Modified On:  2018/11/29 19:23
// Modified By:  Alexis

#endregion




using System;
using System.Threading;
using Anotar.Serilog;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Exceptions;
using mshtml;
using SuperMemoAssistant.COM.InternetExplorer;
using SuperMemoAssistant.Interop.SuperMemo.Components.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Components.Models;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.SuperMemo.SuperMemo17.UI.Element;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Components.Controls
{
  public class ControlHtml : ControlTextBased, IControlHtml
  {
    #region Properties & Fields - Non-Public

    private readonly int            _nativeControlAddr;
    private          IHTMLDocument2 _document;

    #endregion




    #region Constructors

    /// <inheritdoc />
    public ControlHtml(int          id,
                       ControlGroup @group,
                       int          nativeControlAddr)
      : base(id,
             ComponentType.Html,
             @group)
    {
      _nativeControlAddr = nativeControlAddr;
    }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public IHTMLDocument2 Document => _document ?? (_document = GetDocument());

    public override string Text
    {
      get => Document.body.innerHTML;
      set
      {
        Document.body.innerHTML = value;
        ElementWdw.Instance.SetText(this,
                                    value);
      }
    }

    #endregion




    #region Methods

    private IHTMLDocument2 GetDocument()
    {
      AutomationElement ieSrvFrame = null;
      DateTime          start      = DateTime.Now;

      while (ieSrvFrame == null && (DateTime.Now - start).TotalMilliseconds <= 1000)
        try
        {
          IntPtr shellEmbedHwnd = _group._smProcess.Memory.Read<IntPtr>(
            new IntPtr(_nativeControlAddr + SMNatives.TControl.HandleOffset)
          );

          ieSrvFrame = Svc.UIAutomation.FromHandle(shellEmbedHwnd).FindFirstDescendant(c => c.ByClassName("Internet Explorer_Server"));
        }
        catch (ElementNotAvailableException)
        {
          Thread.Sleep(50);
        }
        catch (TimeoutException)
        {
          Thread.Sleep(50);
        }
        catch (Exception ex)
        {
          LogTo.Error(ex,
                      "Failed to acquire IHTMLDocument2's ShellEmbed handle");
          break;
        }

      if (ieSrvFrame == null)
        return null;

      IntPtr hwnd = ieSrvFrame.FrameworkAutomationElement.NativeWindowHandle.Value;

      return IEComHelper.GetDocumentFromHwnd(hwnd);
    }

    #endregion
  }
}
