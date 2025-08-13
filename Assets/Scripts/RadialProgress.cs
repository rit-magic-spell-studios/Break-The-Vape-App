using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

// https://discussions.unity.com/t/how-to-best-to-do-a-radial-progress-circle/854069/23

public class RadialProgress : VisualElement {
    public new class UxmlTraits : VisualElement.UxmlTraits {
        // The progress property is exposed to UXML.
        UxmlFloatAttributeDescription progressAttribute = new UxmlFloatAttributeDescription( ) { name = "progress", defaultValue = 30 };

        UxmlFloatAttributeDescription widthAttribute = new UxmlFloatAttributeDescription( ) { name = "width", defaultValue = 25 };

        UxmlColorAttributeDescription backgroundColorAttribute = new UxmlColorAttributeDescription { name = "background-color", defaultValue = Color.grey };

        UxmlColorAttributeDescription fillColorAttribute = new UxmlColorAttributeDescription { name = "fill-color", defaultValue = Color.white };

        // The Init method is used to assign to the C# progress property from the value of the progress UXML
        // attribute.
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
            base.Init(ve, bag, cc);

            (ve as RadialProgress).Progress = progressAttribute.GetValueFromBag(bag, cc);
            (ve as RadialProgress).Width = widthAttribute.GetValueFromBag(bag, cc);
            (ve as RadialProgress).BackgroundColor = backgroundColorAttribute.GetValueFromBag(bag, cc);
            (ve as RadialProgress).FillColor = fillColorAttribute.GetValueFromBag(bag, cc);
        }
    }

    // A Factory class is needed to expose this control to UXML.
    public new class UxmlFactory : UxmlFactory<RadialProgress, UxmlTraits> {
    }

    public float Progress {
        get => progress;
        set {
            // Whenever the progress property changes, MarkDirtyRepaint() is named. This causes a call to the
            // generateVisualContents callback.
            progress = value;
            MarkDirtyRepaint( );
        }
    }

    public float Width {
        get => width;
        set {
            width = value;
            MarkDirtyRepaint( );
        }
    }

    public Color BackgroundColor {
        get => backgroundColor;
        set {
            backgroundColor = value;
            MarkDirtyRepaint( );
        }
    }

    public Color FillColor {
        get => fillColor;
        set {
            fillColor = value;
            MarkDirtyRepaint( );
        }
    }

    // This is the number of outer vertices to generate the circle.
    private const int numSteps = 200;

    // These are the meshes this control uses.
    private EllipseMesh backgroundMesh;
    private EllipseMesh fillMesh;
    private EllipseMesh circleStartMesh;
    private EllipseMesh circleEndMesh;

    private float progress;
    private float width;
    private Color backgroundColor;
    private Color fillColor;

    // This default constructor is RadialProgress's only constructor.
    public RadialProgress( ) {
        // Create meshes for the track and the progress.
        fillMesh = new EllipseMesh(numSteps);
        backgroundMesh = new EllipseMesh(numSteps);
        circleStartMesh = new EllipseMesh(numSteps);
        circleEndMesh = new EllipseMesh(numSteps);

        // Register a callback after custom style resolution.
        RegisterCallback<CustomStyleResolvedEvent>(evt => CustomStylesResolved(evt));

        // Register a callback to generate the visual content of the control.
        generateVisualContent += context => GenerateVisualContent(context);

        MarkDirtyRepaint( );
    }

    static void CustomStylesResolved(CustomStyleResolvedEvent evt) {
        RadialProgress element = (RadialProgress) evt.currentTarget;
        element.UpdateCustomStyles( );
    }

    // After the custom colors are resolved, this method uses them to color the meshes and (if necessary) repaint
    // the control.
    void UpdateCustomStyles( ) {
        fillMesh.Color = fillColor;
        backgroundMesh.Color = backgroundColor;
        circleStartMesh.Color = fillColor;
        circleEndMesh.Color = fillColor;

        if (fillMesh.IsDirty || backgroundMesh.IsDirty)
            MarkDirtyRepaint( );
    }

    // The GenerateVisualContent() callback method calls DrawMeshes().
    static void GenerateVisualContent(MeshGenerationContext context) {
        RadialProgress element = (RadialProgress) context.visualElement;
        element.DrawMeshes(context);
    }

    // DrawMeshes() uses the EllipseMesh utility class to generate an array of vertices and indices, for both the
    // "track" ring and the progress ring. It then passes the geometry to the MeshWriteData
    // object, as returned by the MeshGenerationContext.Allocate() method. For the "progress" mesh, only a slice of
    // the index arrays is used to progressively reveal parts of the mesh.
    void DrawMeshes(MeshGenerationContext context) {
        float halfWidth = contentRect.width * 0.5f;
        float halfHeight = contentRect.height * 0.5f;

        if (halfWidth < 2.0f || halfHeight < 2.0f)
            return;

        fillMesh.Width = halfWidth;
        fillMesh.Height = halfHeight;
        fillMesh.BorderSize = Width;
        fillMesh.UpdateMesh( );

        backgroundMesh.Width = halfWidth;
        backgroundMesh.Height = halfHeight;
        backgroundMesh.BorderSize = Width;
        backgroundMesh.UpdateMesh( );

        circleStartMesh.Width = width / 2;
        circleStartMesh.Height = width / 2;
        circleStartMesh.BorderSize = width;
        circleStartMesh.UpdateMesh( );

        circleEndMesh.Width = width / 2;
        circleEndMesh.Height = width / 2;
        circleEndMesh.BorderSize = width;
        circleEndMesh.UpdateMesh( );

        // Draw background mesh first
        var backgroundMeshWriteData = context.Allocate(backgroundMesh.Vertices.Length, backgroundMesh.Indices.Length);
        backgroundMeshWriteData.SetAllVertices(backgroundMesh.Vertices);
        backgroundMeshWriteData.SetAllIndices(backgroundMesh.Indices);

        // Keep progress between 0 and 100
        float clampedProgress = Mathf.Clamp(Progress, 0.0f, 100.0f);

        // Determine how many triangles are used to depending on fill, to achieve a partially filled circle
        int sliceSize = Mathf.Max(1, Mathf.FloorToInt((numSteps * clampedProgress) / 100.0f));

        // Every step is 6 indices in the corresponding array
        sliceSize *= 6;

        var fillMeshWriteData = context.Allocate(fillMesh.Vertices.Length, sliceSize);
        fillMeshWriteData.SetAllVertices(fillMesh.Vertices);

        var tempIndicesArray = new NativeArray<ushort>(fillMesh.Indices, Allocator.Temp);
        fillMeshWriteData.SetAllIndices(tempIndicesArray.Slice(0, sliceSize));
        tempIndicesArray.Dispose( );

        circleStartMesh.PositionOffset = GetOffset(0, halfWidth, halfHeight);
        circleStartMesh.UpdateMesh( );
        var circleStartMeshWriteData = context.Allocate(circleStartMesh.Vertices.Length, circleStartMesh.Indices.Length);
        circleStartMeshWriteData.SetAllVertices(circleStartMesh.Vertices);
        circleStartMeshWriteData.SetAllIndices(circleStartMesh.Indices);

        circleEndMesh.PositionOffset = GetOffset(Progress, halfWidth, halfHeight);
        circleEndMesh.UpdateMesh( );
        var circleEndMeshWriteData = context.Allocate(circleEndMesh.Vertices.Length, circleEndMesh.Indices.Length);
        circleEndMeshWriteData.SetAllVertices(circleEndMesh.Vertices);
        circleEndMeshWriteData.SetAllIndices(circleEndMesh.Indices);
    }

    private Vector2 GetOffset(float percent, float halfWidth, float halfHeight) {
        float angle = 180 - (percent / 100f) * 360f;
        float x = (1 + Mathf.Sin(angle * Mathf.Deg2Rad)) * (halfWidth - width / 2f);
        float y = (1 + Mathf.Cos(angle * Mathf.Deg2Rad)) * (halfHeight - width / 2f);

        return new Vector2(x, y);
    }
}