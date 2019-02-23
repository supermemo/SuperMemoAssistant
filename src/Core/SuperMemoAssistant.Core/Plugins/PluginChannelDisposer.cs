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
// Created On:   2019/02/21 14:22
// Modified On:  2019/02/22 23:19
// Modified By:  Alexis

#endregion




using System;
using SuperMemoAssistant.Sys;

namespace SuperMemoAssistant.Plugins
{
  public class PluginChannelDisposer : SMMarshalByRefObject, IDisposable
  {
    #region Properties & Fields - Non-Public

    private readonly string _assemblyName;

    private readonly string        _interfaceType;
    private readonly PluginManager _pm;

    #endregion




    #region Constructors

    /// <inheritdoc />
    public PluginChannelDisposer(PluginManager pm,
                                 string        interfaceType,
                                 string        assemblyName)
    {
      _pm            = pm;
      _interfaceType = interfaceType;
      _assemblyName  = assemblyName;
    }

    /// <inheritdoc />
    public void Dispose()
    {
      _pm.UnregisterChannelType(_interfaceType, _assemblyName);
    }

    #endregion
  }
}
