using Godot;

[GlobalClass]
public partial class LinearMovementConfig : MovementConfig
{
    [Export]
    public float Speed { get; set; } = 100f;

    public override IMovementStrategy CreateStrategy() => new LinearMovementStrategy(Speed);
}

public class LinearMovementStrategy(float speed) : IMovementStrategy
{
    private readonly float _speed = speed;

    public Vector2 Calculate(Vector2 position, float angle, float lifetime, float delta)
    {
        return Vector2.FromAngle(angle) * _speed * delta;
    }
}
