using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Godot;

[GlobalClass]
public partial class BulletSystem2D : Node2D
{
    [ExportGroup("Configuration")]
    [Export]
    public BulletMotion Movement { get; set; }

    [Export]
    public float MaxLifetime { get; set; } = 3.0f;

    [Export]
    public SpawnPattern2D Pattern { get; set; }

    [Export]
    public BulletRenderer2D View { get; set; }

    // Flat SoA Data arrays
    private System.Numerics.Vector2[] _positions;
    private System.Numerics.Vector2[] _velocities;
    private float[] _lifetimes;
    private int _activeCount;

    public int Capacity => _positions?.Length ?? 0;

    public override void _Ready()
    {
        int capacity = Pattern?.BulletCount ?? 1000;
        _positions = new System.Numerics.Vector2[capacity];
        _velocities = new System.Numerics.Vector2[capacity];
        _lifetimes = new float[capacity];
    }

    public void SpawnPattern(Godot.Vector2 position, float rotation)
    {
        if (Pattern == null || Movement == null)
            return;

        var worldMatrix = BuildWorldMatrix(position, rotation);

        int maxSpawn = Math.Min(Pattern.BulletCount, Capacity - _activeCount);
        if (maxSpawn == 0)
            return;

        var posSpan = _positions.AsSpan(_activeCount, maxSpawn);
        var velSpan = _velocities.AsSpan(_activeCount, maxSpawn);

        int count = Pattern.Execute(posSpan, velSpan, worldMatrix);

        _lifetimes.AsSpan(_activeCount, count).Clear();
        SimdMath.ApplySpeedAndRotation(
            velSpan.Slice(0, count),
            Movement.Speed,
            Movement.DirectionAngle
        );

        _activeCount += count;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_activeCount == 0)
            return;

        float d = (float)delta;
        var posSpan = _positions.AsSpan(0, _activeCount);
        var velSpan = _velocities.AsSpan(0, _activeCount);
        var lifeSpan = _lifetimes.AsSpan(0, _activeCount);

        // 1. Advance Time
        SimdMath.AddScalar(lifeSpan, d);

        // 2. Execute Motion
        Movement?.Execute(posSpan, velSpan, lifeSpan, d);

        // 3. Despawn Dead Bullets
        for (int i = _activeCount - 1; i >= 0; i--)
        {
            if (_lifetimes[i] >= MaxLifetime)
            {
                Despawn(i);
            }
        }

        // 4. Render
        if (View != null)
        {
            var activePosSpan = _positions.AsSpan(0, _activeCount);
            var godotPositions = MemoryMarshal.Cast<System.Numerics.Vector2, Godot.Vector2>(
                activePosSpan
            );
            View.Update(godotPositions);
        }
    }

    private void Despawn(int index)
    {
        int last = _activeCount - 1;
        if (index != last)
        {
            _positions[index] = _positions[last];
            _velocities[index] = _velocities[last];
            _lifetimes[index] = _lifetimes[last];
        }
        _activeCount--;
    }

    private static Matrix3x2 BuildWorldMatrix(Godot.Vector2 position, float rotation)
    {
        return Matrix3x2.CreateRotation(rotation)
            * Matrix3x2.CreateTranslation(position.X, position.Y);
    }
}
