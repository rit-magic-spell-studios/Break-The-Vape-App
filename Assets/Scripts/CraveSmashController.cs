using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CraveSmashController : GameController {
    [Header("CraveSmashController")]
    [SerializeField, Range(0, 100)] private int clickDamage;
    [SerializeField] private List<Sprite> craveMonsterStages;

    private int craveMonsterHealth;

    private Button craveMonsterButton;
    private VisualElement craveMonsterVisual;

    protected override void Awake( ) {
        base.Awake( );

        // Get crave monster elements
        craveMonsterButton = ui.Q<Button>("CraveMonsterButton");
        craveMonsterVisual = ui.Q<VisualElement>("CraveMonsterVisual");

        // When monster is clicked, deal damage and display indicator text
        craveMonsterButton.RegisterCallback<ClickEvent>((e) => {
            // Make sure the crave monster health does not go below 0
            if (craveMonsterHealth <= 0) {
                return;
            }

            // Deal damage to the monster
            craveMonsterHealth -= clickDamage;

            // Set the crave monster stage image
            int craveMonsterStage = Mathf.Clamp(Mathf.CeilToInt(craveMonsterHealth / 100f * craveMonsterStages.Count), 1, 3);
            craveMonsterVisual.style.backgroundImage = new StyleBackground(craveMonsterStages[craveMonsterStage - 1]);

            // Update the size of the monster
            // The reason we are updating the width and the top padding is to keep a 1:1 aspect ratio
            // The measurements are also in terms of percentages to make sure the monster
            // Adding 49 here because adding 50 (for a total of 100% at full health) the crave monster appears stretched in the UI
            int craveMonsterSize = (craveMonsterHealth / 2) + 49;
            craveMonsterVisual.style.width = new StyleLength(Length.Percent(craveMonsterSize));
            craveMonsterVisual.style.paddingTop = new StyleLength(Length.Percent(craveMonsterSize / 2f));
            craveMonsterVisual.style.paddingBottom = new StyleLength(Length.Percent(craveMonsterSize / 2f));

            // Increase the player's score
            AddPoints(20);

            // If the monster has run out of health, then go to the end state
            if (craveMonsterHealth == 0) {
                // Set the monster to be invisible
                craveMonsterVisual.style.visibility = Visibility.Hidden;

                // Delay a bit after the health is 0 to allow the player to stop tapping the screen
                // Players were accidentally pressing buttons on the win screen so this delay should hopefully prevent that
                DelayAction(( ) => { UIControllerState = UIState.WIN; }, 1f);
            }
        });

        // Make sure the crave monster starts at the max health
        craveMonsterHealth = 100;
    }

    protected override void Start( ) {
        base.Start( );

        // When the game starts, the tutorial should be shown first
        UIControllerState = UIState.TUTORIAL;
    }
}
