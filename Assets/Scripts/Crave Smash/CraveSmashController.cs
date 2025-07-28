using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CraveSmashController : GameController {
    [Header("CraveSmashController")]
    [SerializeField, Range(0, 100)] private float clickDamage;
    [SerializeField, Range(0, 50)] private int baseClickPoints;
    [SerializeField, Range(0, 50)] private float craveMonsterHealSpeed;
    [SerializeField] private List<Sprite> craveMonsterSprites;

    private float lastClickTime;

    private Button craveMonsterButton;
    private VisualElement craveMonsterVisual;

    public float CraveMonsterHealth {
        get => _craveMonsterHealth;
        private set {
            // Make sure the health stays between 0 and 100
            _craveMonsterHealth = Mathf.Clamp(value, 0f, 100f);

            // Update the size of the monster
            // The reason we are updating the width and the top padding is to keep a 1:1 aspect ratio
            // The measurements are also in terms of percentages to make sure the monster
            // Adding 49 here because adding 50 (for a total of 100% at full health) the crave monster appears stretched in the UI
            float craveMonsterSize = (CraveMonsterHealth / 2f) + 49f;
            craveMonsterVisual.style.width = new StyleLength(Length.Percent(craveMonsterSize));
            craveMonsterVisual.style.paddingTop = new StyleLength(Length.Percent(craveMonsterSize / 2f));
            craveMonsterVisual.style.paddingBottom = new StyleLength(Length.Percent(craveMonsterSize / 2f));
        }
    }
    private float _craveMonsterHealth;

    protected override void Awake( ) {
        base.Awake( );

        craveMonsterButton = ui.Q<Button>("CraveMonsterButton");
        craveMonsterButton.RegisterCallback<ClickEvent>(OnCraveMonsterClick);

        craveMonsterVisual = ui.Q<VisualElement>("CraveMonsterVisual");
        craveMonsterVisual.style.backgroundImage = new StyleBackground(craveMonsterSprites[Random.Range(0, craveMonsterSprites.Count)]);

        CraveMonsterHealth = 100f;
        lastClickTime = -1;
    }

    protected override void Update( ) {
        base.Update( );

        if (UIControllerState != UIState.GAME) {
            return;
        }

            CraveMonsterHealth += Time.deltaTime * craveMonsterHealSpeed;
    }

    /// <summary>
    /// Handle when the crave monster is clicked on by the player
    /// </summary>
    /// <param name="e">Event information about the monster click</param>
    private void OnCraveMonsterClick(ClickEvent e) {
        // Make sure the crave monster health does not go below 0
        if (CraveMonsterHealth <= 0) {
            return;
        }

        // Deal damage to the monster
        CraveMonsterHealth -= clickDamage;

        // Calculate the points gained by the player clicking the monster
        // This depends on how long it has been since the last click
        int gainedPoints = baseClickPoints;
        if (lastClickTime >= 0) {
            float timeDifference = Time.time - lastClickTime;
            gainedPoints += Mathf.RoundToInt(Mathf.Exp(-3 * timeDifference + 3));
        }

        // Make sure the player does not get a huge amount of points per click
        GamePoints += gainedPoints;
        lastClickTime = Time.time;

        // If the monster has run out of health, then go to the end state
        if (CraveMonsterHealth <= 0) {
            // Set the monster to be invisible
            craveMonsterVisual.style.visibility = Visibility.Hidden;

            // Delay a bit after the health is 0 to allow the player to stop tapping the screen
            // Players were accidentally pressing buttons on the win screen so this delay should hopefully prevent that
            DelayAction(( ) => { UIControllerState = UIState.WIN; }, 2f);
        }
    }
}
