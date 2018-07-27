// 
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

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using SuperMemoAssistant.Sys.IO.Devices;
using Keyboard = SuperMemoAssistant.Sys.IO.Devices.Keyboard;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.UI
{
  public class MenuRoot : MenuBase
  {
    #region Constructors

    public MenuRoot(WdwBase parent)
      : base(parent) { }

    #endregion




    #region Properties Impl

    protected override Key Key => throw new InvalidOperationException();

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override Keys GetKeys()
    {
      return SubMenu.GetKeys();
    }

    #endregion




    #region Methods

    public Task<bool> Execute()
    {
      return Keyboard.PostSysKeysAsync(
        Parent.Window.Properties.NativeWindowHandle.ValueOrDefault,
        GetKeys()
      );
    }

    #endregion
  }
}
