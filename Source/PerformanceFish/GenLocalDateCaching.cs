// Copyright (c) 2023 bradson
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Linq;
using System.Reflection.Emit;

// ReSharper disable PossibleMultipleEnumeration

namespace PerformanceFish;

public sealed class GenLocalDateCaching : ClassWithFishPatches
{
	public sealed class DayTickByThing_Patch : FishPatch
	{
		public override string Description { get; }
			= "Caches results of GenLocalDate.DayTick for the first map. This is similar to Rim73's mind state "
			+ "optimization, but yields accurate results instead of a placeholder value to avoid issues";

		public override Delegate TargetMethodGroup { get; } = (Func<Thing, int>)GenLocalDate.DayTick;
		public override int PrefixMethodPriority => Priority.First;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Prefix(Thing thing, ref int __result)
		{
			if (thing.TryGetMap() is not { } map)
				return true;

			__result = GenLocalDate.DayTick(map);
			return false;
		}
	}

	public sealed class DayTickByMap_Patch : FishPatch
	{
		public override string Description { get; }
			= "Caches results of GenLocalDate.DayTick for the first map. This is similar to Rim73's mind state "
			+ "optimization, but yields accurate results instead of a placeholder value to avoid issues";

		public override Delegate TargetMethodGroup { get; } = (Func<Map, int>)GenLocalDate.DayTick;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Prefix(Map map, ref int __result, out bool __state)
		{
			if (map != SavedMap)
			{
				if (SavedMap is null && Find.Maps is { Count: > 0 } maps && maps[0] == map)
					SavedMap = map;

				return __state = true;
			}

			if (SavedTick != TickHelper.TicksGame)
				return __state = true;

			__result = DayTick;
			return __state = false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Postfix(Map map, int __result, bool __state)
		{
			if (!__state || SavedMap != map)
				return;

			SavedTick = TickHelper.TicksGame;
			DayTick = __result;
		}
	}

	public static Map? SavedMap;
	public static int SavedTick = -2;
	public static int DayTick = -2;
}
