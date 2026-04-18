// Copyright (c) 2026 Bradson
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Mono.Cecil;
using Mono.Cecil.Cil;
using PerformanceFish.Prepatching;

namespace PerformanceFish;

public sealed class CompTickOptimization : ClassWithFishPrepatches
{
	private static readonly MethodInfo _thingCompTickMethod
		= AccessTools.DeclaredMethod(typeof(ThingComp), nameof(ThingComp.CompTick));

	private static readonly MethodInfo _thingCompTickIntervalMethod
		= AccessTools.DeclaredMethod(typeof(ThingComp), nameof(ThingComp.CompTickInterval), [typeof(int)]);

	private static readonly MethodInfo _thingCompTickRareMethod
		= AccessTools.DeclaredMethod(typeof(ThingComp), nameof(ThingComp.CompTickRare));

	private static readonly MethodInfo _thingCompTickLongMethod
		= AccessTools.DeclaredMethod(typeof(ThingComp), nameof(ThingComp.CompTickLong));

	private static readonly MethodInfo _hediffCompPostTickMethod
		= AccessTools.DeclaredMethod(typeof(HediffComp), nameof(HediffComp.CompPostTick), [typeof(float).MakeByRefType()]);

	private static readonly MethodInfo _hediffCompPostTickIntervalMethod = AccessTools.DeclaredMethod(
		typeof(HediffComp),
		nameof(HediffComp.CompPostTickInterval),
		[typeof(float).MakeByRefType(), typeof(int)]);

	private static readonly bool _thingCompTickBaseIsTrivial = IsTrivialVoidMethod(_thingCompTickMethod);
	private static readonly bool _thingCompTickIntervalBaseIsTrivial = IsTrivialVoidMethod(_thingCompTickIntervalMethod);
	private static readonly bool _thingCompTickRareBaseIsTrivial = IsTrivialVoidMethod(_thingCompTickRareMethod);
	private static readonly bool _thingCompTickLongBaseIsTrivial = IsTrivialVoidMethod(_thingCompTickLongMethod);
	private static readonly bool _hediffCompPostTickBaseIsTrivial = IsTrivialVoidMethod(_hediffCompPostTickMethod);
	private static readonly bool _hediffCompPostTickIntervalBaseIsTrivial
		= IsTrivialVoidMethod(_hediffCompPostTickIntervalMethod);

	private static readonly Lazy<bool> _canSkipInheritedBaseThingCompTick
		= new(static () => _thingCompTickBaseIsTrivial && !HasHarmonyPatches(_thingCompTickMethod));

	private static readonly Lazy<bool> _canSkipInheritedBaseThingCompTickInterval
		= new(static () => _thingCompTickIntervalBaseIsTrivial && !HasHarmonyPatches(_thingCompTickIntervalMethod));

	private static readonly Lazy<bool> _canSkipInheritedBaseThingCompTickRare
		= new(static () => _thingCompTickRareBaseIsTrivial && !HasHarmonyPatches(_thingCompTickRareMethod));

	private static readonly Lazy<bool> _canSkipInheritedBaseThingCompTickLong
		= new(static () => _thingCompTickLongBaseIsTrivial && !HasHarmonyPatches(_thingCompTickLongMethod));

	private static readonly Lazy<bool> _canSkipInheritedBaseHediffCompPostTick
		= new(static () => _hediffCompPostTickBaseIsTrivial && !HasHarmonyPatches(_hediffCompPostTickMethod));

	private static readonly Lazy<bool> _canSkipInheritedBaseHediffCompPostTickInterval = new(
		static () => _hediffCompPostTickIntervalBaseIsTrivial && !HasHarmonyPatches(_hediffCompPostTickIntervalMethod));

	private static readonly FishTable<Type, bool> _thingCompTickTypes = new()
	{
		ValueInitializer = static type => InheritsThingCompTick(type)
	};

	private static readonly FishTable<Type, bool> _thingCompTickIntervalTypes = new()
	{
		ValueInitializer = static type => InheritsThingCompTickInterval(type)
	};

	private static readonly FishTable<Type, bool> _thingCompTickRareTypes = new()
	{
		ValueInitializer = static type => InheritsThingCompTickRare(type)
	};

	private static readonly FishTable<Type, bool> _thingCompTickLongTypes = new()
	{
		ValueInitializer = static type => InheritsThingCompTickLong(type)
	};

	private static readonly FishTable<Type, bool> _hediffCompPostTickTypes = new()
	{
		ValueInitializer = static type => InheritsHediffCompPostTick(type)
	};

	private static readonly FishTable<Type, bool> _hediffCompPostTickIntervalTypes = new()
	{
		ValueInitializer = static type => InheritsHediffCompPostTickInterval(type)
	};

	public sealed class ThingTickPatch : FishPrepatch
	{
		public override bool DefaultState => false;

		public override string? Description { get; }
			= "Skips inherited empty ThingComp.CompTick calls when the base method is still unpatched and trivial.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.DeclaredMethod(typeof(ThingWithComps), "Tick");

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(ReplacementBody);

		public static void ReplacementBody(ThingWithComps instance)
		{
			var comps = instance.AllComps;
			var count = comps.Count;
			var canSkipInheritedBase = CanSkipInheritedBaseThingCompTick();
			for (var i = 0; i < count; i++)
			{
				var comp = comps[i];
				if (!canSkipInheritedBase || !_thingCompTickTypes.GetOrAdd(comp.GetType()))
					comp.CompTick();
			}
		}
	}

	public sealed class ThingTickIntervalPatch : FishPrepatch
	{
		public override bool DefaultState => false;

		public override string? Description { get; }
			= "Skips inherited empty ThingComp.CompTickInterval calls when the base method is still unpatched and trivial.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.DeclaredMethod(typeof(ThingWithComps), "TickInterval", [typeof(int)]);

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(ReplacementBody);

		public static void ReplacementBody(ThingWithComps instance, int delta)
		{
			var comps = instance.AllComps;
			var count = comps.Count;
			var canSkipInheritedBase = CanSkipInheritedBaseThingCompTickInterval();
			for (var i = 0; i < count; i++)
			{
				var comp = comps[i];
				if (!canSkipInheritedBase || !_thingCompTickIntervalTypes.GetOrAdd(comp.GetType()))
					comp.CompTickInterval(delta);
			}
		}
	}

	public sealed class ThingTickRarePatch : FishPrepatch
	{
		public override bool DefaultState => false;

		public override string? Description { get; }
			= "Skips inherited empty ThingComp.CompTickRare calls when the base method is still unpatched and trivial.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.DeclaredMethod(typeof(ThingWithComps), nameof(ThingWithComps.TickRare));

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(ReplacementBody);

		public static void ReplacementBody(ThingWithComps instance)
		{
			var comps = instance.AllComps;
			var count = comps.Count;
			var canSkipInheritedBase = CanSkipInheritedBaseThingCompTickRare();
			for (var i = 0; i < count; i++)
			{
				var comp = comps[i];
				if (!canSkipInheritedBase || !_thingCompTickRareTypes.GetOrAdd(comp.GetType()))
					comp.CompTickRare();
			}
		}
	}

	public sealed class ThingTickLongPatch : FishPrepatch
	{
		public override bool DefaultState => false;

		public override string? Description { get; }
			= "Skips inherited empty ThingComp.CompTickLong calls when the base method is still unpatched and trivial.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.DeclaredMethod(typeof(ThingWithComps), nameof(ThingWithComps.TickLong));

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(ReplacementBody);

		public static void ReplacementBody(ThingWithComps instance)
		{
			var comps = instance.AllComps;
			var count = comps.Count;
			var canSkipInheritedBase = CanSkipInheritedBaseThingCompTickLong();
			for (var i = 0; i < count; i++)
			{
				var comp = comps[i];
				if (!canSkipInheritedBase || !_thingCompTickLongTypes.GetOrAdd(comp.GetType()))
					comp.CompTickLong();
			}
		}
	}

	public sealed class HediffPostTickPatch : FishPrepatch
	{
		public override bool DefaultState => false;

		public override string? Description { get; }
			= "Skips inherited empty HediffComp.CompPostTick calls when the base method is still unpatched and trivial.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.DeclaredMethod(typeof(HediffWithComps), nameof(HediffWithComps.PostTick));

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(ReplacementBody);

		public static void ReplacementBody(HediffWithComps instance)
		{
			var comps = instance.comps;
			if (comps == null)
				return;

			var count = comps.Count;
			var severityAdjustment = 0f;
			var canSkipInheritedBase = CanSkipInheritedBaseHediffCompPostTick();

			for (var i = 0; i < count; i++)
			{
				var comp = comps[i];
				if (!canSkipInheritedBase || !_hediffCompPostTickTypes.GetOrAdd(comp.GetType()))
					comp.CompPostTick(ref severityAdjustment);
			}

			if (severityAdjustment != 0f)
				instance.Severity += severityAdjustment;
		}
	}

	public sealed class HediffPostTickIntervalPatch : FishPrepatch
	{
		public override bool DefaultState => false;

		public override string? Description { get; }
			= "Skips inherited empty HediffComp.CompPostTickInterval calls when the base method is still unpatched and trivial.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.DeclaredMethod(typeof(HediffWithComps), nameof(HediffWithComps.PostTickInterval), [typeof(int)]);

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(ReplacementBody);

		public static void ReplacementBody(HediffWithComps instance, int delta)
		{
			var comps = instance.comps;
			if (comps == null)
				return;

			var count = comps.Count;
			var severityAdjustment = 0f;
			var canSkipInheritedBase = CanSkipInheritedBaseHediffCompPostTickInterval();

			for (var i = 0; i < count; i++)
			{
				var comp = comps[i];
				if (!canSkipInheritedBase || !_hediffCompPostTickIntervalTypes.GetOrAdd(comp.GetType()))
					comp.CompPostTickInterval(ref severityAdjustment, delta);
			}

			if (severityAdjustment != 0f)
				instance.Severity += severityAdjustment;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool CanSkipInheritedBaseThingCompTick()
		=> _canSkipInheritedBaseThingCompTick.Value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool CanSkipInheritedBaseThingCompTickInterval()
		=> _canSkipInheritedBaseThingCompTickInterval.Value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool CanSkipInheritedBaseThingCompTickRare()
		=> _canSkipInheritedBaseThingCompTickRare.Value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool CanSkipInheritedBaseThingCompTickLong()
		=> _canSkipInheritedBaseThingCompTickLong.Value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool CanSkipInheritedBaseHediffCompPostTick()
		=> _canSkipInheritedBaseHediffCompPostTick.Value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool CanSkipInheritedBaseHediffCompPostTickInterval()
		=> _canSkipInheritedBaseHediffCompPostTickInterval.Value;

	private static bool InheritsThingCompTick(Type type)
		=> AccessTools.Method(type, nameof(ThingComp.CompTick))?.DeclaringType == typeof(ThingComp);

	private static bool InheritsThingCompTickInterval(Type type)
		=> AccessTools.Method(type, nameof(ThingComp.CompTickInterval), [typeof(int)])?.DeclaringType == typeof(ThingComp);

	private static bool InheritsThingCompTickRare(Type type)
		=> AccessTools.Method(type, nameof(ThingComp.CompTickRare))?.DeclaringType == typeof(ThingComp);

	private static bool InheritsThingCompTickLong(Type type)
		=> AccessTools.Method(type, nameof(ThingComp.CompTickLong))?.DeclaringType == typeof(ThingComp);

	private static bool InheritsHediffCompPostTick(Type type)
		=> AccessTools.Method(type, nameof(HediffComp.CompPostTick), [typeof(float).MakeByRefType()])?.DeclaringType
			== typeof(HediffComp);

	private static bool InheritsHediffCompPostTickInterval(Type type)
		=> AccessTools.Method(type, nameof(HediffComp.CompPostTickInterval),
				[typeof(float).MakeByRefType(), typeof(int)])?.DeclaringType
			== typeof(HediffComp);

	private static bool IsTrivialVoidMethod(MethodInfo method)
	{
		if (method.ReturnType != typeof(void)
			|| method.GetMethodBody() is not { ExceptionHandlingClauses.Count: 0, LocalVariables.Count: 0 } body)
		{
			return false;
		}

		var il = body.GetILAsByteArray();
		if (il == null || il.Length == 0)
			return false;

		var sawRet = false;

		for (var i = 0; i < il.Length; i++)
		{
			switch (il[i])
			{
				case 0x00:
					continue;
				case 0x2A when i == il.Length - 1 && !sawRet:
					sawRet = true;
					continue;
				default:
					return false;
			}
		}

		return sawRet;
	}

	private static bool HasHarmonyPatches(MethodBase method)
	{
		var patches = Harmony.GetPatchInfo(method);
		return patches is not null
			&& (patches.Prefixes.Count != 0
				|| patches.Postfixes.Count != 0
				|| patches.Transpilers.Count != 0
				|| patches.Finalizers.Count != 0);
	}
}
