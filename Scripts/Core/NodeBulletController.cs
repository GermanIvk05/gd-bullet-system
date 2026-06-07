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

	[Obsolete("Use SpawnPattern(BulletPattern2D, ...) instead.")]
	public override void SpawnPattern(BulletPattern pattern, Godot.Vector2 position, float rotation)
	{
		var spawnData = pattern.GetSpawnData(rotation);
		var bullets = new (Godot.Vector2, float)[spawnData.Length];
		for (int i = 0; i < spawnData.Length; i++)
		{
			bullets[i] = (
				position + spawnData[i].Position.Rotated(rotation),
				rotation + spawnData[i].Angle
			);
		}
		var batch = new NodeBulletBatch(
			_bulletContainer,
			Config.Movement.CreateStrategy(),
			Config.DespawnConditions
		);
		batch.SpawnBullets(_bulletScene, bullets);
		_batches.Add(batch);
	}

	public override void SpawnPattern(BulletPattern2D pattern, Godot.Vector2 position, float rotation)
	{
		var worldMatrix = BuildWorldMatrix(position, rotation);
		Span<Matrix3x2> buffer = pattern.BulletCount <= 128
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
