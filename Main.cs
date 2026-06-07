using Godot;

public partial class Main : Node2D
{
	[Export] BulletController ServerController;
	[Export] BulletPattern Pattern;

	public void OnButtonPressed()
	{
		ServerController.SpawnPattern(Pattern, ServerController.GlobalPosition, ServerController.GlobalRotation);
	}
}
