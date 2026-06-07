using Godot;

[GlobalClass]
public abstract partial class DespawnCondition : Resource
{
    public abstract bool ShouldDespawn(Vector2 position, float angle, float lifetime);
}
