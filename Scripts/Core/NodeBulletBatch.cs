using System;
using System.Collections.Generic;
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

    public void Spawn(PackedScene scene, Vector2 position, float angle)
    {
        var node = scene.Instantiate<BulletNode>();
        node.GlobalPosition = position;
        node.Angle = angle;
        _container.AddChild(node);
        _bullets.Add(node);
    }

    public void SpawnBullets(
        PackedScene scene,
        ReadOnlySpan<(Vector2 Position, float Angle)> bullets
    )
    {
        foreach (var (position, angle) in bullets)
        {
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

