// Copyright (c) 2023 bradson
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace PerformanceFish.Rendering;

public sealed class TextureAtlasPatches : ClassWithFishPatches
{
	public sealed class StaticTextureAtlas_ApplyTextureCompression : FishPatch
	{
		public override bool DefaultState => false;

		public override string? Description { get; }
			= "Routes texture atlas compression through the GPU path when compute shaders are available, "
			+ "regardless of the VRAM threshold check in UnityData.ComputeShadersSupported. Prevents native "
			+ "crashes in debug Mono builds.";

		public override MethodBase TargetMethodInfo
			=> AccessTools.DeclaredMethod(typeof(StaticTextureAtlas), "ApplyTextureCompression");

		public static bool Prefix(StaticTextureAtlas __instance, bool noGpuCompressionSupport)
		{
			if (!noGpuCompressionSupport || !SystemInfo.supportsComputeShaders)
				return true;

			if (__instance.ColorTexture != null)
			{
				var name = __instance.ColorTexture.name;
				var compressed = StaticTextureAtlas.FastCompressDXT(__instance.ColorTexture, deleteOriginal: true);
				compressed.name = name;
				_colorTextureField.SetValue(__instance, compressed);
			}

			if (__instance.MaskTexture != null)
			{
				var name = __instance.MaskTexture.name;
				var compressed = StaticTextureAtlas.FastCompressDXT(__instance.MaskTexture, deleteOriginal: true);
				compressed.name = name;
				_maskTextureField.SetValue(__instance, compressed);
			}

			return false;
		}

		private static readonly FieldInfo _colorTextureField
			= AccessTools.DeclaredField(typeof(StaticTextureAtlas), "colorTexture");

		private static readonly FieldInfo _maskTextureField
			= AccessTools.DeclaredField(typeof(StaticTextureAtlas), "maskTexture");
	}
}
