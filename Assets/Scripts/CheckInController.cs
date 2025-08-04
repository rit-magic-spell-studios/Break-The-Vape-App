using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CheckInController : UIController {
    [Header("CheckInController")]
    [SerializeField] private Sprite checkInCompleteIcon;

    private VisualElement checkInScreen;

    private Label checkInQuestion;
    private Label checkInSubtitle;
    private Button skipButton;
    private Button nextButton;

    private VisualElement cravingIntensityContainer;
    private SliderInt cravingIntensitySlider;

    private VisualElement cravingCauseContainer;
    private List<Button> cravingCauseButtons;
    private List<Button> selectedButtons;

    protected override void Awake( ) {
        base.Awake( );

        checkInScreen = ui.Q<VisualElement>("CheckInScreen");

        // The screen is the same for all of the questions in the check in form
        screens[(int) UIState.CRAVE] = checkInScreen;
        screens[(int) UIState.CAUSE] = checkInScreen;
        screens[(int) UIState.COMPLETE] = checkInScreen;

        checkInQuestion = ui.Q<Label>("CheckInQuestion");
        checkInSubtitle = ui.Q<Label>("CheckInSubtitle");
        skipButton = ui.Q<Button>("SkipButton");
        nextButton = ui.Q<Button>("NextButton");

        skipButton.clicked += ( ) => { AdvanceCheckInProgress(false); };
        nextButton.clicked += ( ) => { AdvanceCheckInProgress(true); };

        cravingIntensityContainer = ui.Q<VisualElement>("CravingIntensityContainer");
        cravingIntensitySlider = ui.Q<SliderInt>("CravingIntensitySlider");

        cravingCauseContainer = ui.Q<VisualElement>("CravingCauseContainer");
        cravingCauseButtons = cravingCauseContainer.Query<Button>( ).ToList( );
        for (int i = 0; i < cravingCauseButtons.Count; i++) {
            cravingCauseButtons[i].RegisterCallback<ClickEvent>((e) => { ToggleSelectOption(cravingCauseButtons, cravingCauseButtons.IndexOf((Button) e.target)); });
        }

        selectedButtons = new List<Button>( );

        JSONManager.ActiveAppSession.CheckInSessionData.Add(new CheckInSessionData( ));
        AddEventHandlers( );
        JSONManager.InvokeAllDelegates( );
    }

    protected override void Start( ) {
        base.Start( );

        UIControllerState = UIState.CRAVE;
    }

    protected override void Update( ) {
        base.Update( );

        JSONManager.ActiveCheckInSession.AddToPlaytimeSeconds(Time.deltaTime);
    }

    /// <summary>
    /// Advance the progress of the check in form
    /// </summary>
    /// <param name="saveData">Whether or not to save the data that the user has input into the form</param>
    private void AdvanceCheckInProgress(bool saveData) {
        switch (UIControllerState) {
            case UIState.CRAVE:
                if (saveData) {
                    JSONManager.ActiveCheckInSession.Intensity = cravingIntensitySlider.value;
                }

                UIControllerState = UIState.CAUSE;
                break;
            case UIState.CAUSE:
                if (saveData) {
                    if (selectedButtons.Count == 0) {
                        return;
                    }

                    for (int i = 0; i < selectedButtons.Count; i++) {
                        JSONManager.ActiveCheckInSession.Triggers.Add(selectedButtons[i].text);
                    }
                }

                UIControllerState = UIState.COMPLETE;
                break;
            case UIState.COMPLETE:
                JSONManager.Instance.SavePlayerData( );
                FadeToScene(0);
                break;
        }
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

    protected override void UpdateSubscreens( ) {
        SetElementVisibility(cravingIntensityContainer, UIControllerState == UIState.CRAVE);
        SetElementVisibility(cravingCauseContainer, UIControllerState == UIState.CAUSE);

        switch (UIControllerState) {
            case UIState.CRAVE:
                checkInQuestion.text = "What is your craving intensity?";
                SetElementVisibility(checkInSubtitle, false);
                break;
            case UIState.CAUSE:
                checkInQuestion.text = "What is affecting your craving?";
                SetElementVisibility(checkInSubtitle, true);
                checkInSubtitle.text = "You can select more than one option";
                break;
            case UIState.COMPLETE:
                checkInQuestion.text = "Thank you for completing your check in!";
                SetElementVisibility(checkInSubtitle, false);
                skipButton.style.visibility = Visibility.Hidden;
                ui.Q<VisualElement>("NextButtonIcon").style.backgroundImage = new StyleBackground(checkInCompleteIcon);
                break;
        }
    }
}
