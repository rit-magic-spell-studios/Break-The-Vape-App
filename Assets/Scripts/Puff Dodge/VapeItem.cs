using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class VapeItem : MonoBehaviour {
    [SerializeField] private Rigidbody2D rigidBody2D;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PolygonCollider2D polygonCollider2D;
    [SerializeField] private PuffDodgeController puffDodgeController;
    [SerializeField] private LungCharacter lungCharacter;
    [SerializeField] private GameObject vapePoofParticlesPrefab;
    [SerializeField, Range(0f, 20f)] private float minLaunchVelocity;
    [SerializeField, Range(0f, 20f)] private float maxLaunchVelocity;
    [SerializeField, Range(0f, 360f)] private float minLaunchAngularVelocity;
    [SerializeField, Range(0f, 360f)] private float maxLaunchAngularVelocity;
    [SerializeField, Range(0, 100)] private int vapeDestroyPoints;
    [SerializeField, Range(0, 20)] private int vapeMinPoints;
    [SerializeField, Range(0, 20)] private int lungHitPointDecrease;
    [SerializeField, Range(0f, 5f)] private float doublePointsDistance;
    [SerializeField] private List<Sprite> sprites;
    [SerializeField] private List<Color> colors;

    private float cameraHalfWidth, cameraHalfHeight;
    private int spriteIndex;

    private void Awake( ) {
        puffDodgeController = FindAnyObjectByType<PuffDodgeController>( );
        lungCharacter = FindAnyObjectByType<LungCharacter>( );

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
            puffDodgeController.VapeItems.Remove(this);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Destroy this vape item with a slice. This spawns particle effects and gives points to the player
    /// </summary>
    /// <param name="givePoints">Whether or not to give points to the player in this function</param>
    public void Slice(bool givePoints = true) {
        puffDodgeController.VapeItems.Remove(this);
        if (givePoints) {
            puffDodgeController.DestroyedItems++;
            SoundManager.Instance.PlaySoundEffect(SoundEffectType.VAPE_BROKEN);

            // Give double points for destroying a vape item close to the lung character
            int pointMult = 1;
            if (Vector2.Distance(lungCharacter.transform.position, transform.position) <= doublePointsDistance) {
                pointMult = 2;
            }
            int gainedPoints = Mathf.Max((vapeDestroyPoints - (lungCharacter.TotalHits * lungHitPointDecrease)) * pointMult, vapeMinPoints);
            puffDodgeController.AddPoints(transform.position, gainedPoints);
        }

        // Spawn poof particles when this vape item is destroyed
        ParticleSystem vapeParticles = Instantiate(vapePoofParticlesPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>( );
        ParticleSystem.MainModule main = vapeParticles.main;
        main.startColor = colors[spriteIndex];
        vapeParticles.Play( );

        // Destroy this object
        Destroy(gameObject);
    }
}
