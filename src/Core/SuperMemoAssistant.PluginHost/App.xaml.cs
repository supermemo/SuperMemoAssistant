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
// Created On:   2019/01/19 06:01
// Modified On:  2019/01/25 23:42
// Modified By:  Alexis

#endregion




using System.Linq;
using System.Windows;

namespace SuperMemoAssistant.PluginHost
{
  /// <summary>Interaction logic for App.xaml</summary>
  public partial class App : Application
  {
    #region Methods

    private void Application_Startup(object           sender,
                                     StartupEventArgs e)
    {
      if (ReadArgs(e.Args, out var assemblyName, out var parentPId) == false)
        return;

      // Monitor parentPID
      // new IpcChannel(null, null, null)
      // Request assemblies for assemblyName
      // AppDomain
      // AssemblyLoader
      // Plugin loader
    }

    private bool ReadArgs(string[]   args,
                          out string assemblyName,
                          out int    parentPId)
    {
      assemblyName = null;
      parentPId    = -1;

      if (args.Length != 2 || args.Any(string.IsNullOrWhiteSpace))
        return false;

      assemblyName = args[0];

      return int.TryParse(args[1], out parentPId);
    }

    #endregion
  }
}
