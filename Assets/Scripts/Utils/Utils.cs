using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {
    /// <summary>
    /// Manually update the shape of the polygon collider after a sprite has changed
    /// </summary>
    public static void UpdatePolygonColliderShape(PolygonCollider2D polygonCollider2D, SpriteRenderer spriteRenderer) {
        polygonCollider2D.pathCount = spriteRenderer.sprite.GetPhysicsShapeCount( );

        List<Vector2> path = new List<Vector2>( );
        for (int i = 0; i < polygonCollider2D.pathCount; i++) {
            path.Clear( );
            spriteRenderer.sprite.GetPhysicsShape(i, path);
            polygonCollider2D.SetPath(i, path.ToArray( ));
        }
    }

    /// https://discussions.unity.com/t/clever-way-to-shuffle-a-list-t-in-one-line-of-c-code/535113
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    /// <param name="list">The list to shuffle</param>
    public static void Shuffle(IList list) {
        var count = list.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i) {
            var randomIndex = Random.Range(i, count);
            var tmp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = tmp;
        }
    }

    /// <summary>
    /// Calculate the launch angle in radians based on a distance travelled, a starting height, and a starting velocity
    /// </summary>
    /// <param name="x">The distance that the object travelled</param>
    /// <param name="h">The initial height the object started at</param>
    /// <param name="vi">The initial velocity of the object</param>
    /// <returns>An angle in radians that is the starting launch angle of the object</returns>
    public static float CalculateLaunchAngle(float x, float h, float vi) {
        // https://www.youtube.com/watch?v=bqYtNrhdDAY
        float a = (Mathf.Abs(Physics2D.gravity.y) * x * x) / (vi * vi);
        float b = Mathf.Sqrt((h * h) + (x * x));
        float c = Mathf.Acos(Mathf.Clamp((a - h) / b, -1, 1));
        float d = Mathf.Atan(x / h);
        float theta = (c + d) / 2f;
        return theta;
    }
}
