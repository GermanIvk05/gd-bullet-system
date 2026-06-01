using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class ServerBulletController : BulletController
{
    private readonly List<BulletBatch> _batches = [];
    private Rid _space;

    [Export]
    public BulletView View;

    public override void _Ready()
    {
        _space = GetWorld2D().Space;
    }

    public override void SpawnPattern(BulletPattern pattern, Vector2 position, float rotation)
    {
        var spawnData = pattern.GetSpawnData(rotation);
        var bullets = new (Vector2, float)[spawnData.Length];
        for (int i = 0; i < spawnData.Length; i++)
        {
            bullets[i] = (
                position + spawnData[i].Position.Rotated(rotation),
                rotation + spawnData[i].Angle
            );
        }
        var batch = CreateBatch();
        batch.SpawnBullets(bullets);
    }

    private BulletBatch CreateBatch()
    {
        var batch = new BulletBatch(
            _space,
            Config.Shape,
            Config.CollisionLayer,
            Config.CollisionMask,
            Config.Movement.CreateStrategy(),
            Config.DespawnConditions
        );
        _batches.Add(batch);
        return batch;
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (var batch in _batches)
        {
            batch.Update((float)delta);
        }
        View.Update(GetPositions());
    }

    private Vector2[] GetPositions()
    {
        int total = 0;
        foreach (var batch in _batches)
        {
            total += batch.Count;
        }
        var positions = new Vector2[total];
        int offset = 0;
        foreach (var batch in _batches)
        {
            batch.CopyPositionsTo(positions, offset);
            offset += batch.Count;
        }
        return positions;
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

