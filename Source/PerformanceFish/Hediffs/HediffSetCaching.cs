﻿// Copyright (c) 2023 bradson
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Mono.Cecil;
using Mono.Cecil.Cil;
using PerformanceFish.Prepatching;
using HediffCache
	= PerformanceFish.Cache.ByInt<Verse.HediffSet, Verse.HediffDef,
		PerformanceFish.Hediffs.HediffSetCaching.HediffCacheValue>;
using VisibleHediffCache
	= PerformanceFish.Cache.ByInt<Verse.HediffSet, Verse.HediffDef,
		PerformanceFish.Hediffs.HediffSetCaching.VisibleHediffCacheValue>;
using NotMissingPartsCache
	= PerformanceFish.Cache.ByInt<Verse.HediffSet,
		PerformanceFish.Hediffs.HediffSetCaching.NotMissingPartsCacheValue>;

namespace PerformanceFish.Hediffs;

public class HediffSetPrecaching : ClassWithFishPrepatches
{
	public class GetFirstHediffOfDef : FishPrepatch
	{
		public override string Description { get; }
			= "Caches results of the HediffSet.GetFirstHediffOfDef method. Impact scales with hediff count.";
		
		public override MethodBase TargetMethodBase { get; }
			= SymbolExtensions.GetMethodInfo(static ()
				=> default(HediffSet)!.GetFirstHediffOfDef(null, default));

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(Replacement);
		
		public static Hediff? Replacement(HediffSet instance, HediffDef? def, bool mustBeVisible)
		{
			var canCache = true;

			if (instance.hediffs.Count >= 5
				&& def is not null
				// && def.GetType() == typeof(HediffDef) // different types have different indexing. Can't use those
				)
			{
				ref var cache = ref GetCacheValue(instance, def, mustBeVisible);
				if (!cache.Dirty)
					return cache.Hediff;
			}
			else
			{
				canCache = false;
			}

			// original method, mostly left untouched in case of other mods patching in
			Hediff? result;
			var hediffs = instance.hediffs;
			for (var i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].def != def || (mustBeVisible && !hediffs[i].Visible))
					continue;

				result = hediffs[i];
				goto Result;
			}

			result = null;

		Result:
			if (canCache)
				UpdateCache(instance, def!, mustBeVisible, result);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe ref HediffSetCaching.HediffCacheValue GetCacheValue(HediffSet instance, HediffDef def,
			bool mustBeVisible)
		{
			var key = new HediffCache { First = instance.pawn.thingIDNumber, Second = def.shortHash };
			ref var cache = ref mustBeVisible
				? ref Unsafe.As<HediffSetCaching.VisibleHediffCacheValue, HediffSetCaching.HediffCacheValue>(
					ref VisibleHediffCache.GetOrAddReference(Unsafe.As<HediffCache, VisibleHediffCache>(ref key)))
				: ref HediffCache.GetOrAddReference(key);
			
			return ref cache;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void UpdateCache(HediffSet __instance, HediffDef def, bool mustBeVisible, Hediff? __result)
		{
			if (mustBeVisible)
				VisibleHediffCache.GetExistingReference(__instance, def).Update(__instance, __result);
			else
				HediffCache.GetExistingReference(__instance, def).Update(__instance, __result);
		}
	}

	public class HasHediff : FishPrepatch
	{
		public override string Description { get; }
			= "Caches results of the HediffSet.HasHediff method. Impact scales with hediff count.";
		
		public override MethodBase TargetMethodBase { get; }
			= SymbolExtensions.GetMethodInfo(static ()
				=> default(HediffSet)!.HasHediff(null, default));

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(Replacement);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Replacement(HediffSet __instance, HediffDef def, bool mustBeVisible)
			=> __instance.GetFirstHediffOfDef(def, mustBeVisible) != null;
	}
	
	public class GetNotMissingParts : FishPrepatch
	{
		public override string? Description { get; }
			= "Caches results of the HediffSet.GetNotMissingParts method. Impact scales with hediff count.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.Method(typeof(HediffSet), nameof(HediffSet.GetNotMissingParts));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Prefix(HediffSet __instance, BodyPartHeight height, BodyPartDepth depth, BodyPartTagDef? tag,
			BodyPartRecord? partParent, ref IEnumerable<BodyPartRecord> __result, out bool __state)
		{
			if (height != BodyPartHeight.Undefined
				|| depth != BodyPartDepth.Undefined
				|| tag != null
				|| partParent != null)
			{
				__state = false;
				return true;
			}

			ref var cache = ref NotMissingPartsCache.GetOrAddReference(__instance.pawn.thingIDNumber);

			if (cache.Dirty)
				return __state = true;

			__result = cache.Parts;
			return __state = false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Postfix(HediffSet __instance, bool __state, ref IEnumerable<BodyPartRecord> __result)
		{
			if (!__state)
				return;

			UpdateCache(__instance, ref __result);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void UpdateCache(HediffSet __instance, ref IEnumerable<BodyPartRecord> __result)
			=> NotMissingPartsCache.GetExistingReference(__instance).Update(__instance, ref __result);
	}
}

public class HediffSetCaching : ClassWithFishPatches
{
	public class DirtyCache : FishPatch
	{
		public override string Description { get; }
			= "Patched to trigger psylink cache clearing for the psychic entropy optimization.";

		public override MethodBase TargetMethodInfo { get; }
			= AccessTools.Method(typeof(HediffSet), nameof(HediffSet.DirtyCache));

		public static void Postfix(HediffSet __instance)
		{
			if (__instance.pawn.psychicEntropy is { } entropy)
				entropy.psylinkCachedForTick = 0;
		}
	}

	public record struct NotMissingPartsCacheValue
	{
		private int _version = -1;
		private List<Hediff> _hediffsInSet = new();
		private int _nextRefreshTick;
		public List<BodyPartRecord> Parts = new();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(HediffSet set, ref IEnumerable<BodyPartRecord> result)
		{
			_hediffsInSet = set.hediffs;
			_version = set.hediffs._version;
			_nextRefreshTick = TickHelper.Add(GenTicks.TickLongInterval, set.pawn.thingIDNumber);

			var previousPartsVersion = Parts._version;
			Parts.Clear();
			Parts.AddRange(result);
			Parts._version = previousPartsVersion;

			result = Parts;
		}

		public bool Dirty
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get =>  _version != _hediffsInSet._version || TickHelper.Past(_nextRefreshTick);
		}

		public NotMissingPartsCacheValue()
		{
		}
	}

	public record struct HediffCacheValue
	{
		private int _version = -1;
		private List<Hediff> _hediffsInSet = new();
		private int _nextRefreshTick;
		public Hediff? Hediff;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(HediffSet set, Hediff? hediff)
		{
			_hediffsInSet = set.hediffs;
			Hediff = hediff;
			_version = _hediffsInSet._version;
			_nextRefreshTick = TickHelper.Add(GenTicks.TickLongInterval, set.pawn.thingIDNumber);
		}

		public bool Dirty
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _version != _hediffsInSet._version || TickHelper.Past(_nextRefreshTick);
		}

		public HediffCacheValue()
		{
		}
	}

	public record struct VisibleHediffCacheValue
	{
		public HediffCacheValue Value = new();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(HediffSet set, Hediff? hediff) => Value.Update(set, hediff);

		public VisibleHediffCacheValue()
		{
		}
	}
}