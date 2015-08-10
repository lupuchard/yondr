using System;

/// Component for entity groups whose entities can be rendered.
/// The property system for these groups must contain _mesh and _texture string properties.
public class GraphicalComponent: IComponent {
	public void Init(PropertySystem propertySystem) {
		props = propertySystem;
		
		meshProperty = props.WithName(".mesh");
		if (meshProperty == null) {
			Log.Error("Graphical group has no .mesh property.");
		} else if (!meshProperty.Value.Is<string>()) {
			Log.Error("Graphical group's .mesh property is not a string type.");
		}
		
		texProperty = props.WithName(".texture");
		if (texProperty == null) {
			Log.Error("Graphical group has no .texture property.");
		} else if (!texProperty.Value.Is<string>()) {
			Log.Error("Graphical group's .texture property is not a string type.");
		}
	}
	
	public void Add(Entity entity) {
		if (Renderer != null) {
			Renderer.AddObject(entity);
		}
	}
	public void Remove(Entity entity) {
		if (Renderer != null) {
			Renderer.RemoveObject(entity);
		}
	}
	
	private PropertySystem props;
	public IRenderer Renderer { get; set; } = null;
	
	private Property meshProperty;
	public Property MeshProperty    { get { return meshProperty; } }
	private Property texProperty;
	public Property TextureProperty { get { return texProperty; } }
}
