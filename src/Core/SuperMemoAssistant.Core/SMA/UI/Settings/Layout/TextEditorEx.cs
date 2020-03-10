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
// Created On:   2019/02/27 15:01
// Modified On:  2019/02/27 15:02
// Modified By:  Alexis

#endregion




using System;
using System.ComponentModel;
using System.Windows;
using PropertyChanged;

namespace SuperMemoAssistant.SMA.UI.Settings.Layout
{
  internal class TextEditorEx : ICSharpCode.AvalonEdit.TextEditor, INotifyPropertyChanged
  {
    #region Constants & Statics

    /// <summary>The bindable text property dependency property</summary>
    public static readonly DependencyProperty TextProperty =
      DependencyProperty.Register(
        "Text",
        typeof(string),
        typeof(TextEditorEx),
        new FrameworkPropertyMetadata
        {
          DefaultValue            = default(string),
          BindsTwoWayByDefault    = true,
          PropertyChangedCallback = OnDependencyPropertyChanged
        }
      );

    #endregion




    #region Properties & Fields - Public

    /// <summary>A bindable Text property</summary>
    public new string Text
    {
      get => (string)GetValue(TextProperty);
      set
      {
        SetValue(TextProperty, value);
        RaisePropertyChanged("Text");
      }
    }

    #endregion




    #region Methods Impl
    
    [SuppressPropertyChangedWarnings]
    protected override void OnTextChanged(EventArgs e)
    {
      if (Document != null)
        Text = Document.Text;

      base.OnTextChanged(e);
    }

    #endregion




    #region Methods
    
    [SuppressPropertyChangedWarnings]
    protected static void OnDependencyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      var target = (TextEditorEx)obj;

      if (target.Document != null)
      {
        var caretOffset = target.CaretOffset;
        var newValue    = args.NewValue;

        if (newValue == null)
          newValue = "";

        target.Document.Text = (string)newValue;
        target.CaretOffset   = Math.Min(caretOffset, newValue.ToString().Length);
      }
    }

    /// <summary>Raises a property changed event</summary>
    /// <param name="property">The name of the property that updates</param>
    public void RaisePropertyChanged(string property)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
