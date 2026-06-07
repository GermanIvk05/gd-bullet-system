using System;
using Godot;

[GlobalClass]
[Obsolete("Use ArcPattern2D instead.")]
public partial class ArcPattern : BulletPattern
{
    [Export]
    public int BulletCount { get; set; } = 5;

    [Export]
    public float Radius { get; set; } = 50f;

    [Export]
    public float SpreadAngle { get; set; } = 90f;

    public override SpawnData[] GetSpawnData(float targetAngle = 0f)
    {
        if (BulletCount <= 0)
        {
            return [];
        }

        if (BulletCount == 1)
        {
            return
            [
                new SpawnData
                {
                    Position = Vector2.FromAngle(targetAngle) * Radius,
                    Angle = targetAngle,
                },
            ];
        }

        float spreadRad = Mathf.DegToRad(SpreadAngle);
        float halfSpread = spreadRad / 2f;
        float angleStep = spreadRad / (BulletCount - 1);

        SpawnData[] spawns = new SpawnData[BulletCount];
        for (int i = 0; i < BulletCount; i++)
        {
            float angle = targetAngle - halfSpread + i * angleStep;

            spawns[i] = new SpawnData
            {
                Position = Vector2.FromAngle(angle) * Radius,
                Angle = angle,
            };
        }
        return spawns;
    }
}

