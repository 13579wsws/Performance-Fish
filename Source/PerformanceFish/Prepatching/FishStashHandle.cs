﻿// Copyright (c) 2023 bradson
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Runtime.InteropServices;

namespace PerformanceFish.Prepatching;

public class FishStashHandle : IDisposable
{
	private static readonly List<nint> _allHandles = new();
	
	public ref FishStash Stash => ref FishStash.FromHandle(Handle);

	public nint Handle { get; }

	public FishStashHandle(long handle)
	{
		if (_allHandles.Contains((nint)handle))
			ThrowHelper.ThrowInvalidOperationException("Tried creating duplicate FishStashHandle.");
		else
			_allHandles.Add((nint)handle);

		Handle = (nint)handle;
	}

	private void ReleaseUnmanagedResources()
	{
		Marshal.FreeHGlobal(Handle);
		_allHandles.Remove(Handle);
	}

	public void Dispose()
	{
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~FishStashHandle() => ReleaseUnmanagedResources();
}