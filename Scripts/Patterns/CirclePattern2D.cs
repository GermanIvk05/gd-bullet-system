using System;
using System.Numerics;

[Godot.GlobalClass]
public partial class CirclePattern2D : BulletPattern2D
{
	[Godot.Export]
	public float Radius { get; set; } = 50f;

	public override int FillBuffer(Span<Matrix3x2> buffer, Matrix3x2 worldMatrix)
	{
		int count = System.Math.Min(BulletCount, buffer.Length);
		if (count == 0)
		{
			return 0;
		}

		float angleStep = MathF.Tau / count;

		for (int i = 0; i < count; i++)
		{
			float angle = i * angleStep;

			var localPos = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * Radius;
			var worldPos = Vector2.Transform(localPos, worldMatrix);

			buffer[i] = Matrix3x2.CreateRotation(angle) * Matrix3x2.CreateTranslation(worldPos);
		}
		return count;
	}
}
