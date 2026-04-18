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

			if (SavedDayTickTick != TickHelper.TicksGame)
				return __state = true;

			__result = DayTick;
			return __state = false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Postfix(Map map, int __result, bool __state)
		{
			if (!__state || SavedMap != map)
				return;

			SavedDayTickTick = TickHelper.TicksGame;
			DayTick = __result;
		}
	}

	public sealed class HourIntegerByMap_Patch : FishPatch
	{
		public override string Description { get; }
			= "Caches results of GenLocalDate.HourInteger for the first map.";

		public override Delegate TargetMethodGroup { get; } = (Func<Map, int>)GenLocalDate.HourInteger;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Prefix(Map map, ref int __result, out bool __state)
		{
			if (map != SavedMap)
			{
				if (SavedMap is null && Find.Maps is { Count: > 0 } maps && maps[0] == map)
					SavedMap = map;

				return __state = true;
			}

			if (SavedHourIntegerTick != TickHelper.TicksGame)
				return __state = true;

			__result = HourInteger;
			return __state = false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Postfix(Map map, int __result, bool __state)
		{
			if (!__state || SavedMap != map)
				return;

			SavedHourIntegerTick = TickHelper.TicksGame;
			HourInteger = __result;
		}
	}

	public sealed class SeasonByMap_Patch : FishPatch
	{
		public override string Description { get; }
			= "Caches results of GenLocalDate.Season for the first map.";

		public override Delegate TargetMethodGroup { get; } = (Func<Map, Season>)GenLocalDate.Season;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Prefix(Map map, ref Season __result, out bool __state)
		{
			if (map != SavedMap)
			{
				if (SavedMap is null && Find.Maps is { Count: > 0 } maps && maps[0] == map)
					SavedMap = map;

				return __state = true;
			}

			if (SavedSeasonTick != TickHelper.TicksGame)
				return __state = true;

			__result = Season;
			return __state = false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Postfix(Map map, Season __result, bool __state)
		{
			if (!__state || SavedMap != map)
				return;

			SavedSeasonTick = TickHelper.TicksGame;
			Season = __result;
		}
	}

	public static Map? SavedMap;
	public static int SavedDayTickTick = -2;
	public static int SavedHourIntegerTick = -2;
	public static int SavedSeasonTick = -2;
	public static int DayTick = -2;
	public static int HourInteger = -2;
	public static Season Season = Season.Undefined;
}
