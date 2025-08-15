using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class VapeItem : MonoBehaviour {
    [SerializeField] private Rigidbody2D rigidBody2D;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PolygonCollider2D polygonCollider2D;
    [SerializeField] private PuffDodgeController puffDodgeController;
    [SerializeField, Range(0f, 20f)] private float minLaunchVelocity;
    [SerializeField, Range(0f, 20f)] private float maxLaunchVelocity;
    [SerializeField, Range(0f, 360f)] private float minLaunchAngularVelocity;
    [SerializeField, Range(0f, 360f)] private float maxLaunchAngularVelocity;
    [SerializeField] private List<Sprite> sprites;

    //float cameraHalfWidth, cameraHalfHeight;

    //private void Awake( ) {
    //    puffDodgeController = FindAnyObjectByType<PuffDodgeController>( );

    //    spriteRenderer.sprite = sprites[Random.Range(0, sprites.Count)];
    //    Utils.UpdatePolygonColliderShape(polygonCollider2D, spriteRenderer);
    //}

    //private void Start( ) {
    //    cameraHalfHeight = Camera.main.orthographicSize;
    //    cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;
    //    float xDistance = Random.Range(cameraHalfWidth, cameraHalfWidth * 3f);
    //    float yDistance = transform.position.y + cameraHalfHeight;

    //    float launchVelocity = Random.Range(minLaunchVelocity, maxLaunchVelocity);
    //    float launchAngle = CalculateLaunchAngle(xDistance, yDistance, launchVelocity);
    //    if (transform.position.x > 0) {
    //        // Reflect the launch angle over the vertical axis
    //        launchAngle = Mathf.PI - launchAngle;
    //    }

    //    rigidBody2D.velocity = new Vector2(Mathf.Cos(launchAngle), Mathf.Sin(launchAngle)) * launchVelocity;
    //    rigidBody2D.angularVelocity = Random.Range(minLaunchAngularVelocity, maxLaunchAngularVelocity);
    //}

    //private void Update( ) {
    //    if (transform.position.y <= -cameraHalfHeight * 2f) {
    //        puffDodgeController.VapeItems.Remove(gameObject);
    //        Destroy(gameObject);
    //    }
    //}

    ///// <summary>
    ///// Calculate the launch angle in radians based on a distance travelled, a starting height, and a starting velocity
    ///// </summary>
    ///// <param name="x">The distance that the object travelled</param>
    ///// <param name="h">The initial height the object started at</param>
    ///// <param name="vi">The initial velocity of the object</param>
    ///// <returns>An angle in radians that is the starting launch angle of the object</returns>
    //private float CalculateLaunchAngle(float x, float h, float vi) {
    //    // https://www.youtube.com/watch?v=bqYtNrhdDAY
    //    float a = (Mathf.Abs(Physics2D.gravity.y) * x * x) / (vi * vi);
    //    float b = Mathf.Sqrt((h * h) + (x * x));
    //    float c = Mathf.Acos(Mathf.Clamp((a - h) / b, -1, 1));
    //    float d = Mathf.Atan(x / h);
    //    float theta = (c + d) / 2f;
    //    return theta;
    //}
}
