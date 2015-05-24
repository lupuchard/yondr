using System.IO;

namespace Yondr {

public interface IContext {
	TextWriter Out { get; }


	EntityIdx? CreateEntityI(string group, string bass);
	void EntitySetPosition(EntityIdx entity, Vec3<float> position);
	Vec3<float> EntityGetPosition(EntityIdx entity);
	void EntityLookAt(EntityIdx entity, Vec3<float> position);

	void SetCamera(EntityIdx entity);
}

}
