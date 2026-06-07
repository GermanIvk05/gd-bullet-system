using System;
using Godot;

[Obsolete("Use BulletPattern2D instead. SpawnData will be removed in a future version.")]
public struct SpawnData
{
	public Vector2 Position;
	public float Angle;
}

[GlobalClass]
[Obsolete("Use BulletPattern2D instead. BulletPattern will be removed in a future version.")]
public abstract partial class BulletPattern : Resource
{
	public abstract SpawnData[] GetSpawnData(float targetAngle = 0f);
}
