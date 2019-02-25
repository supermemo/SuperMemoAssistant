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
// Created On:   2019/02/22 19:27
// Modified On:  2019/02/24 20:54
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.PluginHost
{
  public static class HostConst
  {
    #region Constants & Statics

    public const int ExitUnknownError               = -1;
    public const int ExitParameters                 = 1;
    public const int ExitParentExited               = 2;
    public const int ExitIpcConnectionError         = 3;
    public const int ExitCouldNotGetAssembliesPaths = 4;
    public const int ExitNoPluginTypeFound          = 5;
    public const int ExitCouldNotConnectPlugin      = 6;


    public const string AppDomainName          = "PluginsHost_AppDomain";
    public const string PluginHostAssemblyName = "SuperMemoAssistant.Interop";
    public const string PluginHostTypeName     = "SuperMemoAssistant.Interop.Plugins.PluginHost";

    #endregion
  }
}
