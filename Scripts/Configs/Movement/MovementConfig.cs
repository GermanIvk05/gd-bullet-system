using Godot;

[GlobalClass]
public abstract partial class MovementConfig : Resource
{
    public abstract IMovementStrategy CreateStrategy();
}

public interface IMovementStrategy
{
    public System.Numerics.Vector2 Calculate(System.Numerics.Vector2 position, float angle, float lifetime, float delta);
}
