using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CraveMonster : MonoBehaviour {
    [SerializeField] private CraveSmashController craveSmashController;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<Sprite> monsterSprites;
    [Space]
    [SerializeField, Range(0, 100)] private float clickDamage;
    [SerializeField, Range(0, 50)] private int baseClickPoints;
    [SerializeField, Range(0, 50)] private float healSpeed;
    [SerializeField, Range(0f, 4f)] private float moveRadius;
    [SerializeField, Range(0f, 1f)] private float moveSpeed;

    private float lastClickTime;
    private Vector3 startingPosition;
    private Vector3 toPosition;
    private Vector3 positionVelocity;

    /// <summary>
    /// Whether or not this crave monster is dead
    /// </summary>
    public bool IsDead { get; private set; }

    /// <summary>
    /// The current health of the crave monster
    /// </summary>
    public float Health {
        get => _health;
        set {
            _health = Mathf.Clamp(value, 0f, 100f);
            transform.localScale = ((_health / 200f) + 0.5f) * Vector2.one;
        }
    }
    private float _health;

    private void Awake( ) {
        spriteRenderer.sprite = monsterSprites[Random.Range(0, monsterSprites.Count)];

        Health = 100f;
        lastClickTime = -1;
        IsDead = false;
        startingPosition = transform.position;
        toPosition = transform.position;
    }

    private void Update( ) {
        if (!craveSmashController.IsPlayingGame) {
            return;
        }

        // Randomly move the crave monster around the screen within a certain radius
        if ((transform.position - toPosition).magnitude <= 0.01f) {
            toPosition = startingPosition + (Vector3) (Random.insideUnitCircle * moveRadius);
        }
        transform.position = Vector3.SmoothDamp(transform.position, toPosition, ref positionVelocity, moveSpeed);

        Health += Time.deltaTime * healSpeed;
    }

    private void OnMouseDown( ) {
        // Make sure the crave monster health does not go below 0
        if (IsDead || !craveSmashController.IsPlayingGame) {
            return;
        }

        // Deal damage to the monster
        Health -= clickDamage;

        transform.DOKill(complete: true);
        //transform.DOShakePosition(0.3f, strength: 0.15f);
        transform.DOShakeRotation(0.2f, strength: 20f);

        craveSmashController.UpdateTouchInput( );

        // Calculate the points gained by the player clicking the monster
        // This depends on how long it has been since the last click
        int gainedPoints = baseClickPoints;
        if (lastClickTime >= 0) {
            float timeDifference = Time.time - lastClickTime;
            gainedPoints += Mathf.RoundToInt(Mathf.Exp(-3 * timeDifference + 3));
        }

        craveSmashController.AddPoints(craveSmashController.LastTouchWorldPosition, gainedPoints);
        lastClickTime = Time.time;

        // If the monster has run out of health, then go to the end state
        if (Health <= 0) {
            spriteRenderer.color = Color.clear;
            IsDead = true;

            SoundManager.Instance.PlaySoundEffect(SoundEffectType.MONSTER_DEFEATED);
            craveSmashController.WinGame( );
        } else {
            SoundManager.Instance.PlaySoundEffect(SoundEffectType.MONSTER_HURT);
        }
    }
}
