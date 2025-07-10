using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CheckInController : UIController {
    [Header("CheckInController")]
    [SerializeField, Range(0f, 2f)] private float progressBarTransitionTime;

    protected CheckInSessionData checkInData;

    private VisualElement checkInScreen;

    private VisualElement craveSubscreen;
    private List<Button> craveLevelButtons;

    private VisualElement causeSubscreen;
    private List<Button> craveCauseButtons;

    private VisualElement completeSubscreen;

    private Label checkInLabel;
    private Button nextButton;
    private ProgressBar checkInProgressBar;

    private Coroutine progressBarCoroutine;
    private List<Button> selectedButtons;

    protected override void Awake( ) {
        base.Awake( );

        // Get a reference to all the screens and subscreens
        checkInScreen = ui.Q<VisualElement>("CheckInScreen");
        craveSubscreen = ui.Q<VisualElement>("CraveSubscreen");
        causeSubscreen = ui.Q<VisualElement>("CauseSubscreen");
        completeSubscreen = ui.Q<VisualElement>("CompleteSubscreen");

        // The screen is the same for all of the questions in the check in form
        screens[(int) UIState.CRAVE] = checkInScreen;
        screens[(int) UIState.CAUSE] = checkInScreen;
        screens[(int) UIState.COMPLETE] = checkInScreen;

        // Each subscreen has a different UI state
        subscreens[(int) UIState.CRAVE] = craveSubscreen;
        subscreens[(int) UIState.CAUSE] = causeSubscreen;
        subscreens[(int) UIState.COMPLETE] = completeSubscreen;

        selectedButtons = new List<Button>( );

        // Set up all buttons in the check in
        checkInLabel = ui.Q<Label>("CheckInLabel");

        checkInProgressBar = ui.Q<ProgressBar>("CheckInProgressBar");
        checkInProgressBar.value = checkInProgressBar.lowValue;

        nextButton = ui.Q<Button>("NextButton");
        nextButton.clicked += ( ) => {
            // Only go to the next UI controller state if a button is selected
            if (selectedButtons.Count > 0) {
                // This only works if the UIStates for each of the subscreens of the check in are right next to each other in the enum list
                UIControllerState = UIControllerState + 1;

                // Update the progress bar values since the player has just completed a screen
                if (progressBarCoroutine != null) {
                    StopCoroutine(progressBarCoroutine);
                }
                progressBarCoroutine = StartCoroutine(ProgressBarTransition(checkInProgressBar.value + 0.5f));
            }
        };

        // Set up all elements on the crave level subscreen
        craveLevelButtons = ui.Q<VisualElement>("CraveOptions").Query<Button>( ).ToList( );
        for (int i = 0; i < craveLevelButtons.Count; i++) {
            craveLevelButtons[i].RegisterCallback<ClickEvent>((e) => { SelectOption(craveLevelButtons, craveLevelButtons.IndexOf((Button) e.target)); });
        }

        // Set up all elements on the crave cause subscreen
        craveCauseButtons = ui.Q<VisualElement>("CauseOptions").Query<Button>( ).ToList( );
        for (int i = 0; i < craveCauseButtons.Count; i++) {
            craveCauseButtons[i].RegisterCallback<ClickEvent>((e) => { ToggleSelectOption(craveCauseButtons, craveCauseButtons.IndexOf((Button) e.target)); });
        }

        // Set up all elements on the completed subscreen
        ui.Q<Button>("FinishButton").clicked += ( ) => { FadeToScene(0); };
    }

    protected override void Start( ) {
        base.Start( );

        // The check in controller will always start with the vape question first
        UIControllerState = UIState.CRAVE;
    }

    /// <summary>
    /// Select a button option out of a list of options
    /// </summary>
    /// <param name="options">The option list to select from</param>
    /// <param name="selectOptionIndex">The index within the options list to select</param>
    private void SelectOption(List<Button> options, int selectOptionIndex) {
        // Remove the current selected button if there is one selected
        if (selectedButtons.Count > 0) {
            selectedButtons[0].RemoveFromClassList("uofr-button-selected");
            selectedButtons.RemoveAt(0);
        }

        // Select the new option at the specified index
        selectedButtons.Add(options[selectOptionIndex]);
        selectedButtons[0].AddToClassList("uofr-button-selected");
    }

    /// <summary>
    /// Toggle a button option out of a list of options. This will allow multiple options to be selected at one time
    /// </summary>
    /// <param name="options">The option list to select from</param>
    /// <param name="toggleOptionIndex">The index within hte options list to toggle</param>
    private void ToggleSelectOption(List<Button> options, int toggleOptionIndex) {
        Button toggledOption = options[toggleOptionIndex];

        // Toggle the class on the option as well as remove/add it from the selected buttons list
        if (toggledOption.ClassListContains("uofr-button-selected")) {
            toggledOption.RemoveFromClassList("uofr-button-selected");
            selectedButtons.Remove(toggledOption);
        } else {
            toggledOption.AddToClassList("uofr-button-selected");
            selectedButtons.Add(toggledOption);
        }
    }

    /// <summary>
    /// Deselect all options and save their data based on the last UI controller state
    /// </summary>
    private void DeselectAllOptions( ) {
        // Based on the last controller state, save the selected button data to variables so it can be tracked
        switch (LastUIControllerState) {
            case UIState.NULL:
                // Set the check in label for the next subscreen
                checkInLabel.text = "What is your craving level?";

                break;
            case UIState.CRAVE:
                // Set the check in label for the next subscreen
                checkInLabel.text = "What is causing your craving?";

                // There should only be one option selected for the crave level screen
                // The text should also always be a number
                checkInData.Intensity = int.Parse(selectedButtons[0].text);

                break;
            case UIState.CAUSE:
                // Set the check in label for the next subscreen
                checkInLabel.text = "Complete!";

                // There could be multiple selected options for the craving cause
                for (int i = 0; i < selectedButtons.Count; i++) {
                    checkInData.Triggers.Add(selectedButtons[i].text);
                }

                break;
        }

        // Clear the selected buttons to allow new options to be selected
        selectedButtons.Clear( );
    }

    /// <summary>
    /// Smoothly transition the progress bar to a new value
    /// </summary>
    /// <param name="toValue">The value to set the progress bar to</param>
    /// <returns></returns>
    private IEnumerator ProgressBarTransition(float toValue) {
        float fromValue = checkInProgressBar.value;
        float elapsed = 0f;

        while (elapsed < 1f) {
            elapsed += Time.deltaTime / progressBarTransitionTime;
            checkInProgressBar.value = Mathf.Lerp(fromValue, toValue, elapsed);

            yield return null;
        }

        checkInProgressBar.value = toValue;
    }

    protected override void UpdateSubscreens( ) {
        SetSubscreenVisibility(craveSubscreen, UIControllerState == UIState.CRAVE);
        SetSubscreenVisibility(causeSubscreen, UIControllerState == UIState.CAUSE);
        SetSubscreenVisibility(completeSubscreen, UIControllerState == UIState.COMPLETE);

        // On the last complete screen of the check in form, make sure the next button is not visible anymore
        nextButton.style.display = (UIControllerState == UIState.COMPLETE ? DisplayStyle.None : DisplayStyle.Flex);

        // Deselect all of the options on each subscreen
        DeselectAllOptions( );
    }
}
