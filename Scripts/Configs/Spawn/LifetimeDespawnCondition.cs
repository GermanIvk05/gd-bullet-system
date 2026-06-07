using Godot;

[GlobalClass]
public partial class LifetimeDespawnCondition : DespawnCondition
{
    [Export]
    public float MaxLifetime { get; set; }

    public override bool ShouldDespawn(System.Numerics.Vector2 position, float angle, float lifetime) =>
        lifetime >= MaxLifetime;
}
