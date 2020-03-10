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
// Modified On:  2020/03/05 13:51
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Diagnostics;
using PluginManager.Interop.Contracts;
using PluginManager.Interop.PluginHost;
using SuperMemoAssistant.Interop.SuperMemo;

namespace SuperMemoAssistant.Interop.Plugins
{
  public class PluginHost : PluginHostBase<ISuperMemoAssistant>
  {
    #region Properties & Fields - Non-Public

    protected override HashSet<Type> CoreInterfaceTypes { get; } = new HashSet<Type>
    {
      typeof(ISuperMemoAssistant)
      // Insert subsequent versions here
    };

    protected override HashSet<Type> PluginMgrInterfaceTypes { get; } = new HashSet<Type>
    {
      typeof(IPluginManager<ISuperMemoAssistant>)
      // Insert subsequent versions here
    };

    #endregion




    #region Constructors

    public PluginHost(
      string  pluginPackageName,
      Guid    sessionGuid,
      string  smaChannelName,
      Process smaProcess,
      bool    isDev)
      : base(pluginPackageName, sessionGuid, smaChannelName, smaProcess, isDev) { }

    #endregion




    #region Methods Impl

    protected override void MonitorPluginMgrProcess(object param)
    {
      base.MonitorPluginMgrProcess(param);
    }

    #endregion
  }
}
