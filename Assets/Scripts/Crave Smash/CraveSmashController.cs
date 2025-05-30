using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public enum CraveSmashGameState {
    FORM, GAME, END
}

public class CraveSmashController : MonoBehaviour {
    [SerializeField, Range(0, 1000)] private float monsterMinSize;
    [SerializeField, Range(0, 1000)] private float monsterMaxSize;
    [SerializeField, Range(0, 1)] private float monsterClickDamage;

    private VisualElement ui;

    private VisualElement formContainer;
    private VisualElement gameContainer;
    private VisualElement endContainer;

    private Button monster;
    private SliderInt craveSlider;
    private ProgressBar monsterHealthBar;

    /// <summary>
    /// The current game state
    /// </summary>
    public CraveSmashGameState CraveSmashGameState {
        get => _craveSmashGameState;
        set {
            _craveSmashGameState = value;

            // Update the UI based on the current game state
            switch (_craveSmashGameState) {
                case CraveSmashGameState.FORM:
                    formContainer.RemoveFromClassList("container-hidden");
                    gameContainer.AddToClassList("container-hidden");
                    endContainer.AddToClassList("container-hidden");

                    break;
                case CraveSmashGameState.GAME:
                    formContainer.AddToClassList("container-hidden");
                    gameContainer.RemoveFromClassList("container-hidden");
                    endContainer.AddToClassList("container-hidden");

                    break;
                case CraveSmashGameState.END:
                    formContainer.AddToClassList("container-hidden");
                    gameContainer.AddToClassList("container-hidden");
                    endContainer.RemoveFromClassList("container-hidden");

                    break;
            }

        }
    }
    private CraveSmashGameState _craveSmashGameState;

    /// <summary>
    /// The current health of the monster
    /// </summary>
    public float MonsterHealth {
        get => _monsterHealth;
        set {
            _monsterHealth = Mathf.Clamp01(value);

            // Update the health progress bar to match the monster's health
        }
    }
    private float _monsterHealth;

    /// <summary>
    /// The size of the monster
    /// </summary>
    public float MonsterSize {
        get => _monsterSize;
        set {
            _monsterSize = Mathf.Clamp(value, monsterMinSize, monsterMaxSize);

            // Set the size of the monster button UI element
            monster.style.width = _monsterSize;
            monster.style.height = _monsterSize;

            // If the monster has run out of health, then go to the end state
            if (_monsterHealth == 0) {
                CraveSmashGameState = CraveSmashGameState.END;
            }
        }
    }
    private float _monsterSize;

    private void Awake( ) {
        ui = GetComponent<UIDocument>( ).rootVisualElement;

        // Get references to all the different containers that will be toggled on and off
        formContainer = ui.Q<VisualElement>("FormContainer");
        gameContainer = ui.Q<VisualElement>("GameContainer");
        endContainer = ui.Q<VisualElement>("EndContainer");

        // Set up button functionality
        ui.Q<Button>("SubmitButton").clicked += ( ) => { CraveSmashGameState = CraveSmashGameState.GAME; };
        ui.Q<Button>("MainMenuButton").clicked += ( ) => { SceneManager.LoadScene(0); };

        // Update the crave slider value label
        craveSlider = ui.Q<SliderInt>("CraveSlider");
        craveSlider.RegisterValueChangedCallback(e => { craveSlider.label = $"{e.newValue}"; });
        craveSlider.value = 5;
        craveSlider.label = "5";

        // When monster is clicked, deal damage and display indicator text
        monster = ui.Q<Button>("Monster");
        monster.RegisterCallback<ClickEvent>(e => DamageMonster(e.position));
        monsterHealthBar = ui.Q<ProgressBar>("MonsterHealthBar");

        // At the start of the game, the state should be on the form
        CraveSmashGameState = CraveSmashGameState.FORM;
    }

    /// <summary>
    /// Damage the monster by a certain amount based on the size
    /// </summary>
    /// <param name="mousePosition">The position of the mouse when it damaged the monster</param>
    private void DamageMonster(Vector3 mousePosition) {
        // Do a different amount of damage based on the size of the monster
        // This means that larger monsters will be harder to destroy than smaller monsters
        float damageModifier = 1f - ((MonsterSize - monsterMinSize) / (1.05f * monsterMaxSize));
        MonsterHealth -= monsterClickDamage * damageModifier;

        // Spawn an indicator on the screen the position that the mouse clicked
        ui.Add(new Label {
            style = {
                position = Position.Absolute,
                left = mousePosition.x,
                top = mousePosition.y
            },
            text = "Tap!"
        });
        Debug.Log(mousePosition);
    }
}
