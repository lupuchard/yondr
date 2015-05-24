enum ComponentType {
	SPACIAL,
	GRAPHICAL,
	GRID,
}

public interface IComponent {
	void Init(PropertySystem props);
	void Add(Entity entity);
	void Remove(Entity entity);
}
