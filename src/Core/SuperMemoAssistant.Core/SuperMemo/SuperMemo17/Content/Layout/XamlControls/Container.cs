using System.Windows;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Content.Layout.XamlControls
{
  public class Container : System.Windows.Controls.Panel
  {
    #region Constants & Statics

    // Using a DependencyProperty as the backing store for AcceptedContent.
    public static readonly DependencyProperty AcceptedContentProperty =
      DependencyProperty.RegisterAttached("AcceptedContent", typeof(ContentTypeFlag), typeof(Container), new PropertyMetadata(ContentTypeFlag.None));

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
