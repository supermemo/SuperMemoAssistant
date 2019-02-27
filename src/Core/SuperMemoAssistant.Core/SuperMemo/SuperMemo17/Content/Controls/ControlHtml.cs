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
// Modified On:  2019/01/19 04:39
// Modified By:  Alexis

#endregion




using System;
using System.ComponentModel;
using System.Threading;
using Anotar.Serilog;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Exceptions;
using FlaUI.UIA3;
using mshtml;
using Process.NET.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.SuperMemo.SuperMemo17.UI.Element;
using SuperMemoAssistant.Sys.COM.InternetExplorer;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Content.Controls
{
  public class ControlHtml : ControlTextBased, IControlHtml
  {
    #region Constants & Statics

    private static UIA3Automation UIAutomation { get; } = new UIA3Automation();

    #endregion




    #region Properties & Fields - Non-Public

    private IHTMLDocument2 _document;

    private int NativeControlAddr =>
      _group._smProcess.Memory.Read<int>(SM17Natives.TElWind.ObjectsPtr,
                                         4 * Id);

    #endregion




    #region Constructors

    /// <inheritdoc />
    public ControlHtml(int          id,
                       ControlGroup @group)
      : base(id,
             ComponentType.Html,
             @group) { }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public IHTMLDocument2 Document => _document ?? (_document = GetDocument());

    public override string Text
    {
      get
      {
        try
        {
          return Document?.body.innerHTML;
        }
        catch
        {
          return null;
        }
      }
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
            new IntPtr(NativeControlAddr + SM17Natives.TControl.HandleOffset)
          );

          ieSrvFrame = UIAutomation.FromHandle(shellEmbedHwnd).FindFirstDescendant(c => c.ByClassName("Internet Explorer_Server"));
        }
        catch (Exception ex)
          when (ex is ElementNotAvailableException
            || ex is TimeoutException
            || ex is Win32Exception)
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
      {
        LogTo.Warning("IHTMLDocument2 ControlHtml.GetDocument() failed to get ieSrvFrame.");

        return null;
      }

      IntPtr hwnd = ieSrvFrame.FrameworkAutomationElement.NativeWindowHandle.Value;

      return IEComHelper.GetDocumentFromHwnd(hwnd);
    }

    #endregion
  }
}
