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
// Created On:   2019/03/02 18:29
// Modified On:  2019/03/02 23:43
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.SMA.Configs;
using SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlLayouts;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Layout
{
  public class LayoutManager
  {
    #region Constants & Statics
    
    public static LayoutManager Instance { get; } = new LayoutManager();

    public static XamlLayout GenericLayout => Instance._layoutMap[GenericLayoutName];
    public const  string     GenericLayoutName = "Generic";

    public static XamlLayout DefaultOrGenericLayout => Instance.Default.IsValid ? Instance.Default : GenericLayout;

    #endregion




    #region Properties & Fields - Non-Public

    private LayoutsCfg _config;

    private Dictionary<string, XamlLayout>   _layoutMap;
    private ObservableCollection<XamlLayout> _layouts;

    #endregion




    #region Constructors

    public LayoutManager()
    {
      Core.SMA.OnSMStartedEvent += OnSMStarted;
    }

    #endregion




    #region Properties & Fields - Public

    public ReadOnlyObservableCollection<XamlLayout> Layouts { get; private set; }
    public XamlLayout                               Default { get; private set; }

    #endregion




    #region Methods

    public void AddLayout(XamlLayout layout, bool replace = false)
    {
      if (_layoutMap.ContainsKey(layout.Name))
      {
        if (replace == false)
          throw new ArgumentException("A layout already exists with that name");

        _layouts.Remove(_layoutMap[layout.Name]);
      }

      _layoutMap[layout.Name] = layout;
      _layouts.Add(layout);

      layout.NameChanged += OnLayoutNameChanged;
    }

    public void DeleteLayout(XamlLayout layout)
    {
      if (_layoutMap.ContainsKey(layout.Name) == false)
        return;

      _layoutMap.Remove(layout.Name);
      _layouts.Remove(layout);

      layout.NameChanged -= OnLayoutNameChanged;

      SetDefault(Default.Name);
    }

    public XamlLayout GetLayout(string layoutName)
    {
      return string.IsNullOrWhiteSpace(layoutName)
        ? null
        : _layoutMap.SafeGet(layoutName);
    }

    public bool LayoutExists(string layoutName)
    {
      return _layoutMap.ContainsKey(layoutName);
    }

    public void SetDefault(string layoutName)
    {
      var prevDefault = Default;

      try
      {
        if (layoutName == null || _layoutMap.ContainsKey(layoutName) == false)
        {
          Default = GenericLayout;
          return;
        }

        Default = _layoutMap[layoutName];
      }
      finally
      {
        if (prevDefault != null)
          prevDefault.IsDefault = false;

        Default.IsDefault = true;
      }
    }

    public void SaveConfig()
    {
      _config.Default = Default.Name;
      _config.Layouts = new List<XamlLayout>(Layouts.Where(l => l.IsBuiltIn == false));

      Core.Configuration.Save(_config).RunAsync();
    }

    private void OnLayoutNameChanged(XamlLayout xamlLayout, string before, string after)
    {
      if (_layoutMap.ContainsKey(before) == false)
        throw new ArgumentException($"Layout name changed event called, but layout can't be found: {before}");

      if (_layoutMap.ContainsKey(after))
        throw new ArgumentException($"A layout already exists with that name: {after}");

      _layoutMap.Remove(before);
      _layoutMap[after] = xamlLayout;
    }

    private async Task OnSMStarted(object sender, Interop.SuperMemo.Core.SMProcessArgs eventArgs)
    {
      _config = await Core.Configuration.Load<LayoutsCfg>() ?? new LayoutsCfg();

      _layouts   = new ObservableCollection<XamlLayout>(_config.Layouts);
      _layoutMap = _layouts.ToDictionary(k => k.Name);

      LoadBuiltInLayouts();

      SetDefault(_config.Default);

      Layouts = new ReadOnlyObservableCollection<XamlLayout>(_layouts);
    }

    private void LoadBuiltInLayouts()
    {
      AddLayout(
        new XamlLayout(GenericLayoutName, LoadXamlFromResource("GenericLayout.xaml"), true),
        true
      );
    }

    private string LoadXamlFromResource(string xamlFileName)
    {
      var assembly   = Assembly.GetExecutingAssembly();
      var @namespace = typeof(XamlLayout).Namespace;

      using (var stream = assembly.GetManifestResourceStream($"{@namespace}.{xamlFileName}"))
      using (var streamReader = new StreamReader(stream ?? throw new ArgumentException(nameof(xamlFileName))))
        return streamReader.ReadToEnd();
    }

    #endregion
  }
}
