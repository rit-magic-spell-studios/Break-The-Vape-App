using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class CraveSmashController : GameController {
    [Header("CraveSmashController")]
    [SerializeField, Range(0, 100)] private int clickDamage;

    private Button craveMonsterButton;
    private VisualElement craveMonsterVisual;
    private int craveMonsterHealth;

    protected override void Awake( ) {
        base.Awake( );

        // Get crave monster elements
        craveMonsterButton = ui.Q<Button>("CraveMonsterButton");
        craveMonsterVisual = ui.Q<VisualElement>("CraveMonsterVisual");

        // When monster is clicked, deal damage and display indicator text
        craveMonsterButton.RegisterCallback<ClickEvent>((e) => {
            // Deal damage to the monster
            craveMonsterHealth -= clickDamage;

            // Update the size of the monster
            // The reason we are updating the width and the top padding is to keep a 1:1 aspect ratio
            // The measurements are also in terms
            craveMonsterVisual.style.width = Length.Percent(Mathf.Clamp(craveMonsterHealth, 0, 99));
            craveMonsterVisual.style.paddingTop = Length.Percent(Mathf.Clamp(craveMonsterHealth, 0, 99));

            // If the monster has run out of health, then go to the end state
            if (craveMonsterHealth == 0) {
                GameControllerState = GameControllerState.WIN;
            }
        });

        // Make sure the crave monster starts at the max health
        craveMonsterHealth = 100;
    }

    protected override void OnGameControllerStateChanged( ) {
        base.OnGameControllerStateChanged( );

        // For some reason the crave monster visual does not update properly at 100% padding
        // Need to do more testing to figure out how to get around that
        craveMonsterVisual.style.width = Length.Percent(Mathf.Clamp(craveMonsterHealth, 0, 99));
        craveMonsterVisual.style.paddingTop = Length.Percent(Mathf.Clamp(craveMonsterHealth, 0, 99));
    }
}
