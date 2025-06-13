using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CheckInController : UIController {
    private VisualElement checkInScreen;

    private VisualElement vapeSubscreen;
    private VisualElement craveSubscreen;
    private VisualElement causeSubscreen;
    private VisualElement completeSubscreen;

    private Label checkInLabel;
    private Button nextButton;
    private ProgressBar checkInProgressBar;
    private Button finishButton;

    private List<Button> vapeButtons;
    private List<Button> craveLevelButtons;
    private List<Button> craveCauseButtons;

    private List<Button> selectedButtons;
    private bool hasVapedInLast24Hours;
    private int cravingLevel;
    private List<string> cravingCauses;

    protected override void Awake( ) {
        base.Awake( );

        // Get a reference to all the screens and subscreens
        checkInScreen = ui.Q<VisualElement>("CheckInScreen");
        vapeSubscreen = ui.Q<VisualElement>("VapeSubscreen");
        craveSubscreen = ui.Q<VisualElement>("CraveSubscreen");
        causeSubscreen = ui.Q<VisualElement>("CauseSubscreen");
        completeSubscreen = ui.Q<VisualElement>("CompleteSubscreen");

        // The screen is the same for all of the questions in the check in form
        screens[(int) UIState.VAPE] = checkInScreen;
        screens[(int) UIState.CRAVE] = checkInScreen;
        screens[(int) UIState.CAUSE] = checkInScreen;
        screens[(int) UIState.COMPLETE] = checkInScreen;

        // Each subscreen has a different UI state
        subscreens[(int) UIState.VAPE] = vapeSubscreen;
        subscreens[(int) UIState.CRAVE] = craveSubscreen;
        subscreens[(int) UIState.CAUSE] = causeSubscreen;
        subscreens[(int) UIState.COMPLETE] = completeSubscreen;

        // Set up all buttons in the check in
        checkInLabel = ui.Q<Label>("CheckInLabel");
        nextButton = ui.Q<Button>("NextButton");
        checkInProgressBar = ui.Q<ProgressBar>("CheckInProgressBar");

        // Set up all elements on the vape subscreen
        vapeButtons = ui.Q<VisualElement>("VapeOptions").Query<Button>( ).ToList( );

        // Set up all elements on the crave level subscreen
        craveLevelButtons = ui.Q<VisualElement>("CraveOptions").Query<Button>( ).ToList( );

        // Set up all elements on the crave cause subscreen
        craveCauseButtons = ui.Q<VisualElement>("CauseOptions").Query<Button>( ).ToList( );

        // Set up all elements on the completed subscreen
        ui.Q<Button>("FinishButton").clicked += ( ) => { FadeToScene(0); };
    }

    protected override void Start( ) {
        base.Start( );

        // The check in controller will always start with the vape question first
        UIControllerState = UIState.VAPE;
    }

    /// <summary>
    /// Select a button option out of a list of options
    /// </summary>
    /// <param name="options">The option list to select from</param>
    /// <param name="selectOption">The index within the options list to select</param>
    /// <param name="onlyOneOptionSelected">Whether or not multiple options can be selected at once. If this is true, then selecting a new option will automatically deselect the currently selected option</param>
    private void SelectOption(List<VisualElement> options, int selectOption, bool onlyOneOptionSelected) {

    }

    /// <summary>
    /// Deselect all options and save their data based on the last UI controller state
    /// </summary>
    private void DeselectAllOptions( ) {
        // Based on the last controller state, save the selected button data to variables so it can be tracked
        switch (LastUIControllerState) {
            case UIState.VAPE:
                // There should only be one option selected for the vape screen
                hasVapedInLast24Hours = (selectedButtons[0].text == "Yes");

                break;
            case UIState.CRAVE:
                // There should only be one option selected for the crave level screen
                // The text should also always be a number
                cravingLevel = int.Parse(selectedButtons[0].text);

                break;
            case UIState.CAUSE:
                // There could be multiple selected options for the craving cause
                cravingCauses = new List<string>( );
                for (int i = 0; i < selectedButtons.Count; i++) {
                    cravingCauses.Add(selectedButtons[i].text);
                }

                break;
        }

        // Clear the selected buttons to allow new options to be selected
        selectedButtons.Clear( );
    }
}
