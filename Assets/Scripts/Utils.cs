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
}
