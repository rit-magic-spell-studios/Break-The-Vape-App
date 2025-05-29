using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CraveMonster : MonoBehaviour, IPointerClickHandler {
	[SerializeField] private CraveSmashManager craveSmashManager;
	[SerializeField] private Canvas canvas;
	[SerializeField] private RectTransform rectTransform;
	[SerializeField] private RectTransform healthBarBackground;
	[SerializeField] private RectTransform healthBarFill;
	[SerializeField] private GameObject indicatorTextPrefab;
	[Space]
	[SerializeField, Range(0, 1000)] private float _minMonsterSize = 200;
	[SerializeField, Range(0, 1000)] private float _maxMonsterSize = 700;
	[SerializeField, Range(0, 1)] private float damageAmount = 0.1f;

	/// <summary>
	/// The maximum size of the monster
	/// </summary>
	public float MaxMonsterSize => _maxMonsterSize;

	/// <summary>
	/// The minimum size of the monster
	/// </summary>
	public float MinMonsterSize => _minMonsterSize;

	/// <summary>
	/// The current size of the crave monster
	/// </summary>
	public float MonsterSize {
		get => _monsterSize;
		set {
			_monsterSize = Mathf.Clamp(value, MinMonsterSize, MaxMonsterSize);

			// Set the image size based on the monster size
			rectTransform.sizeDelta = new Vector2(_monsterSize, _monsterSize);
		}
	}
	private float _monsterSize;

	/// <summary>
	/// The current percentage that the health bar is filled in
	/// </summary>
	public float HealthPercentage {
		get => _healthPercentage;
		set {
			_healthPercentage = Mathf.Clamp01(value);

			// If the health bar background is reading a width of 0, then wait for the layout to update before calling the function
			// Otherwise just call the function
			if (healthBarBackground.rect.width == 0) {
				Invoke(nameof(UpdateHealthBar), 0.02f);
			} else {
				UpdateHealthBar( );
			}

			// If the monster has no more health, then set the game state to the end of the game
			if (_healthPercentage == 0f) {
				craveSmashManager.CraveSmashGameState = CraveSmashGameState.END;
			}
		}
	}
	private float _healthPercentage;

	private void OnValidate ( ) {
		rectTransform = GetComponent<RectTransform>( );
	}

	private void Awake ( ) {
		OnValidate( );
	}

	public void OnPointerClick (PointerEventData eventData) {
		// When the player clicks on the monster, do damage to it
		DamageMonster( );
	}

	/// <summary>
	/// Update the health bar size based on the current health percentage
	/// </summary>
	private void UpdateHealthBar ( ) {
		// Set the size of the health bar fill based on the health percentage
		float size = healthBarBackground.rect.width * HealthPercentage;
		healthBarFill.anchoredPosition = new Vector3(size / 2f, 0f, 0f);
		healthBarFill.sizeDelta = new Vector2(size, healthBarBackground.rect.height);
	}

	/// <summary>
	/// Damage the monster
	/// </summary>
	private void DamageMonster ( ) {
		// Do a different amount of damage based on the size of the monster
		// This means that larger monsters will be harder to destroy than smaller monsters
		float damageModifier = 1f - ((MonsterSize - MinMonsterSize) / (1.05f * MaxMonsterSize));
		HealthPercentage -= damageAmount * damageModifier;

		// Spawn some indicator text when the player does damage
		Vector3 indicatorPosition = (Vector2) Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Quaternion indicatorRotation = Quaternion.Euler(0f, 0f, Random.Range(-20f, 20f));
		TextMeshProUGUI indicatorText = Instantiate(indicatorTextPrefab, indicatorPosition, indicatorRotation, transform.parent.parent).GetComponent<TextMeshProUGUI>( );
		indicatorText.text = "Tap!";
	}
}
