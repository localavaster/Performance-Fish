// Copyright (c) 2023 bradson
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PerformanceFish.Prepatching;

namespace PerformanceFish;

// TODO: benchmark

public sealed class NeedsTrackerCaching : ClassWithFishPrepatches
{
	public sealed class TryGetNeedPatch : FishPrepatch
	{
		public override string? Description { get; }
			= "Caches generic need lookups behind the needs list version.";

		public override MethodBase TargetMethodBase { get; } = GetTargetMethod(outParameter: false);

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(TryGetNeed<Need>);

		public static T? TryGetNeed<T>(Pawn_NeedsTracker instance) where T : Need
			=> TryGetNeedInternal(instance, out T? need) ? need : null;
	}

	public sealed class TryGetNeedOutPatch : FishPrepatch
	{
		public override string? Description { get; }
			= "Caches generic need lookups behind the needs list version.";

		public override MethodBase TargetMethodBase { get; } = GetTargetMethod(outParameter: true);

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(TryGetNeed<Need>);

		public static bool TryGetNeed<T>(Pawn_NeedsTracker instance, out T? need) where T : Need
			=> TryGetNeedInternal(instance, out need);
	}

	private static bool TryGetNeedInternal<T>(Pawn_NeedsTracker instance, out T? need) where T : Need
	{
		ref var cache
			= ref Cache.ByReference<Pawn_NeedsTracker, RuntimeTypeHandle, CacheValue>.GetOrAddReference(instance,
				typeof(T).TypeHandle);

		var needs = instance.needs;
		if (!cache.IsDirty(needs))
		{
			need = (T?)cache.Need;
			return cache.Found;
		}

		return UpdateCache(instance, typeof(T), out need, ref cache);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool UpdateCache<T>(Pawn_NeedsTracker instance, Type needType, out T? need, ref CacheValue cache)
		where T : Need
	{
		var needs = instance.needs;
		for (var i = 0; i < needs.Count; i++)
		{
			var candidate = needs[i];
			if (candidate.GetType() != needType)
				continue;

			need = (T)candidate;
			cache.Update(needs, need, found: true);
			return true;
		}

		need = null;
		cache.Update(needs, null, found: false);
		return false;
	}

	private static MethodInfo GetTargetMethod(bool outParameter)
	{
		foreach (var method in typeof(Pawn_NeedsTracker).GetMethods(AccessTools.all))
		{
			if (method.Name != nameof(Pawn_NeedsTracker.TryGetNeed)
				|| !method.IsGenericMethodDefinition
				|| method.GetParameters().Length != (outParameter ? 1 : 0))
			{
				continue;
			}

			return method;
		}

		return ThrowMissingMethodException(outParameter);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static MethodInfo ThrowMissingMethodException(bool outParameter)
		=> throw new MissingMethodException(
			$"Could not find Pawn_NeedsTracker.TryGetNeed generic overload with {(outParameter ? "one" : "zero")} parameter(s).");

	public record struct CacheValue()
	{
		private List<Need>? _listReference;
		private int _listVersion = -2;
		public Need? Need;
		public bool Found;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsDirty(List<Need> needs) => !ReferenceEquals(needs, _listReference) || needs._version != _listVersion;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(List<Need> needs, Need? need, bool found)
		{
			_listReference = needs;
			_listVersion = needs._version;
			Need = need;
			Found = found;
		}
	}
}
