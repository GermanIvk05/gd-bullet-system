using Godot;

[GlobalClass]
public partial class CirclePattern : BulletPattern
{
    [Export]
    public int BulletCount { get; set; } = 10;

    [Export]
    public float Radius { get; set; } = 50f;

    public override SpawnData[] GetSpawnData(float targetAngle = 0f)
    {
        SpawnData[] spawns = new SpawnData[BulletCount];
        float angleStep = Mathf.Tau / BulletCount;

        for (int i = 0; i < BulletCount; i++)
        {
            float angle = i * angleStep;

            spawns[i] = new SpawnData
            {
                Position = Vector2.FromAngle(angle) * Radius,
                Angle = angle,
            };
        }
        return spawns;
    }
}

