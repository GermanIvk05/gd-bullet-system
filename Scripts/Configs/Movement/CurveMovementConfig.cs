using Godot;

[GlobalClass]
public partial class CurveMovementConfig : MovementConfig
{
    [Export]
    public Curve SpeedCurve { get; set; }

    public override IMovementStrategy CreateStrategy() => new CurveMovementStrategy(SpeedCurve);
}

public class CurveMovementStrategy(Curve speedCurve) : IMovementStrategy
{
    private readonly Curve _speedCurve = speedCurve;

    public Vector2 Calculate(Vector2 position, float angle, float lifetime, float delta)
    {
        if (_speedCurve == null)
        {
            return Vector2.Zero;
        }
        float currentSpeed = _speedCurve.SampleBaked(lifetime);
        return Vector2.FromAngle(angle) * currentSpeed * delta;
    }
}
