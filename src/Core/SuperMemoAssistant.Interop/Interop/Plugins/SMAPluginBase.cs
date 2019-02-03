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
// Created On:   2018/07/27 12:55
// Modified On:  2019/01/26 06:13
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Diagnostics;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.Configuration;
using SuperMemoAssistant.Sys;
using SuperMemoAssistant.Sys.ComponentModel;

namespace SuperMemoAssistant.Interop.Plugins
{
  public abstract class SMAPluginBase<TPlugin> : SMMarshalByRefObject, ISMAPlugin
    where TPlugin : SMAPluginBase<TPlugin>
  {
    #region Constructors

    protected SMAPluginBase(bool attachDebuggerInDebugConfiguration = true,
                            bool forceAttachDebbuger                = false)
    {
      if (forceAttachDebbuger)
        Debugger.Launch();

      else if (attachDebuggerInDebugConfiguration)
        AttachDebugger();

      Init();
    }

    /// <inheritdoc />
    public virtual void Dispose() { }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public Guid Id => AssemblyEx.GetAssemblyGuid(GetType());
    /// <inheritdoc />
    public string AssemblyName => AssemblyEx.GetAssemblyName(GetType());
    /// <inheritdoc />
    public string AssemblyVersion => AssemblyEx.GetAssemblyVersion(GetType());
    /// <inheritdoc />
    public virtual List<INotifyPropertyChangedEx> SettingsModels { get; protected set; }

    #endregion




    #region Methods Impl

    /// <param name="cfgObject"></param>
    /// <inheritdoc />
    public virtual void SettingsSaved(object cfgObject) { }

    #endregion




    #region Methods

    [Conditional("DEBUG")]
    [Conditional("DEBUG_IN_PROD")]
    private void AttachDebugger()
    {
      Debugger.Launch();
    }

    private void Init()
    {
      Svc<TPlugin>.Plugin = (TPlugin)this;
      Svc<TPlugin>.Configuration = new ConfigurationService(this);

      OnInit();
    }

    #endregion




    #region Methods Abs

    protected abstract void OnInit();

    /// <inheritdoc />
    public abstract string Name { get; }

    #endregion
  }
}
