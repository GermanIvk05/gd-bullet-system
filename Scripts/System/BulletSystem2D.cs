using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Godot;

/// <summary>
/// Visual Game Logic Layer — owns the bullet simulation data arrays and drives the
/// per-frame process loop.
/// </summary>
/// <remarks>
/// Architecture rules enforced here:
/// <list type="bullet">
///   <item>Rule 4.1: Owns all flat SoA arrays; no other class holds references.</item>
///   <item>Rule 4.2: Delegates motion and spawn entirely to injected strategy resources.</item>
///   <item>Rule 4.3: No ad-hoc branching for bullet types — all variation lives in strategies.</item>
///   <item>Rule 4.4: Releases array references and disconnects from View in _ExitTree.</item>
///   <item>Rule 6.2: Passes a pre-built float transform buffer to BulletRenderer2D.</item>
/// </remarks>
public partial class BulletSystem2D : Node2D
{
    // -------------------------------------------------------------------------
    // Exported configuration (set in the Godot Inspector)
    // -------------------------------------------------------------------------

    [ExportGroup("Configuration")]
    [Export]
    public BulletMotion Movement { get; set; }

    [Export]
    public float MaxLifetime { get; set; } = 3.0f;

    [Export]
    public SpawnPattern2D Pattern { get; set; }

    /// <summary>
    /// The renderer node that owns the MultiMeshInstance2D.
    /// BulletSystem2D passes a pre-built transform buffer to it each frame.
    /// </summary>
    [Export]
    public BulletRenderer2D View { get; set; }

    // -------------------------------------------------------------------------
    // Private simulation state — Rule 4.1: only this class may read/write these.
    // -------------------------------------------------------------------------

    private Vector2[] _positions;
    private Vector2[] _velocities;
    private float[] _lifetimes;

    /// <summary>
    /// Per-frame staging buffer for the 8-float-per-instance GPU transform data.
    /// Built here (Rule 6.3) and passed as a ReadOnlySpan to BulletRenderer2D.
    /// </summary>
    private float[] _transformBuffer = [];

    private int _activeCount;

    /// <summary>The maximum number of concurrent bullets this system can hold.</summary>
    public int Capacity => _positions?.Length ?? 0;

    // -------------------------------------------------------------------------
    // Godot lifecycle
    // -------------------------------------------------------------------------

    public override void _Ready()
    {
        int capacity = Pattern?.BulletCount ?? 1000;
        _positions = new Vector2[capacity];
        _velocities = new Vector2[capacity];
        _lifetimes = new float[capacity];
    }

    public override void _ExitTree()
    {
        // Rule 4.4: Release all array references when leaving the scene tree so the
        // GC can reclaim them.  Also clear the View reference to prevent stale-handler
        // errors if the renderer is removed before this node.
        _positions = null;
        _velocities = null;
        _lifetimes = null;
        _transformBuffer = [];
        _activeCount = 0;
        View = null;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Spawns a burst of bullets using the configured <see cref="Pattern"/> and
    /// <see cref="Movement"/> strategies.
    /// </summary>
    /// <param name="position">World-space spawn origin.</param>
    /// <param name="rotation">World-space rotation of the spawner (radians).</param>
    public void SpawnPattern(Godot.Vector2 position, float rotation)
    {
        if (Pattern == null || Movement == null)
            return;

        var worldMatrix = BuildWorldMatrix(position, rotation);

        int maxSpawn = Math.Min(Pattern.BulletCount, Capacity - _activeCount);
        if (maxSpawn == 0)
            return;

        var posSpan = _positions.AsSpan(_activeCount, maxSpawn);
        var velSpan = _velocities.AsSpan(_activeCount, maxSpawn);

        int count = Pattern.Execute(posSpan, velSpan, worldMatrix);

        _lifetimes.AsSpan(_activeCount, count).Clear();
        SimdMath.ApplySpeedAndRotation(
            velSpan.Slice(0, count),
            Movement.Speed,
            Movement.DirectionAngle
        );

        _activeCount += count;
    }

    // -------------------------------------------------------------------------
    // Process loop (Visual Game Logic Layer driver)
    // -------------------------------------------------------------------------

    public override void _PhysicsProcess(double delta)
    {
        if (_activeCount == 0)
            return;

        float d = (float)delta;
        var posSpan = _positions.AsSpan(0, _activeCount);
        var velSpan = _velocities.AsSpan(0, _activeCount);
        var lifeSpan = _lifetimes.AsSpan(0, _activeCount);

        // 1. Advance time for all active bullets.
        SimdMath.AddScalar(lifeSpan, d);

        // 2. Delegate motion to the injected strategy (Rule 4.2, 4.3).
        Movement?.Execute(posSpan, velSpan, lifeSpan, d);

        // 3. Despawn bullets that have exceeded their lifetime (swap-with-last).
        for (int i = _activeCount - 1; i >= 0; i--)
        {
            if (_lifetimes[i] >= MaxLifetime)
            {
                Despawn(i);
            }
        }

        // 4. Build the GPU transform buffer then hand it to the renderer.
        //    Rule 6.2/6.3: BulletRenderer2D receives a ReadOnlySpan<float> only.
        if (View != null)
        {
            var activePosSpan = _positions.AsSpan(0, _activeCount);
            EnsureTransformBuffer(_activeCount);
            BuildTransformBuffer(activePosSpan, _transformBuffer.AsSpan(0, _activeCount * 8));
            View.Update(_transformBuffer.AsSpan(0, _activeCount * 8));
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>Swaps the bullet at <paramref name="index"/> with the last active bullet
    /// and decrements the active count (O(1) unordered removal).</summary>
    private void Despawn(int index)
    {
        int last = _activeCount - 1;
        if (index != last)
        {
            _positions[index] = _positions[last];
            _velocities[index] = _velocities[last];
            _lifetimes[index] = _lifetimes[last];
        }
        _activeCount--;
    }

    /// <summary>
    /// Grows <see cref="_transformBuffer"/> if needed to hold all active bullets.
    /// Each bullet requires 8 floats (2×4 Transform2D row-major layout).
    /// </summary>
    private void EnsureTransformBuffer(int activeCount)
    {
        int required = activeCount * 8;
        if (_transformBuffer.Length < required)
        {
            // Over-allocate to amortize resizing (capacity * 2 like the renderer).
            _transformBuffer = new float[Math.Max(required, _transformBuffer.Length * 2)];
        }
    }

    /// <summary>
    /// Writes one identity-scale Transform2D entry per bullet into <paramref name="buffer"/>.
    /// Layout per instance (8 floats):
    /// [ xx=1, yx=0, pad=0, ox, xy=0, yy=1, pad=0, oy ]
    /// </summary>
    private static void BuildTransformBuffer(
        ReadOnlySpan<Vector2> positions,
        Span<float> buffer)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            int offset = i * 8;
            buffer[offset + 0] = 1f;               // column 0, row 0 (x.x)
            buffer[offset + 1] = 0f;               // column 0, row 1 (y.x)
            buffer[offset + 2] = 0f;               // padding
            buffer[offset + 3] = positions[i].X;   // origin.x
            buffer[offset + 4] = 0f;               // column 1, row 0 (x.y)
            buffer[offset + 5] = 1f;               // column 1, row 1 (y.y)
            buffer[offset + 6] = 0f;               // padding
            buffer[offset + 7] = positions[i].Y;   // origin.y
        }
    }

    private static Matrix3x2 BuildWorldMatrix(Godot.Vector2 position, float rotation)
    {
        return Matrix3x2.CreateRotation(rotation)
            * Matrix3x2.CreateTranslation(position.X, position.Y);
    }
}
