using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class VapeItem : MonoBehaviour {
    [SerializeField] private Rigidbody2D rigidBody2D;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PolygonCollider2D polygonCollider2D;
    [SerializeField, Range(0f, 10f)] private float minLaunchVelocity;
    [SerializeField, Range(0f, 10f)] private float maxLaunchVelocity;
    [SerializeField] private List<Sprite> sprites;

    private void Awake( ) {
        spriteRenderer.sprite = sprites[Random.Range(0, sprites.Count)];
        UpdatePolygonColliderShape( );
    }

    private void Start( ) {
        float cameraHalfHeight = Camera.main.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;
        float xDistance = Random.Range(cameraHalfWidth, cameraHalfWidth * 3f);
        float yDistance = cameraHalfHeight * 0.5f;

        float launchVelocity = Random.Range(minLaunchVelocity, maxLaunchVelocity);
        float launchAngle = CalculateLaunchAngle(xDistance, yDistance, launchVelocity);
        if (transform.position.x > 0) {
            // Reflect the launch angle over the vertical axis
            launchAngle = Mathf.PI - launchAngle;
        }
        Vector2 launchDirection = new Vector2(Mathf.Cos(launchAngle), Mathf.Sin(launchAngle));
        rigidBody2D.AddForce(launchDirection * launchVelocity, ForceMode2D.Impulse);
    }

    private void Update( ) {
        if (transform.position.y < -40f) {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Calculate the launch angle in radians based on a distance travelled, a starting height, and a starting velocity
    /// </summary>
    /// <param name="x">The distance that the object travelled</param>
    /// <param name="h">The initial height the object started at</param>
    /// <param name="vi">The initial velocity of the object</param>
    /// <returns>An angle in radians that is the starting launch angle of the object</returns>
    private float CalculateLaunchAngle(float x, float h, float vi) {
        // https://www.youtube.com/watch?v=bqYtNrhdDAY
        float a = (Mathf.Abs(Physics2D.gravity.y) * x * x) / (vi * vi);
        float b = Mathf.Sqrt((h * h) + (x * x));
        float c = Mathf.Acos(Mathf.Clamp((a - h) / b, -1, 1));
        float d = Mathf.Atan(x / h);
        float theta = (c + d) / 2f;
        return theta;
    }

    /// <summary>
    /// Manually update the shape of the polygon collider after a sprite has changed
    /// </summary>
    private void UpdatePolygonColliderShape( ) {
        polygonCollider2D.pathCount = spriteRenderer.sprite.GetPhysicsShapeCount( );

        List<Vector2> path = new List<Vector2>( );
        for (int i = 0; i < polygonCollider2D.pathCount; i++) {
            path.Clear( );
            spriteRenderer.sprite.GetPhysicsShape(i, path);
            polygonCollider2D.SetPath(i, path.ToArray( ));
        }
    }
}
