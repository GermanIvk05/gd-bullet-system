using System;
using Godot;

[GlobalClass]
public partial class BulletConfig : Resource
{
    [ExportGroup("Combat")]
    [Export]
    public int Damage { get; set; } = 400;

    [Export]
    public Shape2D Shape { get; set; }

    [ExportGroup("Movement")]
    [Export]
    public MovementConfig Movement { get; set; }

    [Export]
    public DespawnCondition[] DespawnConditions { get; set; } = [];

    [ExportGroup("Collision")]
    [Export(PropertyHint.Layers2DPhysics)]
    public uint CollisionLayer { get; set; } = 0;

    [Export(PropertyHint.Layers2DPhysics)]
    public uint CollisionMask { get; set; } = 0;
}
