using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CraveSmashController : GameController {
    [Header("CraveSmashController")]
    [SerializeField, Range(0, 100)] private float clickDamage;
    [SerializeField, Range(0, 50)] private int clickPoints;
    [SerializeField, Range(0, 50)] private int maxClickPoints;
    [SerializeField, Range(0, 3)] private float rapidClickPointIncrease;
    [SerializeField, Range(0, 5)] private float craveMonsterHealDelay;
    [SerializeField, Range(0, 10)] private float craveMonsterHealSpeed;
    [SerializeField] private List<Sprite> craveMonsterStages;

    private float lastClickTime;

    private Button craveMonsterButton;
    private VisualElement craveMonsterVisual;

    public float CraveMonsterHealth {
        get => _craveMonsterHealth;
        private set {
            // Make sure the health stays between 0 and 100
            _craveMonsterHealth = Mathf.Clamp(value, 0f, 100f);

            // Set the crave monster stage image
            int craveMonsterStage = Mathf.Clamp(Mathf.CeilToInt(CraveMonsterHealth / 100f * craveMonsterStages.Count), 1, 3);
            craveMonsterVisual.style.backgroundImage = new StyleBackground(craveMonsterStages[craveMonsterStage - 1]);

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

        // Get crave monster elements
        craveMonsterButton = ui.Q<Button>("CraveMonsterButton");
        craveMonsterVisual = ui.Q<VisualElement>("CraveMonsterVisual");

        // When monster is clicked, deal damage and display indicator text
        craveMonsterButton.RegisterCallback<ClickEvent>((EventCallback<ClickEvent>) ((e) => {
            // Make sure the crave monster health does not go below 0
            if (CraveMonsterHealth <= 0) {
                return;
            }

            // Deal damage to the monster
            CraveMonsterHealth -= clickDamage;

            // Calculate the points gained by the player clicking the monster
            // This depends on how long it has been since the last click
            int gainedPoints = clickPoints;
            if (lastClickTime >= 0) {
                float timeDifference = Time.time - lastClickTime;
                gainedPoints += Mathf.CeilToInt(Mathf.Exp(-timeDifference + 1));
            }

            // Make sure the player does not get a huge amount of points per click
            gainedPoints = Mathf.Min(gainedPoints, maxClickPoints);

            // Increase the player's score
            AddPoints(gainedPoints);
            lastClickTime = Time.time;

            // If the monster has run out of health, then go to the end state
            if (CraveMonsterHealth <= 0) {
                // Set the monster to be invisible
                craveMonsterVisual.style.visibility = Visibility.Hidden;

                // Delay a bit after the health is 0 to allow the player to stop tapping the screen
                // Players were accidentally pressing buttons on the win screen so this delay should hopefully prevent that
                DelayAction(( ) => { UIControllerState = UIState.WIN; }, 1f);
            }
        }));

        // Make sure the crave monster starts at the max health
        CraveMonsterHealth = 100f;
        lastClickTime = -1;
    }

    protected override void Start( ) {
        base.Start( );

        // When the game starts, the tutorial should be shown first
        UIControllerState = UIState.TUTORIAL;
    }

    protected override void Update( ) {
        base.Update( );

        // If it has been a certain amount of time since the last click, have the monster start to heal some of the damage it has taken
        if (lastClickTime != -1 && Time.time - lastClickTime >= craveMonsterHealDelay) {
            CraveMonsterHealth += Time.deltaTime * craveMonsterHealSpeed;
        }
    }
}
