using System;
using System.Collections.Generic;
using System.Numerics;
using Godot;

[GlobalClass]
public partial class NodeBulletController : BulletController
{
    [Export]
    private PackedScene _bulletScene;

    [Export]
    private Node _bulletContainer;

    private readonly List<NodeBulletBatch> _batches = [];

    public override void SpawnPattern(
        BulletPattern2D pattern,
        Godot.Vector2 position,
        float rotation
    )
    {
        var worldMatrix = BuildWorldMatrix(position, rotation);
        Span<Matrix3x2> buffer =
            pattern.BulletCount <= 128
                ? stackalloc Matrix3x2[pattern.BulletCount]
                : new Matrix3x2[pattern.BulletCount];

        int count = pattern.FillBuffer(buffer, worldMatrix);

        var batch = new NodeBulletBatch(
            _bulletContainer,
            Config.Movement.CreateStrategy(),
            Config.DespawnConditions
        );
        batch.SpawnBullets(_bulletScene, buffer, count);
        _batches.Add(batch);
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (var batch in _batches)
        {
            batch.Update((float)delta);
        }
    }

    protected override void Cleanup()
    {
        foreach (var batch in _batches)
        {
            batch.Clear();
        }
        _batches.Clear();
    }
}
