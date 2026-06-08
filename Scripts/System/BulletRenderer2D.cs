using System;
using Godot;

[GlobalClass]
public partial class BulletRenderer2D : MultiMeshInstance2D
{
    private float[] _buffer = [];

    public override void _Ready()
    {
        Multimesh.CustomAabb = new Aabb(Vector3.One * -1e6f, Vector3.One * 2e6f);
    }

    public void Update(ReadOnlySpan<Vector2> positions)
    {
        UpdateCapacity(positions.Length);
        Multimesh.VisibleInstanceCount = positions.Length;

        if (positions.IsEmpty)
            return;

        int instanceCount = Multimesh.InstanceCount;
        int required = instanceCount * 8;
        if (_buffer.Length != required)
        {
            _buffer = new float[required];
        }

        for (int i = 0; i < positions.Length; i++)
        {
            int offset = i * 8;
            _buffer[offset + 0] = 1; // x.x
            _buffer[offset + 1] = 0; // y.x
            _buffer[offset + 2] = 0; // pad
            _buffer[offset + 3] = positions[i].X; // origin.x
            _buffer[offset + 4] = 0; // x.y
            _buffer[offset + 5] = 1; // y.y
            _buffer[offset + 6] = 0; // pad
            _buffer[offset + 7] = positions[i].Y; // origin.y
        }
        RenderingServer.MultimeshSetBuffer(Multimesh.GetRid(), _buffer);
    }

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
