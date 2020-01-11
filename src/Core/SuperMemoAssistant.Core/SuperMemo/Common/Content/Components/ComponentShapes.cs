using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Components
{
  public class ComponentShapeEllipse : ComponentBase, IComponentShapeEllipse
  {
    public ComponentShapeEllipse(ref InfComponentsShape17 comp)
      : base(comp.left, comp.top, comp.width, comp.height, (AtFlags)comp.displayAt)
    {
    }

    public void Update(ref InfComponentsShape17 comp)
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
    public ComponentShapeRectangle(ref InfComponentsShape17 comp)
      : base(comp.left, comp.top, comp.width, comp.height, (AtFlags)comp.displayAt)
    {
    }

    public void Update(ref InfComponentsShape17 comp)
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
    public ComponentShapeRoundedRectangle(ref InfComponentsShape17 comp)
      : base(comp.left, comp.top, comp.width, comp.height, (AtFlags)comp.displayAt)
    {
    }

    public void Update(ref InfComponentsShape17 comp)
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
