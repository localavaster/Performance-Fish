// Copyright (c) 2023 bradson
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;

namespace PerformanceFish.Cache;

public sealed class CellGrid<T>
{
	private readonly Map _map;
	private T[] _innerArray;
	private CellIndices _cellIndices;

	public CellGrid(Map map)
	{
		_map = map;
		_innerArray = Array.Empty<T>();
		_cellIndices = default;
		map.InvokeWhenCellIndicesReady(Initialize);
	}

	[MemberNotNull(nameof(_innerArray))]
	private void Initialize(Map map)
	{
		_cellIndices = map.cellIndices;
		_innerArray = new T[_cellIndices.NumGridCells];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EnsureInitialized()
	{
		if (_cellIndices.NumGridCells != _map.cellIndices.NumGridCells)
			Initialize(_map);
	}

	public ref T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			EnsureInitialized();
			return ref _innerArray[index];
		}
	}

	public ref T this[in IntVec3 c]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			EnsureInitialized();
			var index = c.CellToIndex(_cellIndices);
			return ref _innerArray[index];
		}
	}

	public ref T this[CellIndex cellIndex]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			EnsureInitialized();
			return ref _innerArray[cellIndex.Value];
		}
	}
}
