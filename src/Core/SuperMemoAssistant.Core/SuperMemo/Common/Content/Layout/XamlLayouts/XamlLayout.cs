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
// Modified On:  2019/03/03 15:38
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using Newtonsoft.Json;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlControls;
using SuperMemoAssistant.Sys.ComponentModel;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlLayouts
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

      Name = name;
      Xaml = xaml;
    }

    public XamlLayout(XamlLayout other, string newName = null, bool isBuiltIn = false, bool autoValidationSuspended = false)
    {
      IsBuiltIn               = isBuiltIn;
      AutoValidationSuspended = autoValidationSuspended;

      Name    = newName ?? other.Name;
      IsValid = other.IsValid;
      Xaml    = other.Xaml;
    }

    #endregion




    #region Properties & Fields - Public

    public string Name { get; set; }
    public string Xaml { get; set; }

    [JsonIgnore]
    public bool IsBuiltIn { get; }

    [JsonIgnore]
    public bool IsDefault { get; set; }

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

    public DependencyObject ParseLayout(out Exception xamlParsingException)
    {
      return ParseLayout(Xaml, out xamlParsingException);
    }

    public static DependencyObject ParseLayout(string xaml, out Exception xamlParsingException)
    {
      xamlParsingException = null;

      try
      {
        var context = new ParserContext { XamlTypeMapper = new XamlTypeMapper(new string[] { }) };

        var @namespace   = typeof(XamlControlBase).Namespace;
        var assemblyName = typeof(XamlControlBase).Assembly.GetName().Name;

        context.XmlnsDictionary.Add("sma", $"clr-namespace:{@namespace}");
        // ReSharper disable once AssignNullToNotNullAttribute
        context.XamlTypeMapper.AddMappingProcessingInstruction($"clr-namespace:{@namespace}", @namespace, assemblyName);

        return XamlReader.Parse(xaml, context) as DependencyObject;
      }
      catch (Exception ex)
      {
        xamlParsingException = ex;
        //LogTo.Error(ex, "Exception while parsing layout");

        return null;
      }
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

    public void CopyFrom(XamlLayout other, bool copyValidation = true)
    {
      var restore = AutoValidationSuspended;
      AutoValidationSuspended = copyValidation;

      Name = other.Name;
      Xaml = other.Xaml;

      if (copyValidation)
        IsValid = other.IsValid;

      AutoValidationSuspended = restore;
    }

    public void OnXamlChanged(object before, object after)
    {
      IsValidationRequired = true;

      if (AutoValidationSuspended == false)
        ValidateXaml();

      XamlChanged?.Invoke(this, (string)before, (string)after);
    }

    public void OnNameChanged(object before, object after)
    {
      NameChanged?.Invoke(this, (string)before, (string)after);
    }

    #endregion




    #region Events

    public event PropertyChangedDelegate<XamlLayout, string> NameChanged;

    public event PropertyChangedEventHandler                 PropertyChanged;
    public event PropertyChangedDelegate<XamlLayout, string> XamlChanged;

    #endregion
  }
}
