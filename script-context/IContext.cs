using System.IO;
using System.Numerics;

namespace Yondr {

public interface IContext {
	TextWriter Out { get; }

	bool Control(string name);

	EntityIdx? CreateEntityI(string group, string bass);

	void EntitySetPosition(EntityIdx entity, Vector3 position);
	Vector3 EntityGetPosition(EntityIdx entity);
	void EntityMove(EntityIdx entity, Vector3 amount);

	void EntityLookAt(EntityIdx entity, Vector3 position);
	void EntityRotateX(EntityIdx entity, float radians);
	void EntityRotateY(EntityIdx entity, float radians);
	void EntityRotateZ(EntityIdx entity, float radians);
	Vector3 EntityGetDirection(EntityIdx entity);

	void SetCamera(EntityIdx entity);

	/*
	Val GetPropertyValue(EntityIdx entity, int property);
	void SetPropertyValue(EntityIdx entity, int property, Val value);
	*/
}

}
