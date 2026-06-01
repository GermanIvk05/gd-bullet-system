using Godot;

[GlobalClass]
public partial class OscillateMovementConfig : MovementConfig
{
    [Export]
    public float ForwardSpeed { get; set; } = 100f;

    [Export]
    public float OscillateSpeed { get; set; } = 2f;

    [Export]
    public float Amplitude { get; set; } = 50f;

    public override IMovementStrategy CreateStrategy() =>
        new OscillateMovementStrategy(ForwardSpeed, OscillateSpeed, Amplitude);
}

public class OscillateMovementStrategy(float forwardSpeed, float oscillateSpeed, float amplitude)
    : IMovementStrategy
{
    private float _forwardSpeed = forwardSpeed;
    private float _oscillateSpeed = oscillateSpeed;
    private float _amplitude = amplitude;

    public Vector2 Calculate(Vector2 position, float angle, float lifetime, float delta)
    {
        Vector2 forward = Vector2.FromAngle(angle);
        Vector2 perpendicular = forward.Orthogonal();

        float previousSin = Mathf.Sin((lifetime - delta) * _oscillateSpeed) * _amplitude;
        float currentSin = Mathf.Sin(lifetime * _oscillateSpeed) * _amplitude;
        return forward * _forwardSpeed * delta + perpendicular * (currentSin - previousSin);
    }
}
