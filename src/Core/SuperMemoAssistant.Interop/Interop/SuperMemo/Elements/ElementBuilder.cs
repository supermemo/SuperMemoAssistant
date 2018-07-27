using System;
using System.Collections.Generic;
using SuperMemoAssistant.Interop.SuperMemo.Components;
using SuperMemoAssistant.Interop.SuperMemo.Components.Types;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.Interop.SuperMemo.Elements
{
  public class ElementBuilder
  {
    public ElementType Type { get; private set; }
    public String Name { get; private set; }
    public int Id { get; private set; }
    public IElement Parent { get; private set; }
    public IConcept Concept { get; private set; }
    public IEnumerable<IConcept> LinkedConcepts => LinkedConceptsInternal;
    public IEnumerable<IComponent> Components => ComponentsInternal;

    private List<IComponent> ComponentsInternal { get; set; }
    private List<IConcept> LinkedConceptsInternal { get; set; }

    public ElementBuilder(ElementType type, string name)
    {
      Type = type;
      Name = name;

      LinkedConceptsInternal = new List<IConcept>();
      ComponentsInternal = new List<IComponent>();
    }

    public ElementBuilder WithId(int id)
    {
      Id = id;
      return this;
    }

    public ElementBuilder WithParent(IElement parent)
    {
      Parent = parent;
      return this;
    }

    public ElementBuilder WithConcept(IConcept concept)
    {
      Concept = concept;
      return this;
    }

    public ElementBuilder AddLinkedConcepts(IEnumerable<IConcept> concepts)
    {
      LinkedConceptsInternal.AddRange(concepts);
      return this;
    }

    public ElementBuilder AddLinkedConcept(IConcept concept)
    {
      LinkedConceptsInternal.Add(concept);
      return this;
    }

    public ElementBuilder AddComponentGroup(IComponentGroup componentGroup)
    {
      ComponentsInternal.AddRange(componentGroup.Components);
      return this;
    }

    public ElementBuilder AddComponents(IEnumerable<IComponent> components)
    {
      ComponentsInternal.AddRange(components);
      return this;
    }

    public ElementBuilder AddComponent(IComponent component)
    {
      ComponentsInternal.Add(component);
      return this;
    }
  }
}
