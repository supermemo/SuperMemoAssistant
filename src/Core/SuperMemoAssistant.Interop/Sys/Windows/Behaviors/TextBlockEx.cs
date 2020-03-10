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
// Modified On:  2020/02/29 12:13
// Modified By:  Alexis

#endregion




using System;
using System.Windows;
using System.Windows.Controls;

namespace SuperMemoAssistant.Sys.Windows.Behaviors
{
  public static class TextBlockEx
  {
    #region Constants & Statics

    public static readonly DependencyProperty MaxLinesProperty =
      DependencyProperty.RegisterAttached(
        "MaxLines",
        typeof(int),
        typeof(TextBlockEx),
        new PropertyMetadata(default(int), OnMaxLinesPropertyChangedCallback));

    public static readonly DependencyProperty MinLinesProperty =
      DependencyProperty.RegisterAttached(
        "MinLines",
        typeof(int),
        typeof(TextBlockEx),
        new PropertyMetadata(default(int), OnMinLinesPropertyChangedCallback));

    #endregion




    #region Methods

    public static void SetMaxLines(DependencyObject element, int value)
    {
      element.SetValue(MaxLinesProperty, value);
    }

    public static int GetMaxLines(DependencyObject element)
    {
      return (int)element.GetValue(MaxLinesProperty);
    }

    private static void OnMaxLinesPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is TextBlock textBlock)
        textBlock.MaxHeight = GetLineHeight(textBlock) * GetMaxLines(textBlock);

      else
        throw new InvalidOperationException("This property can only be attached to a TextBlock");
    }

    public static void SetMinLines(DependencyObject element, int value)
    {
      element.SetValue(MinLinesProperty, value);
    }

    public static int GetMinLines(DependencyObject element)
    {
      return (int)element.GetValue(MinLinesProperty);
    }

    private static void OnMinLinesPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is TextBlock textBlock)
        textBlock.MinHeight = GetLineHeight(textBlock) * GetMinLines(textBlock);

      else
        throw new InvalidOperationException("This property can only be attached to a TextBlock");
    }

    private static double GetLineHeight(TextBlock textBlock)
    {
      double lineHeight = textBlock.LineHeight;

      if (double.IsNaN(lineHeight))
        lineHeight = Math.Ceiling(textBlock.FontSize * textBlock.FontFamily.LineSpacing);

      return lineHeight;
    }

    #endregion
  }
}
