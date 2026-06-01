using Godot;

public struct SpawnData
{
    public Vector2 Position;
    public float Angle;
}

[GlobalClass]
public abstract partial class BulletPattern : Resource
{
    public abstract SpawnData[] GetSpawnData(float targetAngle = 0f);
}

