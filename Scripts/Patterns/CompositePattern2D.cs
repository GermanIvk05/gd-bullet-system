using System;
using System.Numerics;
using Godot;

[GlobalClass]
public partial class CompositePattern2D : BulletPattern2D
{
	[Export]
	public Godot.Collections.Array<CompositePatternEntry2D> Children { get; set; } = [];

	public override int BulletCount
	{
		get
		{
			int total = 0;
			if (Children != null)
			{
				foreach (var child in Children)
				{
					if (child != null && child.Pattern != null)
					{
						total += child.Pattern.BulletCount;
					}
				}
			}
			return total;
		}
		set { /* Required by Resource properties but dynamically evaluated in getter */ }
	}

	public override bool HasDelays
	{
		get
		{
			if (Children == null) return false;
			foreach (var child in Children)
			{
				if (child == null) continue;
				if (child.SpawnDelay > 0f) return true;
				if (child.Pattern != null && child.Pattern.HasDelays) return true;
			}
			return false;
		}
	}

	public override void Spawn(BulletController2D controller, Godot.Vector2 position, float rotation)
	{
		if (Children == null) return;

		// If this composite pattern (and all its subpatterns) has no delays,
		// we can batch-spawn it immediately in a single batch.
		if (!HasDelays)
		{
			controller.SpawnPatternImmediate(this, position, rotation);
			return;
		}

		// Otherwise, schedule delayed child spawns and run non-delayed immediately
		var parentWorldMatrix = Matrix3x2.CreateRotation(rotation)
		                      * Matrix3x2.CreateTranslation(position.X, position.Y);

		foreach (var child in Children)
		{
			if (child == null || child.Pattern == null) continue;

			var localMatrix = Matrix3x2.CreateScale(child.Scale)
			                * Matrix3x2.CreateRotation(child.RotationOffset)
			                * Matrix3x2.CreateTranslation(child.PositionOffset.X, child.PositionOffset.Y);

			var childWorldMatrix = localMatrix * parentWorldMatrix;
			var childWorldPos = new Godot.Vector2(childWorldMatrix.M31, childWorldMatrix.M32);
			float childWorldRot = MathF.Atan2(childWorldMatrix.M12, childWorldMatrix.M11);

			var pattern = child.Pattern;

			if (child.SpawnDelay > 0f)
			{
				float delay = child.SpawnDelay;
				controller.GetTree().CreateTimer(delay).Timeout += () =>
				{
					if (GodotObject.IsInstanceValid(controller) && controller.IsInsideTree())
					{
						controller.SpawnPattern(pattern, childWorldPos, childWorldRot);
					}
				};
			}
			else
			{
				pattern.Spawn(controller, childWorldPos, childWorldRot);
			}
		}
	}

	public override int FillBuffer(Span<Matrix3x2> buffer, Matrix3x2 worldMatrix)
	{
		int totalWritten = 0;
		if (Children == null) return 0;

		foreach (var child in Children)
		{
			if (child == null || child.Pattern == null) continue;
			if (child.SpawnDelay > 0f) continue; // Handled asynchronously via Spawn()

			var localMatrix = Matrix3x2.CreateScale(child.Scale)
			                * Matrix3x2.CreateRotation(child.RotationOffset)
			                * Matrix3x2.CreateTranslation(child.PositionOffset.X, child.PositionOffset.Y);

			var childWorldMatrix = localMatrix * worldMatrix;

			if (totalWritten >= buffer.Length) break;
			var subBuffer = buffer.Slice(totalWritten);
			int written = child.Pattern.FillBuffer(subBuffer, childWorldMatrix);
			totalWritten += written;
		}

		return totalWritten;
	}
}
