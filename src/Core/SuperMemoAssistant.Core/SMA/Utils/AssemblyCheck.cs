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
// Created On:   2020/01/14 02:07
// Modified On:  2020/01/14 02:09
// Modified By:  Alexis

#endregion




using System;
using System.IO;
using System.Windows.Forms;
using Anotar.Serilog;
using mshtml;
using Process.NET.Assembly.Assemblers;

namespace SuperMemoAssistant.SMA.Utils
{
  public static class AssemblyCheck
  {
    #region Methods
    
    public static bool CheckRequired(out string error)
    {
      if (CheckFasm32(out error) == false || CheckMshtml(out error) == false)
        return false;

      return true;
    }

    public static bool CheckFasm32(out string error)
    {
      error = null;

      try
      {
        var unused = new Fasm32Assembler().Assemble("nop");

        return true;
      }
      catch (FileNotFoundException ex)
      {
        error = "Fasm32 assembly not found.";
        LogTo.Warning(ex, error);
        return false;
      }
      catch (Exception ex)
      {
        error = "Exception while checking for Fasm32 assembly.";
        LogTo.Error(ex, error);
        return false;
      }
    }

    public static bool CheckMshtml(out string error)
    {
      error = null;

      try
      {
        using (var wb = new WebBrowser())
        {
          wb.DocumentText = "<html><body></body></html>";

          // ReSharper disable once UnusedVariable
          IHTMLDocument2 doc = (IHTMLDocument2)wb.Document.DomDocument;
        }

        return true;
      }
      catch (FileNotFoundException ex)
      {
        error = "Microsoft.mshtml assembly not found.";
        LogTo.Warning(ex, error);
        return false;
      }
      catch (Exception ex)
      {
        error = "Exception while checking for Microsoft.mshtml assembly.";
        LogTo.Error(ex, error);
        return false;
      }
    }

    #endregion
  }
}
