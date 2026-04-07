// Copyright (c) 2023 bradson
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace PerformanceFish.Cache;

public sealed class BitCellGrid
{
	private readonly Map _map;
	private long[] _innerArray;
	private CellIndices _cellIndices;

	public BitCellGrid(Map map)
	{
		_map = map;
		_innerArray = Array.Empty<long>();
		_cellIndices = default;
		map.InvokeWhenCellIndicesReady(Initialize);
	}

	private void Initialize(Map map)
	{
		_cellIndices = map.cellIndices;
		_innerArray = new long[(Math.Max(0, _cellIndices.NumGridCells - 1) >> 6) + 1];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EnsureInitialized()
	{
		if (_cellIndices.NumGridCells != _map.cellIndices.NumGridCells)
			Initialize(_map);
	}

	public bool this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			EnsureInitialized();
			ValidateIndex(index);
			return (_innerArray[index >> 6] & (1L << (index & 63))) != 0L;
		}
		set
		{
			EnsureInitialized();
			ValidateIndex(index);
			ref var bucket = ref _innerArray[index >> 6];
			bucket ^= (-(long)value.AsInt() ^ bucket) & (1L << (index & 63));
		}
	}

	public bool this[in IntVec3 c]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			EnsureInitialized();
			return this[c.CellToIndex(_cellIndices)];
		}
		set
		{
			EnsureInitialized();
			this[c.CellToIndex(_cellIndices)] = value;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void ValidateIndex(int index)
	{
		if ((uint)index >= (uint)_cellIndices.NumGridCells)
		{
			throw new IndexOutOfRangeException($"BitCellGrid index {index} out of bounds for {_cellIndices.NumGridCells} cells on map size {_map.Size}.");
		}
	}
}
