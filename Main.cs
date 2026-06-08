using Godot;

public partial class Main : Node2D
{
	[Export] BulletSystem2D ServerController;

	public void OnButtonPressed()
	{
		ServerController.SpawnPattern(ServerController.GlobalPosition, ServerController.GlobalRotation);
	}
}
