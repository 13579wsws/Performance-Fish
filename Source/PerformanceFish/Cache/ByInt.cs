﻿// Copyright (c) 2023 bradson
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using JetBrains.Annotations;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace PerformanceFish.Cache;

#pragma warning disable CS9091
[PublicAPI]
public record struct ByInt<T, TResult> where T : notnull where TResult : new()
{
	private static FishTable<ByInt<T, TResult>, TResult> _get
		= Utility.AddNew<ByInt<T, TResult>, TResult>();

	[ThreadStatic]
	private static FishTable<ByInt<T, TResult>, TResult>? _getThreadStatic;

	public static FishTable<ByInt<T, TResult>, TResult> Get
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _getThreadStatic ??= Utility.AddNew<ByInt<T, TResult>, TResult>();
	}

	public static FishTable<ByInt<T, TResult>, TResult> GetDirectly
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _get;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref TResult GetOrAddReference(int key)
		=> ref Get.GetOrAddReference(Unsafe.As<int, ByInt<T, TResult>>(ref key));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref TResult GetOrAddReference(ByInt<T, TResult> key) => ref Get.GetOrAddReference(key);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref TResult GetOrAddReference(ref ByInt<T, TResult> key) => ref Get.GetOrAddReference(ref key);

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static ref TResult GetExistingReference(int key)
		=> ref Get.GetReference(Unsafe.As<int, ByInt<T, TResult>>(ref key));

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static ref TResult GetExistingReference(T key) => ref Get.GetReference(new(key));

	public int Key;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ByInt(T key) => Key = FunctionPointers.IndexGetter<T>.Default(key);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ByInt(int key) => Key = key;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(ByInt<T, TResult> other) => Key == other.Key;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode() => Key;
}

[PublicAPI]
public record struct ByInt<T1, T2, TResult>
	where T1 : notnull where T2 : notnull where TResult : new()
{
	private static FishTable<ByInt<T1, T2, TResult>, TResult> _get
		= Utility.AddNew<ByInt<T1, T2, TResult>, TResult>();

	[ThreadStatic]
	private static FishTable<ByInt<T1, T2, TResult>, TResult>? _getThreadStatic;

	public static FishTable<ByInt<T1, T2, TResult>, TResult> Get
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _getThreadStatic ??= Utility.AddNew<ByInt<T1, T2, TResult>, TResult>();
	}

	private static ref FishTable<ByInt<T1, T2, TResult>, TResult> GetCacheRef
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			_getThreadStatic ??= Utility.AddNew<ByInt<T1, T2, TResult>, TResult>();
			return ref _getThreadStatic!;
		}
	}

	public static FishTable<ByInt<T1, T2, TResult>, TResult> GetDirectly
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _get;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref TResult GetOrAddReference(int first, int second)
		=> ref Get.GetOrAddReference(new() { First = first, Second = second });

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref TResult GetOrAddReference(in ByInt<T1, T2, TResult> key)
		=> ref Get.GetOrAddReference(ref Unsafe.AsRef(in key));

	// [MethodImpl(MethodImplOptions.AggressiveInlining)]
	// public static ref TResult GetOrAddReference(long key) // different, worse, GetHashCode method
	// 	=> ref Unsafe.As<FishTable<ByInt<T1, T2, TResult>, TResult>, FishTable<long, TResult>>(ref GetCacheRef)
	// 		.GetOrAddReference(key);

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static ref TResult GetExistingReference(int first, int second)
		=> ref Get.GetReference(new() { First = first, Second = second });

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static ref TResult GetExistingReference(T1 first, T2 second)
		=> ref Get.GetReference(new(first, second));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ByInt(T1 first, T2 second)
	{
		First = FunctionPointers.IndexGetter<T1>.Default(first);
		Second = FunctionPointers.IndexGetter<T2>.Default(second);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ByInt(int first, int second)
	{
		First = first;
		Second = second;
	}
	
	public int First, Second;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(ByInt<T1, T2, TResult> other)
		=> Unsafe.As<ByInt<T1, T2, TResult>, long>(ref this) == Unsafe.As<ByInt<T1, T2, TResult>, long>(ref other);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode() => HashCode.Combine(First, Second);
}

[PublicAPI]
public record struct ByInt<T1, T2, T3, TResult>
	where T1 : notnull where T2 : notnull where T3 : notnull where TResult : new()
{
	private static FishTable<ByInt<T1, T2, T3, TResult>, TResult> _get
		= Utility.AddNew<ByInt<T1, T2, T3, TResult>, TResult>();

	[ThreadStatic]
	private static FishTable<ByInt<T1, T2, T3, TResult>, TResult>? _getThreadStatic;

	public static FishTable<ByInt<T1, T2, T3, TResult>, TResult> Get
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _getThreadStatic ??= Utility.AddNew<ByInt<T1, T2, T3, TResult>, TResult>();
	}

	public static FishTable<ByInt<T1, T2, T3, TResult>, TResult> GetDirectly
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _get;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe ref TResult GetOrAddReference(int first, int second, int third)
	{
		var key = new ByInt<T1, T2, T3, TResult> { First = first, Second = second, Third = third };
		return ref Get.GetOrAddReference(ref key);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref TResult GetOrAddReference(in ByInt<T1, T2, T3, TResult> key)
		=> ref Get.GetOrAddReference(ref Unsafe.AsRef(in key));

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static ref TResult GetExistingReference(int first, int second, int third)
		=> ref Get.GetReference(new() { First = first, Second = second, Third = third });

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static ref TResult GetExistingReference(T1 first, T2 second, T3 third)
		=> ref Get.GetReference(new(first, second, third));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ByInt(T1 first, T2 second, T3 third)
	{
		First = FunctionPointers.IndexGetter<T1>.Default(first);
		Second = FunctionPointers.IndexGetter<T2>.Default(second);
		Third = FunctionPointers.IndexGetter<T3>.Default(third);
	}
	
	public int First, Second, Third;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(ByInt<T1, T2, T3, TResult> other)
		=> (Unsafe.As<ByInt<T1, T2, T3, TResult>, long>(ref this)
			== Unsafe.As<ByInt<T1, T2, T3, TResult>, long>(ref other))
		& (Third == other.Third);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode() => HashCode.Combine(First, Second, Third);
}

[PublicAPI]
public record struct ByInt<T1, T2, T3, T4, TResult>
	where T1 : notnull where T2 : notnull where T3 : notnull where T4 : notnull
	where TResult : new()
{
	private static FishTable<ByInt<T1, T2, T3, T4, TResult>, TResult> _get
		= Utility.AddNew<ByInt<T1, T2, T3, T4, TResult>, TResult>();

	[ThreadStatic]
	private static FishTable<ByInt<T1, T2, T3, T4, TResult>, TResult>? _getThreadStatic;

	public static FishTable<ByInt<T1, T2, T3, T4, TResult>, TResult> Get
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _getThreadStatic ??= Utility.AddNew<ByInt<T1, T2, T3, T4, TResult>, TResult>();
	}

	public static FishTable<ByInt<T1, T2, T3, T4, TResult>, TResult> GetDirectly
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _get;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe ref TResult GetOrAddReference(int first, int second, int third, int fourth)
	{
		var key = new ByInt<T1, T2, T3, T4, TResult>
		{
			First = first, Second = second, Third = third, Fourth = fourth
		};
		return ref Get.GetOrAddReference(ref key);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref TResult GetOrAddReference(in ByInt<T1, T2, T3, T4, TResult> key)
		=> ref Get.GetOrAddReference(ref Unsafe.AsRef(in key));

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static ref TResult GetExistingReference(int first, int second, int third, int fourth)
		=> ref Get.GetReference(new() { First = first, Second = second, Third = third, Fourth = fourth });

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static ref TResult GetExistingReference(T1 first, T2 second, T3 third, T4 fourth)
		=> ref Get.GetReference(new(first, second, third, fourth));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ByInt(T1 first, T2 second, T3 third, T4 fourth)
	{
		First = FunctionPointers.IndexGetter<T1>.Default(first);
		Second = FunctionPointers.IndexGetter<T2>.Default(second);
		Third = FunctionPointers.IndexGetter<T3>.Default(third);
		Fourth = FunctionPointers.IndexGetter<T4>.Default(fourth);
	}

	public int First, Second, Third, Fourth;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(ByInt<T1, T2, T3, T4, TResult> other)
		=> (Unsafe.As<ByInt<T1, T2, T3, T4, TResult>, long>(ref this)
				== Unsafe.As<ByInt<T1, T2, T3, T4, TResult>, long>(ref other))
			& (Unsafe.As<int, long>(ref Third) == Unsafe.As<int, long>(ref other.Third));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode() => HashCode.Combine(First, Second, Third, Fourth);
}
#pragma warning restore CS9091