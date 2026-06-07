using System;
using System.Collections.Generic;
using System.Numerics;
using Godot;

[GlobalClass]
public partial class BulletController2D : Node2D
{
	private readonly List<BulletBatch> _batches = [];
	private Rid _space;

	[Export]
	public BulletConfig Config;

	[Export]
	public BulletView View;

	public override void _Ready()
	{
		_space = GetWorld2D().Space;
	}

	public void SpawnPattern(BulletPattern2D pattern, Godot.Vector2 position, float rotation)
	{
		pattern.Spawn(this, position, rotation);
	}

	public void SpawnPatternImmediate(BulletPattern2D pattern, Godot.Vector2 position, float rotation)
	{
		var worldMatrix = BuildWorldMatrix(position, rotation);
		Span<Matrix3x2> buffer = pattern.BulletCount <= 128
			? stackalloc Matrix3x2[pattern.BulletCount]
			: new Matrix3x2[pattern.BulletCount];

		int count = pattern.FillBuffer(buffer, worldMatrix);

		var batch = CreateBatch();
		batch.SpawnBullets(buffer, count);
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

	private Godot.Vector2[] GetPositions()
	{
		int total = 0;
		foreach (var batch in _batches)
		{
			total += batch.Count;
		}
		var positions = new Godot.Vector2[total];
		int offset = 0;
		foreach (var batch in _batches)
		{
			batch.CopyPositionsTo(positions, offset);
			offset += batch.Count;
		}
		return positions;
	}

	public override void _ExitTree()
	{
		Cleanup();
	}

	private void Cleanup()
	{
		foreach (var batch in _batches)
		{
			batch.Clear();
		}
		_batches.Clear();
	}

	/// <summary>
	/// Builds a <see cref="Matrix3x2"/> world matrix from the Godot position and rotation.
	/// </summary>
	private static Matrix3x2 BuildWorldMatrix(Godot.Vector2 position, float rotation)
	{
		return Matrix3x2.CreateRotation(rotation)
		     * Matrix3x2.CreateTranslation(position.X, position.Y);
	}
}
