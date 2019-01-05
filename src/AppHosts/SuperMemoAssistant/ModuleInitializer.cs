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
// Created On:   2018/05/08 16:29
// Modified On:  2018/12/30 14:32
// Modified By:  Alexis

#endregion




using System;
using SuperMemoAssistant.Plugins;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.Configuration;
using SuperMemoAssistant.Services.IO;
using SuperMemoAssistant.SuperMemo;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Components;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Elements;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Types;
using SuperMemoAssistant.SuperMemo.SuperMemo17.UI.Element;
using SuperMemoAssistant.Sys;

namespace SuperMemoAssistant
{
  public static class ModuleInitializer
  {
    #region Constants & Statics

    public static IDisposable SentryInstance { get; private set; }

    #endregion




    #region Methods

    public static void Initialize()
    {
      //InitOnLoad.Initialize();

      Svc<CorePlugin>.Plugin        = new CorePlugin();
      Svc<CorePlugin>.Configuration = new ConfigurationService(Svc<CorePlugin>.Plugin);

      // ReSharper disable once NotAccessedVariable
      // ReSharper disable once JoinDeclarationAndInitializer
      object tmp;
      tmp = ComponentRegistry.Instance;
      tmp = ElementRegistry.Instance;
      tmp = BinaryRegistry.Instance;
      tmp = ConceptRegistry.Instance;
      tmp = ImageRegistry.Instance;
      tmp = SoundRegistry.Instance;
      tmp = TemplateRegistry.Instance;
      tmp = TextRegistry.Instance;
      tmp = VideoRegistry.Instance;
      tmp = ElementWdw.Instance;
      tmp = PluginMgr.Instance;
      tmp = SMA.Instance;

      SentryInstance = Services.Sentry.Initialize();
      Logger.Instance.Initialize();
    }

    #endregion
  }
}
