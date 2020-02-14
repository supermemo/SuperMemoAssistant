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
// Created On:   2020/01/23 08:17
// Modified On:  2020/02/13 21:34
// Modified By:  Alexis

#endregion




using System;
using SuperMemoAssistant.Sys.Remoting;

namespace SuperMemoAssistant.Interop.Plugins
{
  public interface ISMAPlugin : IDisposable
  {
    string Name            { get; }
    string AssemblyName    { get; }
    string AssemblyVersion { get; }
    string ChannelName     { get; }
    bool   HasSettings     { get; }

    void               OnInjected();
    void               OnServicePublished(string interfaceTypeName);
    void               OnServiceRevoked(string   interfaceTypeName);
    RemoteTask<object> OnMessage(int             msg, params object[] parameters);
    void               ShowSettings();
  }
}
