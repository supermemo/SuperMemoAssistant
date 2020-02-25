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
// Modified On:  2020/02/21 19:57
// Modified By:  Alexis

#endregion




using System.Windows.Input;
using Anotar.Serilog;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.IO.Keyboard;
using SuperMemoAssistant.Services.Sentry;
using SuperMemoAssistant.Sys.IO.Devices;

namespace SuperMemoAssistant.Plugins.DevSandbox
{
  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global
  public class DevSandboxPlugin : SentrySMAPluginBase<DevSandboxPlugin>
  {
    #region Constructors

    public DevSandboxPlugin() : base("https://a63c3dad9552434598dae869d2026696@sentry.io/1362046") { }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "DevSandbox";

    public override bool HasSettings => false;

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    protected override void PluginInit()
    {
      Svc.HotKeyManager
         .RegisterGlobal(
           "TestSomething",
           "TestSomething",
           HotKeyScope.SM,
           new HotKey(Key.D1, KeyModifiers.CtrlAlt),
           TestSomething
         )
         .RegisterGlobal(
           "TestAnotherThing",
           "TestAnotherThing",
           HotKeyScope.SM,
           new HotKey(Key.D1, KeyModifiers.CtrlAltShift),
           TestAnotherThing
         );
    }

    /// <inheritdoc />
    public override void ShowSettings() { }

    #endregion




    #region Methods

    public void TestSomething()
    {
      var elId = Svc.SM.UI.ElementWdw.GenerateCloze();
      
      LogTo.Debug($"GenerateCloze: {elId}");
    }

    public void TestAnotherThing()
    {
      var elId = Svc.SM.UI.ElementWdw.GenerateExtract(ElementType.Topic);

      LogTo.Debug($"GenerateExtract: {elId}");
    }

    #endregion
  }
}
