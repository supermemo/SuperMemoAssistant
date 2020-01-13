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
// Created On:   2019/03/01 23:36
// Modified On:  2019/03/02 00:17
// Modified By:  Alexis

#endregion




using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlControls
{
  public static class Grids
  {
    #region Constants & Statics

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CollapsableProperty =
      DependencyProperty.RegisterAttached("Collapsable", typeof(CollapsableGridAttachedProperty), typeof(Grid),
                                          new PropertyMetadata(null, OnCollapsableChanged));

    #endregion




    #region Methods

    [TypeConverter(typeof(CollapsableConverter))]
    public static CollapsableGridAttachedProperty GetCollapsable(DependencyObject obj)
    {
      return (CollapsableGridAttachedProperty)obj.GetValue(CollapsableProperty);
    }

    [TypeConverter(typeof(CollapsableConverter))]
    public static void SetCollapsable(DependencyObject obj, CollapsableGridAttachedProperty value)
    {
      obj.SetValue(CollapsableProperty, value);
    }

    private static void OnCollapsableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (!(d is Grid grid))
        throw new ArgumentException("Collapsable can only be attached on Grids types");

      if (e.NewValue != null)
      {
        var cgap = (CollapsableGridAttachedProperty)e.NewValue;

        cgap.Attach(grid);
      }
    }

    #endregion




    public class CollapsableConverter : TypeConverter
    {
      #region Methods Impl

      public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
      {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
      }

      public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
      {
        if (value is string str)
          return (CollapsableGridAttachedProperty)str;

        return base.ConvertFrom(context, culture, value);
      }

      #endregion
    }
  }
}
