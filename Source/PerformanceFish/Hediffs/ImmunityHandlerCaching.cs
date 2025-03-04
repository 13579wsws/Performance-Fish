﻿// Copyright (c) 2023 bradson
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using ImmunityInfoCache
	= PerformanceFish.Cache.ByInt<Verse.Pawn,
		PerformanceFish.Hediffs.ImmunityHandlerCaching.ImmunityInfoCacheValue>;
using ImmunityRecordExistsCache
	= PerformanceFish.Cache.ByInt<Verse.Pawn, Verse.HediffDef,
		PerformanceFish.Hediffs.ImmunityHandlerCaching.ImmunityRecordExistsCacheValue>;

namespace PerformanceFish.Hediffs;

public class ImmunityHandlerCaching : ClassWithFishPatches
{
	public class NeededImmunitiesNow : FirstPriorityFishPatch
	{
		public override string Description { get; }
			= "Caches results of the ImmunityHandler.NeededImmunitiesNow method. Impact scales with hediff quantity.";

		public override MethodBase TargetMethodInfo { get; }
			= AccessTools.Method(typeof(ImmunityHandler), nameof(ImmunityHandler.NeededImmunitiesNow));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Prefix(ImmunityHandler __instance, ref List<ImmunityHandler.ImmunityInfo> __result,
			out bool __state)
		{
			ref var cache = ref ImmunityInfoCache.GetOrAddReference(__instance.pawn.thingIDNumber);

			if (cache.Dirty)
				return __state = true;

			(__result = ImmunityHandler.tmpNeededImmunitiesNow).Clear();
			__result.AddRangeFast(cache.Infos);

			return __state = false;
		}

		// ImmunityHandler.tmpNeededImmunitiesNow, which is normally used for the result, is static
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Postfix(ImmunityHandler __instance, List<ImmunityHandler.ImmunityInfo> __result,
			bool __state)
		{
			if (!__state)
				return;

			UpdateCache(__instance, __result);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void UpdateCache(ImmunityHandler __instance, List<ImmunityHandler.ImmunityInfo> __result)
			=> ImmunityInfoCache.GetExistingReference(__instance.pawn)
				.Update(__instance.pawn.health.hediffSet, __result);
	}

	public class ImmunityRecordExists : FishPatch
	{
		public override string? Description { get; } = "Required by the TryAddImmunityRecord optimization.";

		public override bool Enabled
		{
			set
			{
				if (base.Enabled == value)
					return;

				Get<TryAddImmunityRecord>().Enabled = base.Enabled = value;
			}
		}

		public override MethodBase TargetMethodInfo { get; }
			= AccessTools.Method(typeof(ImmunityHandler), nameof(ImmunityHandler.ImmunityRecordExists));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Postfix(ImmunityHandler __instance, HediffDef def, bool __result)
		{
			ref var cache
				= ref ImmunityRecordExistsCache.GetOrAddReference(__instance.pawn.thingIDNumber, def.shortHash);
			cache.Value = __result;
			cache.ImmunityListVersion = __instance.immunityList._version;
		}
	}

	public class TryAddImmunityRecord : FishPatch
	{
		public override string? Description { get; }
			= "Optimization for this specific method to not run more checks than necessary.";
		
		public override bool Enabled
		{
			set
			{
				if (base.Enabled == value)
					return;

				Get<ImmunityRecordExists>().Enabled = base.Enabled = value;
			}
		}

		public override MethodBase TargetMethodInfo { get; }
			= AccessTools.Method(typeof(ImmunityHandler), nameof(ImmunityHandler.TryAddImmunityRecord));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Prefix(ImmunityHandler __instance, HediffDef def)
		{
			ref var cache
				= ref ImmunityRecordExistsCache.GetOrAddReference(__instance.pawn.thingIDNumber, def.shortHash);
			return !cache.Value || cache.ImmunityListVersion != __instance.immunityList._version;
		}
	}

	public record struct ImmunityInfoCacheValue
	{
		private int _version = -1;
		private List<Hediff> _hediffsInSet = new();
		private int _nextRefreshTick;
		public List<ImmunityHandler.ImmunityInfo> Infos = new();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(HediffSet set, List<ImmunityHandler.ImmunityInfo> infos)
		{
			Infos.ReplaceContentsWith(infos);
			
			_hediffsInSet = set.hediffs;
			_version = _hediffsInSet._version;
			_nextRefreshTick = TickHelper.Add(GenTicks.TickLongInterval, set.pawn.thingIDNumber);
		}

		public bool Dirty
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get =>  _version != _hediffsInSet._version || TickHelper.Past(_nextRefreshTick);
		}
		
		public ImmunityInfoCacheValue()
		{
		}
	}

	public record struct ImmunityRecordExistsCacheValue
	{
		public bool Value;
		public int ImmunityListVersion;
	}
}