using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CheckInController : UIController {
    private VisualElement splashScreen;
    private VisualElement checkInScreen;
    private VisualElement ritchCodeScreen;

    private Label checkInQuestion;
    private Label checkInSubtitle;
    private Label formPageNumber;

    private VisualElement checkInPopup;
    private List<VisualElement> checkInFormPages;
    public int CurrentFormPageIndex {
        get => _currentFormSection;
        set {
            checkInFormPages[_currentFormSection].style.display = DisplayStyle.None;
            _currentFormSection = value;
            checkInFormPages[_currentFormSection].style.display = DisplayStyle.Flex;
            formPageNumber.text = $"{_currentFormSection + 1}/{checkInFormPages.Count}";

            // Set the question and subtitle based on the new form page
            if (checkInFormPages[_currentFormSection] == demographicInfoContainer) {
                checkInQuestion.text = "Initial Information";
                checkInSubtitle.text = "You will only have to enter this information once";
            } else if (checkInFormPages[_currentFormSection] == cravingIntensityContainer) {
                checkInQuestion.text = "What is your craving level?";
                checkInSubtitle.text = "On a scale from 0 to 5, how much are you craving to vape?";
            } else if (checkInFormPages[_currentFormSection] == cravingCauseContainer) {
                checkInQuestion.text = "What is affecting your craving?";
                checkInSubtitle.text = "You can select more than one option";
            }
        }
    }
    private int _currentFormSection;

    private VisualElement demographicInfoContainer;
    private RadioButtonGroup ageButtonGroup;
    private RadioButtonGroup environmentButtonGroup;
    private Button selectedFrequencyButton;

    private VisualElement cravingIntensityContainer;
    private Button selectedIntensityButton;

    private VisualElement cravingCauseContainer;
    private List<Button> cravingCauseButtons;
    private List<Button> selectedCauseButtons;

    private List<TextField> ritchCodeTextFields;

    private CheckInSessionData checkInSessionData;

    protected override void Awake( ) {
        base.Awake( );

        splashScreen = ui.Q<VisualElement>("SplashScreen");
        checkInScreen = ui.Q<VisualElement>("CheckInScreen");
        ritchCodeScreen = ui.Q<VisualElement>("RITchCodeScreen");

        checkInPopup = ui.Q<VisualElement>("CheckInPopup");
        checkInQuestion = ui.Q<Label>("CheckInQuestion");
        checkInSubtitle = ui.Q<Label>("CheckInSubtitle");
        formPageNumber = ui.Q<Label>("FormPageNumber");
        ui.Q<Button>("NextButton").clicked += ( ) => {
            if (!CheckForPageComplete( )) {
                return;
            }

            HideCurrentPopup(onComplete: ( ) => { GoToNextFormPage( ); });
        };

        demographicInfoContainer = ui.Q<VisualElement>("DemographicInfoContainer");
        ageButtonGroup = ui.Q<RadioButtonGroup>("AgeButtonGroup");
        environmentButtonGroup = ui.Q<RadioButtonGroup>("EnvironmentButtonGroup");
        List<Button> vapeFrequencyButtons = ui.Q<VisualElement>("VapeFrequencyButtons").Query<Button>( ).ToList( );
        for (int i = 0; i < vapeFrequencyButtons.Count; i++) {
            vapeFrequencyButtons[i].RegisterCallback<ClickEvent>((e) => {
                selectedFrequencyButton?.RemoveFromClassList("uofr-button-selected");
                selectedFrequencyButton = (Button) e.target;
                selectedFrequencyButton.AddToClassList("uofr-button-selected");
            });
        }

        cravingIntensityContainer = ui.Q<VisualElement>("CravingIntensityContainer");
        List<Button> cravingIntensityButtons = ui.Q<VisualElement>("CravingIntensityButtons").Query<Button>( ).ToList( );
        for (int i = 0; i < cravingIntensityButtons.Count; i++) {
            cravingIntensityButtons[i].RegisterCallback<ClickEvent>((e) => {
                selectedIntensityButton?.RemoveFromClassList("uofr-button-selected");
                selectedIntensityButton = (Button) e.target;
                selectedIntensityButton.AddToClassList("uofr-button-selected");
            });
        }

        cravingCauseContainer = ui.Q<VisualElement>("CravingCauseContainer");
        cravingCauseButtons = cravingCauseContainer.Query<Button>( ).ToList( );
        for (int i = 0; i < cravingCauseButtons.Count; i++) {
            cravingCauseButtons[i].RegisterCallback<ClickEvent>((e) => { ToggleSelectOption(cravingCauseButtons, cravingCauseButtons.IndexOf((Button) e.target)); });
        }
        selectedCauseButtons = new List<Button>( );

        ui.Q<Button>("RITchCodeLoginButton").clicked += ( ) => { DisplayScreen(ritchCodeScreen); };
        ui.Q<Button>("GuestButton").clicked += SetupCheckInForm;
        ui.Q<Button>("RITchCodeBackButton").clicked += ( ) => { DisplayScreen(splashScreen); };
        ui.Q<Button>("RITchCodeClearButton").clicked += ClearRITchCodeTextFields;
        ui.Q<Button>("RITchCodeSubmitButton").clicked += SubmitRITchCode;
        ritchCodeTextFields = ritchCodeScreen.Query<TextField>( ).ToList( );
        for (int i = 0; i < ritchCodeTextFields.Count; i++) {
            ritchCodeTextFields[i].RegisterValueChangedCallback(CheckTextFieldForAlphanumericValue);
        }
    }

    protected override void Start( ) {
        base.Start( );
        DisplayScreen(splashScreen);
        checkInSessionData = new CheckInSessionData( );
    }

    protected override void Update( ) {
        checkInSessionData.TotalTimeSecondsValue += Time.deltaTime;
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
            selectedCauseButtons.Remove(toggledOption);
        } else {
            toggledOption.AddToClassList("uofr-button-selected");
            selectedCauseButtons.Add(toggledOption);
        }
    }


    /// <summary>
    /// Clear all of the RITch code text fields
    /// </summary>
    private void ClearRITchCodeTextFields( ) {
        for (int i = 0; i < ritchCodeTextFields.Count; i++) {
            ritchCodeTextFields[i].value = "";
        }

        ritchCodeTextFields[0].Focus( );
    }

    /// <summary>
    /// Submit the currently typed RITch code and load its data
    /// </summary>
    private void SubmitRITchCode( ) {
        string newRITchCode = "";
        for (int i = 0; i < ritchCodeTextFields.Count; i++) {
            newRITchCode += ritchCodeTextFields[i].value;
        }

        if (!DataManager.Instance.CheckForValidRITchCode(newRITchCode)) {
            return;
        }

        DataManager.AppSessionData.RITchCode = newRITchCode;
        SetupCheckInForm( );
    }

    /// <summary>
    /// Check a text field to ensure it has an alphanumeric value. If not, then clear its value. If yes, then move focus the new RITch code text field
    /// </summary>
    /// <param name="e">Event information about the changed value of the text field</param>
    private void CheckTextFieldForAlphanumericValue(ChangeEvent<string> e) {
        TextField textField = (TextField) e.currentTarget;
        int textFieldIndex = ritchCodeTextFields.IndexOf(textField);

        if (e.newValue == "") {
            ritchCodeTextFields[Mathf.Max(textFieldIndex - 1, 0)].Focus( );
            return;
        }

        if (e.newValue.All(x => char.IsLetterOrDigit(x))) {
            ritchCodeTextFields[Mathf.Min(textFieldIndex + 1, ritchCodeTextFields.Count - 1)].Focus( );
        } else {
            textField.value = "";
        }
    }

    private void SetupCheckInForm( ) {
        // If the user has logged in with a ritch code, then check for user data already on the device
        // If there is user data, then they do not have to fill out the demographic section of the form
        // If there is no user data, then they have to fill it out again
        // This new demographic data will be saved to the device so they do not have to do it again later
        // If the user has not logged in, then they need to fill in the demographic data

        checkInFormPages = new List<VisualElement>( );
        checkInFormPages.Add(demographicInfoContainer);
        checkInFormPages.Add(cravingIntensityContainer);
        checkInFormPages.Add(cravingCauseContainer);
        CurrentFormPageIndex = 0;

        DisplayScreen(checkInScreen);
        DisplayBasicPopup(checkInPopup, checkForAnimations: false);
    }

    /// <summary>
    /// Go to the next check in form page
    /// </summary>
    private void GoToNextFormPage( ) {
        if (CurrentFormPageIndex + 1 == checkInFormPages.Count) {
            GoToScene("MainMenu");
        } else {
            CurrentFormPageIndex++;
            DisplayBasicPopup(checkInPopup);
        }
    }

    /// <summary>
    /// Check to see if a specific page is complete, meaning all of the necessary information has been entered
    /// </summary>
    /// <returns>true unless the current page is not filled out all the way, then it returns false</returns>
    private bool CheckForPageComplete( ) {
        if (checkInFormPages[CurrentFormPageIndex] == demographicInfoContainer) {
            return (ageButtonGroup.value != -1 && environmentButtonGroup.value != -1 && selectedFrequencyButton != null);
        } else if (checkInFormPages[CurrentFormPageIndex] == cravingIntensityContainer) {
            return (selectedIntensityButton != null);
        } else if (checkInFormPages[CurrentFormPageIndex] == cravingCauseContainer) {
            return (selectedCauseButtons.Count > 0);
        }

        return true;
    }

    protected override void GoToScene(string sceneName) {
        DataManager.AppSessionData.UserData.Age = ageButtonGroup.choices.ToList( )[ageButtonGroup.value];
        DataManager.AppSessionData.UserData.Environment = environmentButtonGroup.choices.ToList( )[environmentButtonGroup.value];
        DataManager.AppSessionData.UserData.DaysVapedDuringPastWeek = int.Parse(selectedFrequencyButton.text);
        checkInSessionData.CravingIntensity = int.Parse(selectedIntensityButton.text);
        checkInSessionData.CravingTriggers = selectedCauseButtons.Select(button => button.text).ToList( );

        DataManager.Instance.UploadSessionData(checkInSessionData);
        base.GoToScene(sceneName);
    }
}
