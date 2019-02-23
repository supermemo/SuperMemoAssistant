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
// Created On:   2019/01/26 04:22
// Modified On:  2019/01/26 06:06
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using SuperMemoAssistant.Interop.SuperMemo;

namespace SuperMemoAssistant.Interop.Plugins
{
  public interface ISMAPluginManager
  {
    /// <summary>
    ///   Request a list of assemblies to load plugin <paramref name="pluginId" /> and its
    ///   dependencies.
    /// </summary>
    /// <param name="pluginId">The plugin assembly name</param>
    /// <param name="pluginAssemblies">
    ///   The list of paths representing plugin's referenced assemblies,
    ///   or null.
    /// </param>
    /// <param name="dependenciesAssemblies">
    ///   The list of paths representing plugin's dependencies
    ///   referenced assemblies, or null.
    /// </param>
    /// <returns><see langword="true" /> if successfull, <see langword="false" /> otherwise</returns>
    bool GetAssembliesPathsForPlugin(string                  pluginId,
                                     out IEnumerable<string> pluginAssemblies,
                                     out IEnumerable<string> dependenciesAssemblies);

    /// <summary>Registers a newly started plugin process with SMA</summary>
    /// <param name="channel"></param>
    /// <param name="processId">Plugin's process id</param>
    /// <returns>
    ///   An instance of <see cref="ISuperMemoAssistant" /> if successfull,
    ///   <see langword="null" /> otherwise
    /// </returns>
    ISuperMemoAssistant ConnectPlugin(string channel,
                                      int    processId);

    /// <summary>
    ///   Attempts to retrieve an Ipc Server's channel name for given remote interface. The
    ///   interface must be registered with <see cref="RegisterService" />
    ///   beforehand.
    /// </summary>
    /// <param name="remoteInterfaceType"></param>
    /// <returns>The channel name if successfull, <see langword="null" /> otherwise.</returns>
    string GetService(string remoteInterfaceType);

    /// <summary>
    ///   Registers an Ipc Server's channel name for interface of type
    ///   <paramref name="remoteServiceType" />.
    /// </summary>
    /// <param name="remoteServiceType"></param>
    /// <param name="channelName">
    ///   Channel name where clients can acquire a proxy for
    ///   <paramref name="remoteServiceType" />
    /// </param>
    /// <param name="assemblyName"></param>
    /// <returns>
    ///   A disposable object, which unregisters the channel when disposed, or
    ///   <see langword="null" />.
    /// </returns>
    IDisposable RegisterService(string remoteServiceType,
                                string channelName,
                                string assemblyName);
  }
}
