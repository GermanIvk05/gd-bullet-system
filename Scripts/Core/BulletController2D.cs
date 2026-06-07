using System;
using System.Numerics;
using Godot;

[GlobalClass]
public partial class BulletController2D : Node2D
{
	private BulletBatch _batch;
	private Rid _space;

	[Export]
	public BulletConfig Config;

	[Export]
	public BulletView View;

	public override void _Ready()
	{
		_space = GetWorld2D().Space;
		_batch = new BulletBatch(
			_space,
			Config.Shape,
			Config.CollisionLayer,
			Config.CollisionMask,
			Config.Movement.CreateStrategy(),
			Config.DespawnConditions
		);
	}

	public void SpawnPattern(BulletPattern2D pattern, Godot.Vector2 position, float rotation)
	{
		var worldMatrix = BuildWorldMatrix(position, rotation);
		Span<Matrix3x2> buffer = pattern.BulletCount <= 128
			? stackalloc Matrix3x2[pattern.BulletCount]
			: new Matrix3x2[pattern.BulletCount];

		int count = pattern.FillBuffer(buffer, worldMatrix);
		_batch.SpawnBullets(buffer, count);
	}

	public override void _PhysicsProcess(double delta)
	{
		_batch.Update((float)delta);
		View.Update(_batch.GetPositions());
	}

	public override void _ExitTree()
	{
		_batch.Clear();
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
