using Godot;

[GlobalClass]
public partial class CompositePatternEntry2D : Resource
{
	[Export]
	public BulletPattern2D Pattern { get; set; }

	[Export]
	public Godot.Vector2 PositionOffset { get; set; } = Godot.Vector2.Zero;

	[Export]
	public float RotationOffset { get; set; } = 0f;

	[Export]
	public float Scale { get; set; } = 1f;

	[Export]
	public float SpawnDelay { get; set; } = 0f;
}
