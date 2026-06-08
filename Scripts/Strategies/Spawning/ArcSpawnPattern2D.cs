using System;
using System.Numerics;

[Godot.GlobalClass]
public partial class ArcSpawnPattern2D : SpawnPattern2D
{
    [Godot.Export]
    public float Radius { get; set; } = 50f;

    [Godot.Export]
    public float SpreadAngle { get; set; } = 90f;

    public override int Execute(
        Span<System.Numerics.Vector2> positions,
        Span<System.Numerics.Vector2> velocities,
        Matrix3x2 worldMatrix
    )
    {
        int count = positions.Length;
        if (count == 0)
            return 0;

        // Extract the target angle from the world matrix rotation
        float targetAngle = MathF.Atan2(worldMatrix.M12, worldMatrix.M11);

        if (count == 1)
        {
            var dir = new Vector2(MathF.Cos(targetAngle), MathF.Sin(targetAngle));
            positions[0] = Vector2.Transform(dir * Radius, worldMatrix);
            velocities[0] = dir;
            return 1;
        }

        float spreadRad = SpreadAngle * (MathF.PI / 180f);
        float halfSpread = spreadRad * 0.5f;
        float angleStep = spreadRad / (count - 1);

        for (int i = 0; i < count; i++)
        {
            float angle = targetAngle - halfSpread + i * angleStep;
            var dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));

            positions[i] = Vector2.Transform(dir * Radius, worldMatrix);
            velocities[i] = dir;
        }
        return count;
    }
}
