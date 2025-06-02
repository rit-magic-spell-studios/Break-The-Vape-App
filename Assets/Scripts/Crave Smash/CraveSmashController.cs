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
    [SerializeField, Range(0, 2)] private float popupDuration;

    private VisualElement ui;

    private List<Label> popupTextList;
    private List<float> popupTextDurationList;

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

                    // Set the size of the monster based on the form input
                    float cravePercentage = (craveSlider.value - craveSlider.lowValue) / (float) (craveSlider.highValue - craveSlider.lowValue);
                    MonsterSize = (cravePercentage * (monsterMaxSize - monsterMinSize)) + monsterMinSize;

                    break;
                case CraveSmashGameState.END:
                    formContainer.AddToClassList("container-hidden");
                    gameContainer.AddToClassList("container-hidden");
                    endContainer.RemoveFromClassList("container-hidden");

                    // Remove all popups from the screen
                    for (int i = popupTextList.Count - 1; i >= 0; i--) {
                        // Remove it from the UI
                        ui.Remove(popupTextList[i]);
                    }
                    popupTextList.Clear( );
                    popupTextDurationList.Clear( );

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
            monsterHealthBar.value = _monsterHealth;

            // If the monster has run out of health, then go to the end state
            if (_monsterHealth == 0) {
                CraveSmashGameState = CraveSmashGameState.END;
            }
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

        // Set the health bar to max
        monsterHealthBar = ui.Q<ProgressBar>("MonsterHealthBar");
        MonsterHealth = monsterHealthBar.highValue;

        // Initialize lists
        popupTextList = new List<Label>( );
        popupTextDurationList = new List<float>( );

        // At the start of the game, the state should be on the form
        CraveSmashGameState = CraveSmashGameState.FORM;
    }

    private void Update( ) {
        // Loop through all of the popup text and update their duration
        for (int i = popupTextDurationList.Count - 1; i >= 0; i--) {
            popupTextDurationList[i] += Time.deltaTime;

            // If the popup has been on the screen for the popup duration length, then remove it
            if (popupTextDurationList[i] >= popupDuration) {
                // Remove it from the UI
                ui.Remove(popupTextList[i]);

                // Remove it from both the label and duration lists
                popupTextDurationList.RemoveAt(i);
                popupTextList.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Damage the monster by a certain amount based on the size
    /// </summary>
    /// <param name="mousePosition">The position of the mouse when it damaged the monster</param>
    private void DamageMonster(Vector3 mousePosition) {
        // Do a different amount of damage based on the size of the monster
        // This means that larger monsters will be harder to destroy than smaller monsters
        float damageModifier = 1f - ((MonsterSize - monsterMinSize) / (1.35f * (monsterMaxSize - monsterMinSize)));
        Debug.Log("MOD: " + damageModifier + " | TOTAL: " + (monsterClickDamage * damageModifier));
        MonsterHealth -= monsterClickDamage * damageModifier;

        // Spawn an indicator on the screen the position that the mouse clicked
        // NOTE: Right now the label blocks raycasts to the monster below it, meaning that you can't click the monster when the mouse is over this label
        //       Disabling this for now so I can figure out a better way to add it back later
        //Label popupLabel = new Label {
        //    style = {
        //        position = Position.Absolute,
        //        left = mousePosition.x,
        //        top = mousePosition.y,
        //        width = 100,
        //        height = 30,
        //        translate = new StyleTranslate(new Translate(-25, -30))
        //    },
        //    text = "Tap!"
        //};
        //ui.Add(popupLabel);
        //popupTextList.Add(popupLabel);
        //popupTextDurationList.Add(0);
    }
}
