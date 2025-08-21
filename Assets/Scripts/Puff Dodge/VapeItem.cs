using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class VapeItem : MonoBehaviour {
    [SerializeField] private Rigidbody2D rigidBody2D;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PolygonCollider2D polygonCollider2D;
    [SerializeField] private PuffDodgeController puffDodgeController;
    [SerializeField] private GameObject vapePoofParticlesPrefab;
    [SerializeField, Range(0f, 20f)] private float minLaunchVelocity;
    [SerializeField, Range(0f, 20f)] private float maxLaunchVelocity;
    [SerializeField, Range(0f, 360f)] private float minLaunchAngularVelocity;
    [SerializeField, Range(0f, 360f)] private float maxLaunchAngularVelocity;
    [SerializeField, Range(0, 100)] private int vapeDestroyPoints;
    [SerializeField] private List<Sprite> sprites;
    [SerializeField] private List<Color> colors;

    private float cameraHalfWidth, cameraHalfHeight;
    private int spriteIndex;

    private void Awake( ) {
        puffDodgeController = FindAnyObjectByType<PuffDodgeController>( );

        spriteIndex = Random.Range(0, sprites.Count);
        spriteRenderer.sprite = sprites[spriteIndex];
        Utils.UpdatePolygonColliderShape(polygonCollider2D, spriteRenderer);
    }

    private void Start( ) {
        cameraHalfHeight = Camera.main.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;
        float xDistance = Random.Range(cameraHalfWidth, cameraHalfWidth * 3f);
        float yDistance = transform.position.y + cameraHalfHeight;

        float launchVelocity = Random.Range(minLaunchVelocity, maxLaunchVelocity);
        float launchAngle = Utils.CalculateLaunchAngle(xDistance, yDistance, launchVelocity);
        if (transform.position.x > 0) {
            // Reflect the launch angle over the vertical axis
            launchAngle = Mathf.PI - launchAngle;
        }

        rigidBody2D.velocity = new Vector2(Mathf.Cos(launchAngle), Mathf.Sin(launchAngle)) * launchVelocity;
        rigidBody2D.angularVelocity = Random.Range(minLaunchAngularVelocity, maxLaunchAngularVelocity);
    }

    private void Update( ) {
        // When the vape item is below the bottom of the screen, destroy it
        if (transform.position.y <= -cameraHalfHeight * 2f) {
            puffDodgeController.VapeItems.Remove(gameObject);
            Destroy(gameObject);
        }
    }

    public void Slice( ) {
        // Update the controller variables
        puffDodgeController.VapeItems.Remove(gameObject);
        puffDodgeController.DestroyedItems++;
        puffDodgeController.AddPoints(transform.position, vapeDestroyPoints);
        SoundManager.Instance.PlaySoundEffect(SoundEffectType.VAPE_BROKEN);

        // Spawn poof particles when this vape item is destroyed
        ParticleSystem vapeParticles = Instantiate(vapePoofParticlesPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>( );
        ParticleSystem.MainModule main = vapeParticles.main;
        main.startColor = colors[spriteIndex];
        vapeParticles.Play( );

        // Destroy this object
        Destroy(gameObject);
    }
}
