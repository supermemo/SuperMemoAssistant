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
// Created On:   2019/04/14 00:37
// Modified On:  2019/04/14 00:39
// Modified By:  Alexis

#endregion




using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace SuperMemoAssistant.Extensions
{
  public static class ButtonEx
  {
    #region Methods

    public static bool SimulateClick(this Button btn)
    {
      ButtonAutomationPeer peer       = new ButtonAutomationPeer(btn);
      IInvokeProvider      invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;

      if (invokeProv == null)
        return false;

      try
      {
        invokeProv.Invoke();

        return true;
      }
      catch
      {
        return false;
      }
    }

    #endregion
  }
}
