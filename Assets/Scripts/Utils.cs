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
}
