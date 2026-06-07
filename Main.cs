using Godot;

public partial class Main : Node2D
{
	[Export] BulletController ServerController;
	[Export] BulletPattern2D Pattern2D;

	public void OnButtonPressed()
	{
		if (Pattern2D != null)
		{
			ServerController.SpawnPattern(Pattern2D, ServerController.GlobalPosition, ServerController.GlobalRotation);
		}
	}
}
