using SuperMemoAssistant.Interop.SuperMemo.Components.Models;

namespace SuperMemoAssistant.Interop.SuperMemo.Components
{

  public abstract class ComponentBuilder
  {
    public ComponentType Type { get; private set; }

    public ComponentBuilder(ComponentType type)
    {
      Type = type;
    }

    public static IComponentGroup DefaultArticle { get; }
    public static IComponentGroup DefaultArticlePicture { get; }
    public static IComponentGroup DefaultItem { get; }
    public static IComponentGroup DefaultItemPicture { get; }
    
    public static IComponentGroup FromClipboard(string clipboardText)
    {
      return null;
    }
  }

  public class HTMLComponentBuilder : ComponentBuilder
  {
    public HTMLComponentBuilder()
      : base(ComponentType.Html)
    { }
  }
}
