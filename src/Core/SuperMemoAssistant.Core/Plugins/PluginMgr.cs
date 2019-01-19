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
// Created On:   2018/05/30 12:47
// Modified On:  2018/06/02 12:24
// Modified By:  Alexis

#endregion




using System;
using Anotar.Serilog;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Types;
using SuperMemoAssistant.Interop.SuperMemo.UI.Element;
using SuperMemoAssistant.PluginsHost;
using SuperMemoAssistant.SuperMemo;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Components;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Elements;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Types;
using SuperMemoAssistant.SuperMemo.SuperMemo17.UI.Element;
using SuperMemoAssistant.Sys;
// ReSharper disable RedundantTypeArgumentsOfMethod

namespace SuperMemoAssistant.Plugins
{
  [InitOnLoad]
  public class PluginMgr : IDisposable
  {
    #region Constants & Statics

    public static PluginMgr Instance { get; } = new PluginMgr();

    #endregion




    #region Properties & Fields - Non-Public

    protected AppDomain  RunnerDomain { get; set; }
    protected PluginHost Runner       { get; set; }

    #endregion




    #region Constructors

    protected PluginMgr()
    {
      SMA.Instance.OnSMStartedEvent += OnSMStarted;
      SMA.Instance.OnSMStoppedEvent += OnSMStopped;
    }

    /// <inheritdoc />
    public void Dispose()
    {
      UnloadPlugins();
    }

    #endregion




    #region Methods

    private void OnSMStarted(object sender, SMProcessArgs e)
    {
      LoadPlugins();
    }

    private void OnSMStopped(object sender, SMProcessArgs e)
    {
      UnloadPlugins();
    }

    protected void LoadPlugins()
    {
      (RunnerDomain, Runner) = PluginHost.Create(SMA.Instance.Collection);
      Runner.Setup();
      
      Runner.Export<ISuperMemoAssistant>(SMA.Instance);
      //Runner.Export<ISuperMemoUI>(SMA.Instance.UI);
      //Runner.Export<ISuperMemoRegistry>(SMA.Instance.Registry);

      //Runner.Export<IElementRegistry>(ElementRegistry.Instance);
      //Runner.Export<IComponentRegistry>(ComponentRegistry.Instance);
      //Runner.Export<ITextRegistry>(TextRegistry.Instance);
      //Runner.Export<IBinaryRegistry>(BinaryRegistry.Instance);
      //Runner.Export<IConceptRegistry>(ConceptRegistry.Instance);
      //Runner.Export<IImageRegistry>(ImageRegistry.Instance);
      //Runner.Export<ITemplateRegistry>(TemplateRegistry.Instance);
      //Runner.Export<ISoundRegistry>(SoundRegistry.Instance);
      //Runner.Export<IVideoRegistry>(VideoRegistry.Instance);
      
      //Runner.Export<IElementWdw>(ElementWdw.Instance);

      Runner.PostSetup();

      foreach (var plugin in Runner.Plugins)
        LogTo.Debug($"[PluginMgr] Loaded plugin {plugin.Name} ({plugin.Version})");
    }

    protected void UnloadPlugins()
    {
      Runner?.Dispose();

      if (RunnerDomain != null)
        AppDomain.Unload(RunnerDomain);

      Runner       = null;
      RunnerDomain = null;
    }

    public void ReloadPlugins(bool full = false)
    {
      if (!full)
      {
        Runner.Recompose();
        return;
      }

      UnloadPlugins();
      LoadPlugins();
    }

    #endregion
  }
}
