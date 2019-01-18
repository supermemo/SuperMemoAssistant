using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Components.Types
{
  public class ComponentShapeEllipse : ComponentBase, IComponentShapeEllipse
  {
    public ComponentShapeEllipse(InfComponentsShape comp)
      : base(comp.left, comp.top, comp.width, comp.height, (AtFlags)comp.displayAt)
    {
    }

    public void Update(InfComponentsShape comp)
    {
      base.Update(
        comp.left, comp.top,
        comp.width, comp.height,
        (AtFlags)comp.displayAt,
        ComponentFieldFlags.None
      );
    }
  }

  public class ComponentShapeRectangle : ComponentBase, IComponentShapeRectangle
  {
    public ComponentShapeRectangle(InfComponentsShape comp)
      : base(comp.left, comp.top, comp.width, comp.height, (AtFlags)comp.displayAt)
    {
    }

    public void Update(InfComponentsShape comp)
    {
      base.Update(
        comp.left, comp.top,
        comp.width, comp.height,
        (AtFlags)comp.displayAt,
        ComponentFieldFlags.None
      );
    }
  }

  public class ComponentShapeRoundedRectangle : ComponentBase, IComponentShapeRoundedRectangle
  {
    public ComponentShapeRoundedRectangle(InfComponentsShape comp)
      : base(comp.left, comp.top, comp.width, comp.height, (AtFlags)comp.displayAt)
    {
    }

    public void Update(InfComponentsShape comp)
    {
      base.Update(
        comp.left, comp.top,
        comp.width, comp.height,
        (AtFlags)comp.displayAt,
        ComponentFieldFlags.None
      );
    }
  }
}
