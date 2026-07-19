using Godot;

/// <summary>
/// Visual Game Logic Layer — entry-point node that wires the UI button to the
/// <see cref="BulletSystem2D"/> spawner.
/// </summary>
/// <remarks>
/// Rule 3.2 / Rule 4: This node does not own simulation state.  Its only job is
/// to forward the button-pressed input event to <see cref="BulletSystem2D.SpawnPattern"/>
/// using the system's own world transform as the spawn origin.
/// </remarks>
public partial class Main : Node2D
{
	/// <summary>The bullet system driven by this demo scene.</summary>
	[Export] BulletSystem2D ServerController;

	/// <summary>
	/// Called by the scene's button signal.  Triggers a burst spawn at the
	/// system's current world position and rotation.
	/// </summary>
	public void OnButtonPressed()
	{
		ServerController.SpawnPattern(ServerController.GlobalPosition, ServerController.GlobalRotation);
	}
}
