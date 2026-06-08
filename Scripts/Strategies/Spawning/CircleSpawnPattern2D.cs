using System;
using System.Numerics;

[Godot.GlobalClass]
public partial class CircleSpawnPattern2D : SpawnPattern2D
{
    [Godot.Export]
    public float Radius { get; set; } = 50f;

    public override int Execute(
        Span<System.Numerics.Vector2> positions,
        Span<System.Numerics.Vector2> velocities,
        Matrix3x2 worldMatrix
    )
    {
        int count = positions.Length;
        if (count == 0)
            return 0;

        float angleStep = MathF.Tau / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            var dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));

            positions[i] = Vector2.Transform(dir * Radius, worldMatrix);
            velocities[i] = dir;
        }
        return count;
    }
}
