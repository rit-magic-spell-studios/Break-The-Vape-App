using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CheckInController : UIController {
    [Header("CheckInController")]
    [SerializeField, Range(0f, 2f)] private float progressBarTransitionTime;

    private VisualElement checkInScreen;
    private VisualElement cravingLevelSubscreen;
    private VisualElement cravingCauseSubscreen;

    private SliderInt cravingLevelSlider;

    private List<Button> cravingCausesButtons;
    private List<Button> selectedButtons;

    protected override void Awake( ) {
        base.Awake( );

        // Get a reference to all the screens and subscreens
        checkInScreen = ui.Q<VisualElement>("CheckInScreen");
        cravingLevelSubscreen = ui.Q<VisualElement>("CravingLevelSubscreen");
        cravingCauseSubscreen = ui.Q<VisualElement>("CravingCauseSubscreen");

        // The screen is the same for all of the questions in the check in form
        screens[(int) UIState.CRAVE] = checkInScreen;
        screens[(int) UIState.CAUSE] = checkInScreen;

        // Each subscreen has a different UI state
        subscreens[(int) UIState.CRAVE] = cravingLevelSubscreen;
        subscreens[(int) UIState.CAUSE] = cravingCauseSubscreen;

        ui.Q<Button>("SkipCravingLevelButton").clicked += AdvanceCheckInProgress;
        ui.Q<Button>("SkipCravingCauseButton").clicked += AdvanceCheckInProgress;
        ui.Q<Button>("FinishButton").clicked += AdvanceCheckInProgress;
        ui.Q<Button>("NextButton").clicked += AdvanceCheckInProgress;

        cravingLevelSlider = ui.Q<SliderInt>("CravingSliderInt");

        // Set up all elements on the crave cause subscreen
        cravingCausesButtons = ui.Q<VisualElement>("CravingCausesContainer").Query<Button>( ).ToList( );
        selectedButtons = new List<Button>( );
        for (int i = 0; i < cravingCausesButtons.Count; i++) {
            cravingCausesButtons[i].RegisterCallback<ClickEvent>((e) => { ToggleSelectOption(cravingCausesButtons, cravingCausesButtons.IndexOf((Button) e.target)); });
        }

        JSONManager.ActiveAppSession.CheckInSessionData.Add(new CheckInSessionData( ));
    }

    protected override void Start( ) {
        base.Start( );

        // The check in controller will always start with the vape question first
        UIControllerState = UIState.CRAVE;
    }

    protected override void Update( ) {
        base.Update( );

        JSONManager.ActiveCheckInSession.PlaytimeSeconds += Time.deltaTime;
    }

    private void AdvanceCheckInProgress( ) {
        if (UIControllerState == UIState.CAUSE) {
            JSONManager.Instance.SavePlayerData( );
            FadeToScene(0);
        } else {
            UIControllerState += 1;
        }
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
        if (toggledOption.ClassListContains("uofr-button-discreet")) {
            toggledOption.RemoveFromClassList("uofr-button-discreet");
            selectedButtons.Add(toggledOption);
        } else {
            toggledOption.AddToClassList("uofr-button-discreet");
            selectedButtons.Remove(toggledOption);
        }
    }

    /// <summary>
    /// Deselect all options and save their data based on the last UI controller state
    /// </summary>
    //private void DeselectAllOptions( ) {
    //    // Based on the last controller state, save the selected button data to variables so it can be tracked
    //    switch (LastUIControllerState) {
    //        case UIState.NULL:
    //            // Set the check in label for the next subscreen
    //            checkInLabel.text = "What is your craving level?";

    //            break;
    //        case UIState.CRAVE:
    //            // Set the check in label for the next subscreen
    //            checkInLabel.text = "What is causing your craving?";

    //            // There should only be one option selected for the crave level screen
    //            // The text should also always be a number
    //            JSONManager.ActiveCheckInSession.Intensity = int.Parse(selectedButtons[0].text);

    //            break;
    //        case UIState.CAUSE:
    //            // Set the check in label for the next subscreen
    //            checkInLabel.text = "Complete!";

    //            // There could be multiple selected options for the craving cause
    //            for (int i = 0; i < selectedButtons.Count; i++) {
    //                JSONManager.ActiveCheckInSession.Triggers.Add(selectedButtons[i].text);
    //            }

    //            break;
    //    }

    //    // Clear the selected buttons to allow new options to be selected
    //    selectedButtons.Clear( );
    //}

    protected override void UpdateSubscreens( ) {
        SetSubscreenVisibility(cravingLevelSubscreen, UIControllerState == UIState.CRAVE);
        SetSubscreenVisibility(cravingCauseSubscreen, UIControllerState == UIState.CAUSE);
    }
}
