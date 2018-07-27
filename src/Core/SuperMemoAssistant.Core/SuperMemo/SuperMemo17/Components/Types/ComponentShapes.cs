using SuperMemoAssistant.Interop.SuperMemo.Components.Models;
using SuperMemoAssistant.Interop.SuperMemo.Components.Types;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Components.Types
{
  public class ComponentShapeEllipse : ComponentBase, IComponentShapeEllipse
  {
    public ComponentShapeEllipse(InfComponentsShape comp)
      : base(comp.left, comp.top, comp.right, comp.bottom, (AtFlags)comp.displayAt)
    {
    }

    public void Update(InfComponentsShape comp)
    {
      base.Update(
        comp.left, comp.top,
        comp.right, comp.bottom,
        (AtFlags)comp.displayAt,
        ComponentFieldFlags.None
      );
    }
  }

  public class ComponentShapeRectangle : ComponentBase, IComponentShapeRectangle
  {
    public ComponentShapeRectangle(InfComponentsShape comp)
      : base(comp.left, comp.top, comp.right, comp.bottom, (AtFlags)comp.displayAt)
    {
    }

    public void Update(InfComponentsShape comp)
    {
      base.Update(
        comp.left, comp.top,
        comp.right, comp.bottom,
        (AtFlags)comp.displayAt,
        ComponentFieldFlags.None
      );
    }
  }

  public class ComponentShapeRoundedRectangle : ComponentBase, IComponentShapeRoundedRectangle
  {
    public ComponentShapeRoundedRectangle(InfComponentsShape comp)
      : base(comp.left, comp.top, comp.right, comp.bottom, (AtFlags)comp.displayAt)
    {
    }

    public void Update(InfComponentsShape shape)
    {
      base.Update(
        shape.left, shape.top,
        shape.right, shape.bottom,
        (AtFlags)shape.displayAt,
        ComponentFieldFlags.None
      );
    }
  }
}
