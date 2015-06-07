using System;

public interface IRenderer {
	void Render();
	Entity Camera { get; set; }
	void AddObject(Entity obj);
	void RemoveObject(Entity obj);
}
