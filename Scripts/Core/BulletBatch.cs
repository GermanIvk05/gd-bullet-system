using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Godot;

public class BulletBatch(
	Rid space,
	Shape2D shape,
	uint collisionLayer,
	uint collisionMask,
	IMovementStrategy strategy,
	DespawnCondition[] despawnConditions
)
{
	private readonly List<System.Numerics.Vector2> _positions = [];
	private readonly List<float> _angles = [];
	private readonly List<float> _lifetimes = [];
	private readonly List<Rid> _bodies = [];
	private Rid _space = space;
	private Shape2D _shape = shape;
	private uint _collisionLayer = collisionLayer;
	private uint _collisionMask = collisionMask;

	public int Count => _positions.Count;
	public IMovementStrategy MovementStrategy { get; set; } = strategy;
	public DespawnCondition[] DespawnConditions { get; set; } = despawnConditions;

	public void Spawn(System.Numerics.Vector2 position, float angle)
	{
		_positions.Add(position);
		_angles.Add(angle);
		_lifetimes.Add(0f);

		var body = PhysicsServer2D.BodyCreate();
		PhysicsServer2D.BodySetMode(body, PhysicsServer2D.BodyMode.Kinematic);
		PhysicsServer2D.BodyAddShape(body, _shape.GetRid());
		PhysicsServer2D.BodySetSpace(body, _space);
		PhysicsServer2D.BodySetCollisionLayer(body, _collisionLayer);
		PhysicsServer2D.BodySetCollisionMask(body, _collisionMask);

		var t = new Transform2D(0f, new Godot.Vector2(position.X, position.Y));
		PhysicsServer2D.BodySetState(body, PhysicsServer2D.BodyState.Transform, t);

		_bodies.Add(body);
	}

	/// <summary>
	/// Spawns bullets from an array of <see cref="Matrix3x2"/> transforms produced by a
	/// <see cref="BulletPattern2D"/>. Each matrix encodes position (M31, M32) and
	/// rotation (via M11, M12).
	/// </summary>
	public void SpawnBullets(ReadOnlySpan<Matrix3x2> transforms, int count)
	{
		int newCapacity = _positions.Count + count;
		_positions.Capacity = _angles.Capacity = _lifetimes.Capacity = _bodies.Capacity = newCapacity;
		for (int i = 0; i < count; i++)
		{
			ref readonly var m = ref transforms[i];
			var position = new System.Numerics.Vector2(m.M31, m.M32);
			float angle = MathF.Atan2(m.M12, m.M11);
			Spawn(position, angle);
		}
	}

	public void Update(float delta)
	{
		var positionsSpan = CollectionsMarshal.AsSpan(_positions);
		var anglesSpan = CollectionsMarshal.AsSpan(_angles);
		var lifetimesSpan = CollectionsMarshal.AsSpan(_lifetimes);
		var bodiesSpan = CollectionsMarshal.AsSpan(_bodies);

		for (int i = positionsSpan.Length - 1; i >= 0; i--)
		{
			ref var position = ref positionsSpan[i];
			ref var angle = ref anglesSpan[i];
			ref var lifetime = ref lifetimesSpan[i];
			ref var body = ref bodiesSpan[i];

			lifetime += delta;
			position += MovementStrategy.Calculate(
				position,
				angle,
				lifetime,
				delta
			);

			var t = new Transform2D(0f, new Godot.Vector2(position.X, position.Y));
			PhysicsServer2D.BodySetState(body, PhysicsServer2D.BodyState.Transform, t);

			bool shouldDespawn = false;
			foreach (var condition in DespawnConditions)
			{
				if (condition.ShouldDespawn(position, angle, lifetime))
				{
					shouldDespawn = true;
					break;
				}
			}

			if (shouldDespawn)
			{
				Despawn(i);
			}
		}
	}

	public void Despawn(int index)
	{
		PhysicsServer2D.FreeRid(_bodies[index]);
		
		_positions[index] = _positions[^1];
		_angles[index] = _angles[^1];
		_lifetimes[index] = _lifetimes[^1];
		_bodies[index] = _bodies[^1];
		
		_positions.RemoveAt(_positions.Count - 1);
		_angles.RemoveAt(_angles.Count - 1);
		_lifetimes.RemoveAt(_lifetimes.Count - 1);
		_bodies.RemoveAt(_bodies.Count - 1);
	}

	public void Clear()
	{
		foreach (var body in _bodies)
		{
			PhysicsServer2D.FreeRid(body);
		}
		_positions.Clear();
		_angles.Clear();
		_lifetimes.Clear();
		_bodies.Clear();
	}

	public ReadOnlySpan<Godot.Vector2> GetPositions()
	{
		return MemoryMarshal.Cast<System.Numerics.Vector2, Godot.Vector2>(CollectionsMarshal.AsSpan(_positions));
	}
}
