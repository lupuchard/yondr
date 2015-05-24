public struct EntityIdx {
	public EntityIdx(byte group, ushort idx) {
		Group = group;
		Idx = idx;
	}
	public ushort Idx { get; set; }
	public byte Group { get; set; } 
}
