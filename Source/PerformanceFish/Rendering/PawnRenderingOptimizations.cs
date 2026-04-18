// Copyright (c) 2026 Bradson
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Mono.Cecil;
using Mono.Cecil.Cil;
using PerformanceFish.Prepatching;

namespace PerformanceFish.Rendering;

public sealed class PawnRenderingOptimizations : ClassWithFishPrepatches
{
	private const int DAMAGE_FLASH_TICKS_TOTAL = 16;

	public sealed class IsHiddenFromPlayerPatch : FishPrepatch
	{
		// not 100% on this even benefitting, bionics, scars, etc likely worsen performance here because of the extra check
		// probably better to just track if a hediff adds invisibility
		public override string? Description { get; }
			= "Fast-paths InvisibilityUtility.IsHiddenFromPlayer for the case where a pawn has no hediffs.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.DeclaredMethod(typeof(InvisibilityUtility),
				nameof(InvisibilityUtility.IsHiddenFromPlayer), [typeof(Pawn)]);

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(ReplacementBody);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReplacementBody(Pawn pawn)
		{
			if (DebugSettings.showHiddenPawns)
				return false;

			if (pawn.Faction == Faction.OfPlayer)
				return false;

			var hediffs = pawn.health.hediffSet.hediffs;
			if (hediffs.Count == 0)
				return false;

			for (var i = 0; i < hediffs.Count; i++)
			{
				var comp = hediffs[i].TryGetComp<HediffComp_Invisibility>();
				if (comp != null && !comp.Props.visibleToPlayer && !comp.PsychologicallyVisible)
					return true;
			}

			return false;
		}
	}

	public sealed class PawnRenderTreeComputeMatrixPatch : FishPrepatch
	{
		public override string? Description { get; }
			= "Avoids Matrix4x4 inverse work when undoing a pivot translation in PawnRenderTree.ComputeMatrix.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.DeclaredMethod(typeof(PawnRenderTree), "ComputeMatrix");

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(ReplacementBody);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ReplacementBody(PawnRenderTree instance, ref Matrix4x4 matrix, in Vector3 offset,
			in Vector3 pivot, in Quaternion rotation, in Vector3 scale, bool canRotate)
		{
			if (offset != Vector3.zero)
				matrix *= Matrix4x4.Translate(offset);

			if (pivot != Vector3.zero)
				matrix *= Matrix4x4.Translate(pivot);

			if (canRotate && rotation != Quaternion.identity)
				matrix *= Matrix4x4.Rotate(rotation);

			if (scale != Vector3.one)
				matrix *= Matrix4x4.Scale(scale);

			if (pivot != Vector3.zero)
				matrix *= Matrix4x4.Translate(-pivot);
		}
	}

	public sealed class DamageFlasherCurColorPatch : FishPrepatch
	{
		private static readonly AccessTools.FieldRef<DamageFlasher, int> _lastDamageTick
			= AccessTools.FieldRefAccess<DamageFlasher, int>("lastDamageTick");

		public override string? Description { get; }
			= "Fast-paths DamageFlasher.CurColor when no damage flash is active.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.DeclaredPropertyGetter(typeof(DamageFlasher), nameof(DamageFlasher.CurColor));

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(ReplacementBody);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Color ReplacementBody(DamageFlasher instance)
		{
			var ticksLeft = _lastDamageTick(instance) + DAMAGE_FLASH_TICKS_TOTAL - Find.TickManager.TicksGame;
			return ticksLeft <= 0
				? Color.white
				: Color.Lerp(Color.white, DamagedMatPool.DamagedMatStartingColor,
					(float)ticksLeft / DAMAGE_FLASH_TICKS_TOTAL);
		}
	}

	public sealed class DamageFlasherGetDamagedMatPatch : FishPrepatch
	{
		private static readonly AccessTools.FieldRef<DamageFlasher, int> _lastDamageTick
			= AccessTools.FieldRefAccess<DamageFlasher, int>("lastDamageTick");

		public override string? Description { get; }
			= "Fast-paths DamageFlasher.GetDamagedMat when no damage flash is active.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.DeclaredMethod(typeof(DamageFlasher), nameof(DamageFlasher.GetDamagedMat));

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(ReplacementBody);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Material ReplacementBody(DamageFlasher instance, Material baseMat)
		{
			var ticksLeft = _lastDamageTick(instance) + DAMAGE_FLASH_TICKS_TOTAL - Find.TickManager.TicksGame;
			return ticksLeft <= 0
				? baseMat
				: DamagedMatPool.GetDamageFlashMat(baseMat, (float)ticksLeft / DAMAGE_FLASH_TICKS_TOTAL);
		}
	}
}
