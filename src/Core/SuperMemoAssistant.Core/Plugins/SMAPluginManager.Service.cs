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
// Modified On:  2020/02/24 14:04
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anotar.Serilog;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Plugins.Models;
using SuperMemoAssistant.Sys.Remoting;

namespace SuperMemoAssistant.Plugins
{
  public partial class SMAPluginManager
  {
    #region Methods

    public async Task<Dictionary<PluginInstance, bool>> OnLoggerConfigUpdated()
    {
      try
      {
        var plugins     = new List<PluginInstance>(RunningPluginMap.Values);
        var remoteTasks = plugins.AsParallel().Select(p => p.Plugin.OnMessage(PluginMessage.OnLoggerConfigUpdated));

        // ReSharper disable once ConstantConditionalAccessQualifier
        var results = (await RemoteTask.WhenAll(remoteTasks))?.Cast<bool>()?.ToList();

        if (results == null)
          return null;

        if (results.Count != plugins.Count)
          throw new InvalidOperationException($"results.Count is {results.Count} while plugins.Count is {plugins.Count}");

        Dictionary<PluginInstance, bool> pluginCallSuccessMap = new Dictionary<PluginInstance, bool>();

        for (int i = 0; i < results.Count; i++)
          pluginCallSuccessMap[plugins[i]] = results[i];

        return pluginCallSuccessMap;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception thrown in OnLoggerConfigUpdated");

        return null;
      }
    }

    #endregion
  }
}
