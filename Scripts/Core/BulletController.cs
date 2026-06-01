using Godot;

public abstract partial class BulletController : Node2D
{
    [Export]
    public BulletConfig Config;

    public abstract void SpawnPattern(BulletPattern pattern, Vector2 position, float rotation);

    public override void _ExitTree()
    {
        Cleanup();
    }

    protected abstract void Cleanup();
}

