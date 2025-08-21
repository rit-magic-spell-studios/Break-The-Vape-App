using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LungCharacter : MonoBehaviour {
    [Header("Lung Character")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PolygonCollider2D polygonCollider2D;
    [SerializeField] private List<Sprite> healthStages;
    [SerializeField] private PuffDodgeController puffDodgeController;
    [Space]
    [SerializeField, Range(0.5f, 10f)] private float healthRegainSeconds;
    [SerializeField, Range(0f, 5f)] private float walkSpeed;
    [SerializeField, Range(0f, 10f)] private float minWalkSeconds;
    [SerializeField, Range(0f, 10f)] private float maxWalkSeconds;
    [SerializeField, Range(0f, 10f)] private float minIdleSeconds;
    [SerializeField, Range(0f, 10f)] private float maxIdleSeconds;
    [SerializeField] private LayerMask vapeItemLayer;
    [Space]
    [SerializeField] private int _health;

    private float healthRegainTimer;
    private GameObject lastObjectHit;

    private int walkDirection;
    private float stateSwitchTimer;
    private bool isWalking;

    private float cameraHalfWidth, cameraHalfHeight;

    /// <summary>
    /// The current health of the lungs
    /// </summary>
    public int Health {
        get => _health;
        set {
            _health = Mathf.Clamp(value, 0, healthStages.Count - 1);
            spriteRenderer.sprite = healthStages[_health];
            Utils.UpdatePolygonColliderShape(polygonCollider2D, spriteRenderer);
        }
    }

    private void Awake( ) {
        puffDodgeController = FindFirstObjectByType<PuffDodgeController>();

        cameraHalfHeight = Camera.main.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;

        Health = healthStages.Count - 1;
        lastObjectHit = null;

        walkDirection = 1;
        isWalking = true;
    }

    private void Update( ) {
        if (!puffDodgeController.IsPlayingGame) {
            return;
        }

        // Regain health over time
        healthRegainTimer += Time.deltaTime;
        if (healthRegainTimer >= healthRegainSeconds) {
            Health++;
            healthRegainTimer -= healthRegainSeconds;
        }

        // Switch states after the timer has reached 0
        stateSwitchTimer -= Time.deltaTime;
        if (stateSwitchTimer <= 0) {
            isWalking = !isWalking;
            if (isWalking) {
                stateSwitchTimer = Random.Range(minWalkSeconds, maxWalkSeconds);
            } else {
                stateSwitchTimer = Random.Range(minIdleSeconds, maxIdleSeconds);
            }
        }

        // While the lungs are walking, move their position
        // The lungs will walk slower the lower their health is
        if (isWalking) {
            Vector3 walkVector = walkDirection * Vector3.right;
            walkVector *= walkSpeed * ((Health + 1f) / healthStages.Count);
            transform.position += Time.deltaTime * walkVector;

            // Have the lung character bounce back and forth on the walls
            if (transform.position.x >= cameraHalfWidth * 0.5f || transform.position.x <= -cameraHalfWidth * 0.5f) {
                walkDirection *= -1;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        // When a vape item has collided with the lungs, damage its health
        if ((vapeItemLayer & (1 << collision.gameObject.layer)) > 0 && collision.gameObject != lastObjectHit) {
            Health--;
            healthRegainTimer = 0f;
            isWalking = true;
            stateSwitchTimer = 0f;
            lastObjectHit = collision.gameObject;
            SoundManager.Instance.PlaySoundEffect(SoundEffectType.LUNG_DAMAGE);
        }
    }
}
