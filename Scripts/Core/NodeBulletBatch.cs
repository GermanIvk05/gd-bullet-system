using System;
using System.Collections.Generic;
using System.Numerics;
using Godot;

public class NodeBulletBatch(
    Node containter,
    IMovementStrategy strategy,
    DespawnCondition[] despawnConditions
)
{
    private readonly List<BulletNode> _bullets = [];
    private Node _container = containter;

    public IMovementStrategy MovementStrategy { get; set; } = strategy;
    public DespawnCondition[] DespawnConditions { get; set; } = despawnConditions;

    public void Spawn(PackedScene scene, Godot.Vector2 position, float angle)
    {
        var node = scene.Instantiate<BulletNode>();
        node.GlobalPosition = position;
        node.Angle = angle;
        _container.AddChild(node);
        _bullets.Add(node);
    }

    /// <summary>
    /// Spawns bullets from an array of <see cref="Matrix3x2"/> transforms produced by a
    /// <see cref="BulletPattern2D"/>. Each matrix encodes position (M31, M32) and
    /// rotation (via M11, M12).
    /// </summary>
    public void SpawnBullets(PackedScene scene, ReadOnlySpan<Matrix3x2> transforms, int count)
    {
        for (int i = 0; i < count; i++)
        {
            ref readonly var m = ref transforms[i];
            var position = new Godot.Vector2(m.M31, m.M32);
            float angle = MathF.Atan2(m.M12, m.M11);
            Spawn(scene, position, angle);
        }
    }

    public void Update(float delta)
    {
        for (int i = _bullets.Count - 1; i >= 0; i--)
        {
            var bullet = _bullets[i];
            bullet.Lifetime += delta;
            bullet.GlobalPosition += MovementStrategy.Calculate(
                bullet.GlobalPosition,
                bullet.Angle,
                bullet.Lifetime,
                delta
            );

            bool shouldDespawn = false;
            foreach (var condition in DespawnConditions)
            {
                if (condition.ShouldDespawn(bullet.GlobalPosition, bullet.Angle, bullet.Lifetime))
                {
                    shouldDespawn = true;
                    break;
                }
            }

            if (shouldDespawn)
            {
                bullet.QueueFree();
                _bullets[i] = _bullets[^1];
                _bullets.RemoveAt(_bullets.Count - 1);
            }
        }
    }

    public void Clear()
    {
        foreach (var bullet in _bullets)
        {
            bullet.QueueFree();
        }
        _bullets.Clear();
    }
}
