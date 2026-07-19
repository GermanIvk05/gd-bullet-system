using System;
using Godot;

/// <summary>
/// Visual Layer — Renders all active bullets in a single draw call via MultiMesh.
/// This component is stateless with respect to game logic: it only accepts a
/// pre-built transform buffer and pushes it to the GPU.
/// </summary>
/// <remarks>
/// Per Rule 6.3: the only operation permitted in the render path is writing the
/// caller-supplied <c>ReadOnlySpan&lt;float&gt;</c> transform buffer to the GPU via
/// <c>RenderingServer.MultimeshSetBuffer</c>. Buffer construction is the caller's
/// responsibility (<see cref="BulletSystem2D"/>).
/// </remarks>
public partial class BulletRenderer2D : MultiMeshInstance2D
{
    // Internal staging array — avoids a heap allocation every frame.
    private float[] _buffer = [];

    public override void _Ready()
    {
        Multimesh.CustomAabb = new Aabb(Vector3.One * -1e6f, Vector3.One * 2e6f);
    }

    public override void _ExitTree()
    {
        // Release the staging buffer so the GC can reclaim it when the renderer leaves
        // the scene tree (Rule 3.4: cleanup in _ExitTree).
        _buffer = [];
    }

    /// <summary>
    /// Submits a pre-built transform buffer to the GPU and updates the visible
    /// instance count.  Each instance occupies 8 floats in row-major Transform2D
    /// layout: [ xx, yx, pad, ox, xy, yy, pad, oy ].
    /// </summary>
    /// <param name="transformBuffer">
    /// Flat array of 8 floats per bullet instance.  Must be exactly
    /// <c>instanceCount * 8</c> floats long, or empty to hide all instances.
    /// </param>
    public void Update(ReadOnlySpan<float> transformBuffer)
    {
        int instanceCount = transformBuffer.Length / 8;
        UpdateCapacity(instanceCount);
        Multimesh.VisibleInstanceCount = instanceCount;

        if (transformBuffer.IsEmpty)
            return;

        // Ensure the staging array matches the full multimesh size (not just visible).
        int required = Multimesh.InstanceCount * 8;
        if (_buffer.Length != required)
        {
            _buffer = new float[required];
        }

        // Copy only the visible portion of the buffer; the remainder is stale but
        // invisible due to VisibleInstanceCount.
        transformBuffer.CopyTo(_buffer.AsSpan(0, transformBuffer.Length));

        RenderingServer.MultimeshSetBuffer(Multimesh.GetRid(), _buffer);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private void UpdateCapacity(int count)
    {
        int capacity = Multimesh.InstanceCount;
        if (count > capacity)
        {
            Multimesh.InstanceCount = Mathf.Max(count, capacity * 2);
        }
        else if (count < capacity / 4)
        {
            Multimesh.InstanceCount = Mathf.Max(count, capacity / 2);
        }
    }
}
