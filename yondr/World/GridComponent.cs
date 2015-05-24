// TODO
/*
public class GridGroup: ListGroup {
	
	const ushort GRID_EMPTY = ushort.MaxValue;
	
	public GridGroup(string name, Vec3<ushort> dim): base(name) {
		Dimensions = dim;
		grid = new ushort[dim.X * dim.Y * dim.Z];
		for (int i = 0; i < grid.Length; i++) {
			grid[i] = GRID_EMPTY;
		}
	}
	
	/// @return The Entity at the given cell, or null if there isn't one.
	public Entity GetCell(Vec3<ushort> pos) {
		ushort index = grid[pos.X + pos.Y * Dimensions.X + pos.Z * Dimensions.X * Dimensions.Y];
		return index != GRID_EMPTY ? GetEntity(index) : null;
	}
	
	public Vec3<ushort> Dimensions { get; }
	private ushort[] grid;
}
*/
