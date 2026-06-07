using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Godot;

public struct Bullet
{
    public Godot.Vector2 Position;
    public float Angle;
    public float Lifetime;
}

public class BulletBatch(
    Rid space,
    Shape2D shape,
    uint collisionLayer,
    uint collisionMask,
    IMovementStrategy strategy,
    DespawnCondition[] despawnConditions
)
{
    private readonly List<Bullet> _bullets = [];
    private readonly List<Rid> _bodies = [];
    private Rid _space = space;
    private Shape2D _shape = shape;
    private uint _collisionLayer = collisionLayer;
    private uint _collisionMask = collisionMask;

    public int Count => _bullets.Count;
    public IMovementStrategy MovementStrategy { get; set; } = strategy;
    public DespawnCondition[] DespawnConditions { get; set; } = despawnConditions;

    public void Spawn(Godot.Vector2 position, float angle)
    {
        _bullets.Add(new() { Position = position, Angle = angle });

        var body = PhysicsServer2D.BodyCreate();
        PhysicsServer2D.BodySetMode(body, PhysicsServer2D.BodyMode.Kinematic);
        PhysicsServer2D.BodyAddShape(body, _shape.GetRid());
        PhysicsServer2D.BodySetSpace(body, _space);
        PhysicsServer2D.BodySetCollisionLayer(body, _collisionLayer);
        PhysicsServer2D.BodySetCollisionMask(body, _collisionMask);

        var t = new Transform2D(0f, position);
        PhysicsServer2D.BodySetState(body, PhysicsServer2D.BodyState.Transform, t);

        _bodies.Add(body);
    }

    /// <summary>
    /// Spawns bullets from an array of <see cref="Matrix3x2"/> transforms produced by a
    /// <see cref="BulletPattern2D"/>. Each matrix encodes position (M31, M32) and
    /// rotation (via M11, M12).
    /// </summary>
    public void SpawnBullets(ReadOnlySpan<Matrix3x2> transforms, int count)
    {
        _bullets.Capacity = _bodies.Capacity = _bullets.Count + count;
        for (int i = 0; i < count; i++)
        {
            ref readonly var m = ref transforms[i];
            var position = new Godot.Vector2(m.M31, m.M32);
            float angle = MathF.Atan2(m.M12, m.M11);
            Spawn(position, angle);
        }
    }

    public void Update(float delta)
    {
        var span = CollectionsMarshal.AsSpan(_bullets);
        for (int i = span.Length - 1; i >= 0; i--)
        {
            ref var bullet = ref span[i];
            bullet.Lifetime += delta;
            bullet.Position += MovementStrategy.Calculate(
                bullet.Position,
                bullet.Angle,
                bullet.Lifetime,
                delta
            );

            var t = new Transform2D(0f, bullet.Position);
            PhysicsServer2D.BodySetState(_bodies[i], PhysicsServer2D.BodyState.Transform, t);

            bool shouldDespawn = false;
            foreach (var condition in DespawnConditions)
            {
                if (condition.ShouldDespawn(bullet.Position, bullet.Angle, bullet.Lifetime))
                {
                    shouldDespawn = true;
                    break;
                }
            }

            if (shouldDespawn)
            {
                Despawn(i);
            }
        }
    }

    public void Despawn(int index)
    {
        PhysicsServer2D.FreeRid(_bodies[index]);
        _bullets[index] = _bullets[^1];
        _bodies[index] = _bodies[^1];
        _bullets.RemoveAt(_bullets.Count - 1);
        _bodies.RemoveAt(_bodies.Count - 1);
    }

    public void Clear()
    {
        foreach (var body in _bodies)
        {
            PhysicsServer2D.FreeRid(body);
        }
        _bullets.Clear();
        _bodies.Clear();
    }

    public void CopyPositionsTo(Godot.Vector2[] array, int offset)
    {
        var span = CollectionsMarshal.AsSpan(_bullets);
        for (int i = 0; i < span.Length; i++)
        {
            array[offset + i] = span[i].Position;
        }
    }
}
