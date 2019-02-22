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
// Created On:   2019/02/13 13:55
// Modified On:  2019/02/21 15:44
// Modified By:  Alexis

#endregion




using System.Collections.Concurrent;
using SuperMemoAssistant.Interop.Plugins;
using SysProcess = System.Diagnostics.Process;

namespace SuperMemoAssistant.Plugins
{
  /// <summary>Represents a running instance of a plugin process</summary>
  public class PluginInstance
  {
    #region Properties & Fields - Non-Public

    private SysProcess _process = null;

    #endregion




    #region Constructors

    public PluginInstance(ISMAPlugin     plugin,
                          PluginMetadata metadata,
                          int            processId)
    {
      Plugin    = plugin;
      Metadata  = metadata;
      ProcessId = processId;
    }

    #endregion




    #region Properties & Fields - Public

    public ISMAPlugin     Plugin      { get; }
    public PluginMetadata Metadata    { get; }
    public int            ProcessId   { get; }
    public bool           ExitHandled { get; set; }
    public bool           IsStopping  { get; set; }

    public SysProcess                           Process             => _process ?? (_process = SysProcess.GetProcessById(ProcessId));
    public ConcurrentDictionary<string, string> InterfaceChannelMap { get; } = new ConcurrentDictionary<string, string>();

    #endregion
  }
}
