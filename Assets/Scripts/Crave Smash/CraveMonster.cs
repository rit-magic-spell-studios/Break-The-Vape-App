using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CraveMonster : MonoBehaviour {
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<Sprite> monsterSprites;
    [SerializeField] private CraveSmashController craveSmashController;
    [Space]
    [SerializeField, Range(0, 100)] private float clickDamage;
    [SerializeField, Range(0, 50)] private int baseClickPoints;
    [SerializeField, Range(0, 50)] private float healSpeed;
    [SerializeField] private float _health;

    private float lastClickTime;

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

    private void Awake( ) {
        spriteRenderer.sprite = monsterSprites[Random.Range(0, monsterSprites.Count)];

        Health = 100f;
        lastClickTime = -1;
        IsDead = false;
    }

    private void Update( ) {
        if (craveSmashController.UIControllerState != UIState.GAME) {
            return;
        }

        Health += Time.deltaTime * healSpeed;
    }

    private void OnMouseDown( ) {
        // Make sure the crave monster health does not go below 0
        if (IsDead) {
            return;
        }

        // Deal damage to the monster
        Health -= clickDamage;

        transform.DOKill(complete: true);
        transform.DOShakePosition(0.3f, strength: 0.15f);
        transform.DOShakeRotation(0.2f, strength: 20f);

        craveSmashController.UpdateTouchInput( );

        // Calculate the points gained by the player clicking the monster
        // This depends on how long it has been since the last click
        int gainedPoints = baseClickPoints;
        if (lastClickTime >= 0) {
            float timeDifference = Time.time - lastClickTime;
            gainedPoints += Mathf.RoundToInt(Mathf.Exp(-3 * timeDifference + 3));
        }

        // Make sure the player does not get a huge amount of points per click
        craveSmashController.GameSessionData.PointsEarnedValue += gainedPoints;
        lastClickTime = Time.time;

        craveSmashController.SpawnPointsPopup(craveSmashController.LastTouchWorldPosition, gainedPoints);

        // If the monster has run out of health, then go to the end state
        if (Health <= 0) {
            // Set the monster to be invisible
            spriteRenderer.color = Color.clear;
            IsDead = true;

            craveSmashController.SpawnConfettiParticles(transform.position);

            // Delay a bit after the health is 0 to allow the player to stop tapping the screen
            // Players were accidentally pressing buttons on the win screen so this delay should hopefully prevent that
            craveSmashController.DelayAction(( ) => { craveSmashController.UIControllerState = UIState.WIN; }, 2f);
        }
    }
}
