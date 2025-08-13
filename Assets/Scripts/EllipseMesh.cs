using UnityEngine;
using UnityEngine.UIElements;

public class EllipseMesh {
    public Vertex[ ] Vertices { get; private set; }
    public ushort[ ] Indices { get; private set; }
    public bool IsDirty => isDirty;

    public float Width {
        get => width;
        set => CompareAndWrite(ref width, value);
    }

    public float Height {
        get => height;
        set => CompareAndWrite(ref height, value);
    }

    public Color Color {
        get => color;
        set {
            isDirty = value != color;
            color = value;
        }
    }

    public float BorderSize {
        get => borderSize;
        set => CompareAndWrite(ref borderSize, value);
    }

    public Vector2 PositionOffset {
        get => positionOffset;
        set => CompareAndWrite(ref positionOffset, value);
    }

    private float width;
    private float height;
    private Color color;
    private float borderSize;
    private Vector2 positionOffset;
    private int numSteps;
    private bool isDirty;

    public EllipseMesh(int numSteps) {
        this.numSteps = numSteps;
        isDirty = true;
    }

    public void UpdateMesh( ) {
        if (!isDirty)
            return;

        int numVertices = numSteps * 2;
        int numIndices = numVertices * 6;

        if (Vertices == null || Vertices.Length != numVertices)
            Vertices = new Vertex[numVertices];

        if (Indices == null || Indices.Length != numIndices)
            Indices = new ushort[numIndices];

        float stepSize = 360.0f / (float) numSteps;
        float angle = -180.0f;

        for (int i = 0; i < numSteps; i++) {
            angle -= stepSize;
            float radians = Mathf.Deg2Rad * angle;

            float outerX = Mathf.Sin(radians) * Width;
            float outerY = Mathf.Cos(radians) * Height;
            Vertex outerVertex = new Vertex {
                position = new Vector3(Width + outerX + positionOffset.x, Height + outerY + positionOffset.y, Vertex.nearZ),
                tint = Color
            };
            Vertices[i * 2] = outerVertex;

            float innerX = Mathf.Sin(radians) * (Width - BorderSize);
            float innerY = Mathf.Cos(radians) * (Height - BorderSize);
            Vertex innerVertex = new Vertex {
                position = new Vector3(Width + innerX + positionOffset.x, Height + innerY + positionOffset.y, Vertex.nearZ),
                tint = Color
            };
            Vertices[i * 2 + 1] = innerVertex;

            Indices[i * 6] = (ushort) ((i == 0) ? Vertices.Length - 2 : (i - 1) * 2); // previous outer vertex
            Indices[i * 6 + 1] = (ushort) (i * 2); // current outer vertex
            Indices[i * 6 + 2] = (ushort) (i * 2 + 1); // current inner vertex

            Indices[i * 6 + 3] = (ushort) ((i == 0) ? Vertices.Length - 2 : (i - 1) * 2); // previous outer vertex
            Indices[i * 6 + 4] = (ushort) (i * 2 + 1); // current inner vertex
            Indices[i * 6 + 5] =
                (ushort) ((i == 0) ? Vertices.Length - 1 : (i - 1) * 2 + 1); // previous inner vertex
        }

        isDirty = false;
    }

    private void CompareAndWrite(ref float field, float newValue) {
        if (Mathf.Abs(field - newValue) > float.Epsilon) {
            isDirty = true;
            field = newValue;
        }
    }

    private void CompareAndWrite(ref Vector2 field, Vector2 newValue) {
        if (Vector2.SqrMagnitude(field - newValue) > float.Epsilon) {
            isDirty = true;
            field = newValue;
        }
    }
}