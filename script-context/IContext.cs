using System.IO;
using System.Numerics;

namespace Yondr {

public interface IContext {
	TextWriter Out { get; }


	EntityIdx? CreateEntityI(string group, string bass);
	void EntitySetPosition(EntityIdx entity, Vector3 position);
	Vector3 EntityGetPosition(EntityIdx entity);
	void EntityLookAt(EntityIdx entity, Vector3 position);

	void SetCamera(EntityIdx entity);
}

}
