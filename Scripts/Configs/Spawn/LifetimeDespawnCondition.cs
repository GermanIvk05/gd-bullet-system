using Godot;

[GlobalClass]
public partial class LifetimeDespawnCondition : DespawnCondition
{
    [Export]
    public float MaxLifetime { get; set; }

    public override bool ShouldDespawn(Vector2 position, float angle, float lifetime) =>
        lifetime >= MaxLifetime;
}

