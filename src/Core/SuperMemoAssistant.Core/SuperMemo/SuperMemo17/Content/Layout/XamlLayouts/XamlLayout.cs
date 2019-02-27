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
// Created On:   2019/02/26 23:19
// Modified On:  2019/02/27 13:39
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using Newtonsoft.Json;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Content.Layout.XamlControls;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Content.Layout.XamlLayouts
{
  [Serializable]
  public class XamlLayout : INotifyPropertyChanged
  {
    #region Constructors

    public XamlLayout() { }

    public XamlLayout(string name, string xaml, bool isBuiltIn = false, bool autoValidationSuspended = false)
    {
      IsBuiltIn               = isBuiltIn;
      AutoValidationSuspended = autoValidationSuspended;

      Name      = name;
      Xaml      = xaml;
      IsBuiltIn = isBuiltIn;
    }

    public XamlLayout(XamlLayout other, string newName = null, bool isBuiltIn = false, bool autoValidationSuspended = false)
    {
      IsBuiltIn               = isBuiltIn;
      AutoValidationSuspended = autoValidationSuspended;

      Name    = newName ?? other.Name;
      Xaml    = other.Xaml;
      IsValid = other.IsValid;
    }

    #endregion




    #region Properties & Fields - Public

    public string Name { get; set; }
    public string Xaml { get; set; }

    [JsonIgnore]
    public bool IsBuiltIn { get; }
    
    [JsonIgnore]
    public bool IsDefault => ReferenceEquals(LayoutManager.Instance.Default, this);

    [JsonIgnore]
    public ContentTypeFlag AcceptedContent { get; private set; }

    [JsonIgnore]
    public bool IsValid { get; private set; }

    [JsonIgnore]
    public bool AutoValidationSuspended { get; set; }

    [JsonIgnore]
    public bool IsValidationRequired { get; private set; }

    #endregion




    #region Methods

    public string Build(List<ContentBase> contents)
    {
      return Application.Current.Dispatcher.Invoke(
        () =>
        {
          var ctrlGroup = new XamlControlGroup(this);

          return ctrlGroup.ToElementDefinition(contents);
        }
      );
    }

    public DependencyObject ParseLayout()
    {
      return ParseLayout(Xaml);
    }

    public static DependencyObject ParseLayout(string xaml)
    {
      var context = new ParserContext { XamlTypeMapper = new XamlTypeMapper(new string[] { }) };

      var @namespace   = typeof(XamlControlBase).Namespace;
      var assemblyName = typeof(XamlControlBase).Assembly.GetName().Name;

      context.XmlnsDictionary.Add("sma", $"clr-namespace:{@namespace}");
      // ReSharper disable once AssignNullToNotNullAttribute
      context.XamlTypeMapper.AddMappingProcessingInstruction($"clr-namespace:{@namespace}", @namespace, assemblyName);

      return XamlReader.Parse(xaml, context) as DependencyObject;
    }

    public void ValidateXaml()
    {
      if (IsValidationRequired == false)
        return;

      Application.Current.Dispatcher.Invoke(() =>
      {
        var ctrlGroup = new XamlControlGroup(this);

        IsValid         = ctrlGroup.IsValid;
        AcceptedContent = ctrlGroup.AcceptedContent;
      });

      IsValidationRequired = false;
    }

    public void OnXamlChanged()
    {
      IsValidationRequired = true;

      if (AutoValidationSuspended)
        return;

      ValidateXaml();
    }

    public void CopyFrom(XamlLayout other, bool copyValidation = true)
    {
      var restore = AutoValidationSuspended;
      AutoValidationSuspended = copyValidation;

      Xaml = other.Xaml;

      if (copyValidation)
        IsValid = other.IsValid;

      AutoValidationSuspended = restore;
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
