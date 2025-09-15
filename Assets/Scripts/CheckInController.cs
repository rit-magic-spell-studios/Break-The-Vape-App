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
                checkInQuestion.text = "Your Information";
                checkInSubtitle.text = "";
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
    private RadioButtonGroup genderButtonGroup;
    private RadioButtonGroup environmentButtonGroup;
    private Button selectedFrequencyButton;

    private VisualElement cravingIntensityContainer;
    private Button selectedIntensityButton;

    private VisualElement cravingCauseContainer;
    private List<Button> cravingCauseButtons;
    private List<Button> selectedCauseButtons;

    private Button ritchCodeClearButton;
    private List<TextField> ritchCodeTextFields;

    public CheckInSessionData CheckInSessionData { get; private set; }

    protected override void Awake( ) {
        base.Awake( );

        // Get a reference to all of the screens in the check in scene
        splashScreen = ui.Q<VisualElement>("SplashScreen");
        checkInScreen = ui.Q<VisualElement>("CheckInScreen");
        ritchCodeScreen = ui.Q<VisualElement>("RITchCodeScreen");

        // Get a reference to all of the check in elements
        checkInPopup = ui.Q<VisualElement>("CheckInPopup");
        checkInQuestion = ui.Q<Label>("CheckInQuestion");
        checkInSubtitle = ui.Q<Label>("CheckInSubtitle");
        formPageNumber = ui.Q<Label>("FormPageNumber");

        // Set up the next button on the check in form
        ui.Q<Button>("NextButton").clicked += ( ) => {
            // If the page is not complete, then do not proceed to the next form page
            if (!CheckForPageComplete( )) {
                return;
            }

            HideCurrentPopup(onComplete: ( ) => { GoToNextFormPage( ); });
        };

        // Setup all demographic information buttons and elements
        demographicInfoContainer = ui.Q<VisualElement>("DemographicInfoContainer");
        ageButtonGroup = ui.Q<RadioButtonGroup>("AgeButtonGroup");
        genderButtonGroup = ui.Q<RadioButtonGroup>("GenderButtonGroup");
        environmentButtonGroup = ui.Q<RadioButtonGroup>("EnvironmentButtonGroup");
        List<Button> vapeFrequencyButtons = ui.Q<VisualElement>("VapeFrequencyButtons").Query<Button>( ).ToList( );
        for (int i = 0; i < vapeFrequencyButtons.Count; i++) {
            vapeFrequencyButtons[i].RegisterCallback<ClickEvent>((e) => {
                selectedFrequencyButton?.RemoveFromClassList("uofr-button-selected");
                selectedFrequencyButton = (Button) e.target;
                selectedFrequencyButton.AddToClassList("uofr-button-selected");
            });
        }

        // Setup all craving intensity buttons and elements
        cravingIntensityContainer = ui.Q<VisualElement>("CravingIntensityContainer");
        List<Button> cravingIntensityButtons = ui.Q<VisualElement>("CravingIntensityButtons").Query<Button>( ).ToList( );
        for (int i = 0; i < cravingIntensityButtons.Count; i++) {
            cravingIntensityButtons[i].RegisterCallback<ClickEvent>((e) => {
                selectedIntensityButton?.RemoveFromClassList("uofr-button-selected");
                selectedIntensityButton = (Button) e.target;
                selectedIntensityButton.AddToClassList("uofr-button-selected");
            });
        }

        // Setup all craving cause buttons and elements
        cravingCauseContainer = ui.Q<VisualElement>("CravingCauseContainer");
        cravingCauseButtons = cravingCauseContainer.Query<Button>( ).ToList( );
        for (int i = 0; i < cravingCauseButtons.Count; i++) {
            cravingCauseButtons[i].RegisterCallback<ClickEvent>((e) => { ToggleSelectOption(cravingCauseButtons, cravingCauseButtons.IndexOf((Button) e.target)); });
        }
        selectedCauseButtons = new List<Button>( );

        // Setup RITch code login and screen buttons
        ui.Q<Button>("GuestButton").clicked += SetupCheckInForm;
        ui.Q<Button>("RITchCodeLoginButton").clicked += ( ) => {
            DisplayScreen(ritchCodeScreen,
                onHalfway: ( ) => { ritchCodeClearButton.style.visibility = Visibility.Hidden; },
                onComplete: ( ) => { ritchCodeTextFields[0].Focus( ); }
            );
        };
        ui.Q<Button>("RITchCodeBackButton").clicked += ( ) => { DisplayScreen(splashScreen); };
        ritchCodeClearButton = ui.Q<Button>("RITchCodeClearButton");
        ritchCodeClearButton.clicked += ( ) => {
            // Set the value of all the RITch code text fields to an empty string
            for (int i = 0; i < ritchCodeTextFields.Count; i++) {
                ritchCodeTextFields[i].value = "";
            }

            // Focus the leftmost text field
            ritchCodeTextFields[0].Focus( );
        };
        ui.Q<Button>("RITchCodeSubmitButton").clicked += ( ) => {
            // Add all text to a single string for the RITch code
            string newRITchCode = "";
            for (int i = 0; i < ritchCodeTextFields.Count; i++) {
                newRITchCode += ritchCodeTextFields[i].value;
            }

            // Make sure the RITch code is valid
            // If it is not, flash the prompt text to let the user know they have entered something wrong
            if (!DataManager.Instance.CheckForValidRITchCode(newRITchCode)) {
                FlashTextValidation(new List<Label>( ) { ui.Q<Label>("RITchCodePrompt") });
                return;
            }

            // If there are currently elements animating, also do nothing
            if (animatingVisualElements.Count > 0) {
                return;
            }

            // Set the app session RITch code and start the check in form
            DataManager.AppSessionData.RITchCode = newRITchCode;
            SetupCheckInForm( );
        };

        // Setup RITch code text fields
        // There is an individual text field for every character of the RITch code
        ritchCodeTextFields = ritchCodeScreen.Query<TextField>( ).ToList( );
        for (int i = 0; i < ritchCodeTextFields.Count; i++) {
            ritchCodeTextFields[i].RegisterValueChangedCallback((e) => {
                // Check to see if any RITch code text field has a value in it
                // If so, enable the clear RITch code button
                bool textFieldsHaveValue = false;
                for (int j = 0; j < ritchCodeTextFields.Count; j++) {
                    if (ritchCodeTextFields[j].value.Length > 0) {
                        textFieldsHaveValue = true;
                        break;
                    }
                }
                ritchCodeClearButton.style.visibility = (textFieldsHaveValue ? Visibility.Visible : Visibility.Hidden);

                // Get a reference to the current text field that had its value changed
                TextField textField = (TextField) e.currentTarget;
                int textFieldIndex = ritchCodeTextFields.IndexOf(textField);

                // If the new value is a blank string, then the user just deleted whatever they had in the text field
                // Focus the text field to the left of this one
                if (e.newValue == "") {
                    ritchCodeTextFields[Mathf.Max(textFieldIndex - 1, 0)].Focus( );
                    return;
                }

                // If the new value is not a blank string, check to see if it is alphanumeric
                // If it is, then focus the text field to the right of this one
                // If not, then set this text field's value back to a blank string
                if (e.newValue.All(x => char.IsLetterOrDigit(x))) {
                    ritchCodeTextFields[Mathf.Min(textFieldIndex + 1, ritchCodeTextFields.Count - 1)].Focus( );
                } else {
                    textField.value = "";
                }
            });
        }
    }

    protected override void Start( ) {
        base.Start( );
        DisplayScreen(splashScreen);
        CheckInSessionData = new CheckInSessionData( );
    }

    protected override void Update( ) {
        CheckInSessionData.TotalTimeSecondsValue += Time.deltaTime;
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

        DisplayScreen(checkInScreen, onHalfway: ( ) => { ritchCodeClearButton.style.visibility = Visibility.Hidden; });
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
        List<Label> invalidLabels = new List<Label>( );

        if (checkInFormPages[CurrentFormPageIndex] == demographicInfoContainer) {
            if (ageButtonGroup.value == -1) {
                invalidLabels.Add(ageButtonGroup.Q<Label>( ));
            }

            if (genderButtonGroup.value == -1) {
                invalidLabels.Add(genderButtonGroup.Q<Label>( ));
            }

            if (environmentButtonGroup.value == -1) {
                invalidLabels.Add(environmentButtonGroup.Q<Label>( ));
            }

            if (selectedFrequencyButton == null) {
                invalidLabels.Add(ui.Q<Label>("VapeFrequencyLabel"));
            }
        } else if (checkInFormPages[CurrentFormPageIndex] == cravingIntensityContainer) {
            if (selectedIntensityButton == null) {
                invalidLabels.Add(checkInQuestion);
                invalidLabels.Add(checkInSubtitle);
            }
        } else if (checkInFormPages[CurrentFormPageIndex] == cravingCauseContainer) {
            if (selectedCauseButtons.Count == 0) {
                invalidLabels.Add(checkInQuestion);
                invalidLabels.Add(checkInSubtitle);
            }
        }

        FlashTextValidation(invalidLabels);
        return (invalidLabels.Count == 0);
    }

    protected override void GoToScene(string sceneName) {
        DataManager.AppSessionData.UserData.Age = ageButtonGroup.choices.ToList( )[ageButtonGroup.value];
        DataManager.AppSessionData.UserData.Gender = genderButtonGroup.choices.ToList( )[genderButtonGroup.value];
        DataManager.AppSessionData.UserData.Environment = environmentButtonGroup.choices.ToList( )[environmentButtonGroup.value];
        DataManager.AppSessionData.UserData.DaysVapedDuringPastWeek = int.Parse(selectedFrequencyButton.text);
        CheckInSessionData.CravingIntensity = int.Parse(selectedIntensityButton.text);
        CheckInSessionData.CravingTriggers = selectedCauseButtons.Select(button => button.text).ToList( );

        DataManager.Instance.UploadSessionData(CheckInSessionData);
        base.GoToScene(sceneName);
    }
}
