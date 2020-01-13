using System.Windows;
using System.Windows.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlControls
{
  public static class Panels
  {
    #region Constants & Statics

    // Using a DependencyProperty as the backing store for AcceptedContent.
    public static readonly DependencyProperty AcceptedContentProperty =
      DependencyProperty.RegisterAttached("AcceptedContent", typeof(ContentTypeFlag), typeof(Panel), new PropertyMetadata(ContentTypeFlag.None));

    #endregion




    #region Methods

    public static ContentTypeFlag GetAcceptedContent(DependencyObject obj)
    {
      return (ContentTypeFlag)obj.GetValue(AcceptedContentProperty);
    }

    public static void SetAcceptedContent(DependencyObject obj,
                                          ContentTypeFlag  value)
    {
      obj.SetValue(AcceptedContentProperty, value);
    }

    #endregion
  }
}
