using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public enum CraveSmashGameState {
    GAME, END
}

public class CraveSmashController : MonoBehaviour {
    [SerializeField, Range(0, 1000)] private int monsterSize;
    [SerializeField, Range(0, 1)] private float monsterClickDamage;
    [SerializeField, Range(0, 2)] private float popupDuration;

    private VisualElement ui;

    private List<Label> popupTextList;
    private List<float> popupTextDurationList;

    private VisualElement gameContainer;
    private VisualElement endContainer;

    private Button monsterButton;
    private VisualElement monsterVisual;

    /// <summary>
    /// The current game state
    /// </summary>
    public CraveSmashGameState CraveSmashGameState {
        get => _craveSmashGameState;
        set {
            _craveSmashGameState = value;

            // Update the UI based on the current game state
            switch (_craveSmashGameState) {
                case CraveSmashGameState.GAME:
                    gameContainer.RemoveFromClassList("container-hidden");
                    endContainer.AddToClassList("container-hidden");

                    break;
                case CraveSmashGameState.END:
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

            // Update the size of the monster
            monsterVisual.style.width = _monsterHealth * monsterSize;
            monsterVisual.style.height = _monsterHealth * monsterSize;

            // If the monster has run out of health, then go to the end state
            if (_monsterHealth == 0) {
                CraveSmashGameState = CraveSmashGameState.END;
            }
        }
    }
    private float _monsterHealth;

    private void Awake( ) {
        ui = GetComponent<UIDocument>( ).rootVisualElement;

        // Get references to all the different containers that will be toggled on and off
        gameContainer = ui.Q<VisualElement>("GameContainer");
        endContainer = ui.Q<VisualElement>("EndContainer");

        // Set up button functionality
        ui.Q<Button>("MainMenuButton").clicked += ( ) => { SceneManager.LoadScene(0); };

        // When monster is clicked, deal damage and display indicator text
        monsterButton = ui.Q<Button>("MonsterButton");
        monsterButton.RegisterCallback<ClickEvent>(e => DamageMonster(e.position));
        monsterVisual = ui.Q<VisualElement>("MonsterVisual");
        MonsterHealth = 1f;

        // Initialize lists
        popupTextList = new List<Label>( );
        popupTextDurationList = new List<float>( );

        // At the start of the game, the state should be on the form
        CraveSmashGameState = CraveSmashGameState.GAME;
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
        // Deal damage to the monster
        MonsterHealth -= monsterClickDamage;

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
