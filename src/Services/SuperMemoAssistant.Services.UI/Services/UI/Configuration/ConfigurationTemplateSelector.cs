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
// Created On:   2019/03/01 12:43
// Modified On:  2019/03/01 12:46
// Modified By:  Alexis

#endregion




using System.Windows;
using System.Windows.Controls;
using SuperMemoAssistant.Services.IO.HotKeys;
using SuperMemoAssistant.Sys.ComponentModel;

namespace SuperMemoAssistant.Services.UI.Configuration
{
  internal class ConfigurationTemplateSelector : DataTemplateSelector
  {
    #region Methods Impl

    public override DataTemplate SelectTemplate(
      object item,
      DependencyObject container)
    {
      if (container is FrameworkElement element)
      {
        if (item is INotifyPropertyChangedEx)
          return element.FindResource("ConfigModelTemplate") as DataTemplate;

        if (item is HotKeyManager)
          return element.FindResource("HotKeyManagerTemplate") as DataTemplate;
      }

      return null;
    }

    #endregion
  }
}
