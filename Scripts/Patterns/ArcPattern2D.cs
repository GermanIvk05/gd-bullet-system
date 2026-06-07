using System;
using System.Numerics;

[Godot.GlobalClass]
public partial class ArcPattern2D : BulletPattern2D
{
	[Godot.Export]
	public float Radius { get; set; } = 50f;

	[Godot.Export]
	public float SpreadAngle { get; set; } = 90f;

	public override int FillBuffer(Span<Matrix3x2> buffer, Matrix3x2 worldMatrix)
	{
		int count = System.Math.Min(BulletCount, buffer.Length);
		if (count == 0)
		{
			return 0;
		}

		// Extract the target angle from the world matrix rotation
		float targetAngle = MathF.Atan2(worldMatrix.M12, worldMatrix.M11);

		if (count == 1)
		{
			var localPos = new Vector2(MathF.Cos(targetAngle), MathF.Sin(targetAngle)) * Radius;
			var worldPos = Vector2.Transform(localPos, worldMatrix);

			buffer[0] = Matrix3x2.CreateRotation(targetAngle)
			          * Matrix3x2.CreateTranslation(worldPos);
			return 1;
		}

		float spreadRad = SpreadAngle * (MathF.PI / 180f);
		float halfSpread = spreadRad * 0.5f;
		float angleStep = spreadRad / (count - 1);

		for (int i = 0; i < count; i++)
		{
			float angle = targetAngle - halfSpread + i * angleStep;

			var localPos = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * Radius;
			var worldPos = Vector2.Transform(localPos, worldMatrix);

			buffer[i] = Matrix3x2.CreateRotation(angle)
			          * Matrix3x2.CreateTranslation(worldPos);
		}
		return count;
	}
}
