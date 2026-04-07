// Copyright (c) 2023 bradson
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace PerformanceFish;

public record struct CellIndex(int Value)
{
	public int Value = Value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CellIndex(IntVec3 cell, Map map) : this(cell, map.Size.x) {}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CellIndex(IntVec3 cell, int mapSizeX) : this(cell.CellToIndex(mapSizeX)) {}

	public IntVec3 ToCell(Map map) => ToCell(map.Size.x);
	
	public IntVec3 ToCell(int mapSizeX) => CellIndicesUtility.IndexToCell(Value, mapSizeX);

	public List<Thing> GetThingList(Map map) => map.thingGrid.thingGrid[Value];

	public bool IsFogged(Map map)
#if V1_6
		=> map.fogGrid.fogGrid.IsSet(Value);
#else
		=> map.fogGrid.fogGrid[Value];
#endif

	public float GetSnowDepth(Map map) => map.snowGrid.depthGrid[Value];

	public Building? GetEdifice(Map map) => map.edificeGrid[Value];
}
