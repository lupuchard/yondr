using System;

public interface IRenderer {
	void Render();
	Entity Camera { get; set; }
	void AddObject(Entity obj);
	void RemoveObject(Entity obj);
	void SetTexture(Entity entity, string texture);
	void SetMesh(Entity entity, string mesh);
}
