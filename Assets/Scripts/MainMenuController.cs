using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : UIController {
    [Header("MainMenuController")]
    [SerializeField, Range(600f, 1800f)] private float playGoalSeconds;

    private VisualElement mainScreen;
    private VisualElement playGoalScreen;

    private Label greetingLabel;

    protected override void Awake( ) {
        base.Awake( );

        // Get all screens within the game
        mainScreen = ui.Q<VisualElement>("MainScreen");
        playGoalScreen = ui.Q<VisualElement>("PlayGoalScreen");

        // Set the states of the screens and subscreens based on what game controller state is active
        screens[(int) UIState.MAIN] = mainScreen;
        screens[(int) UIState.PLAYGOAL] = playGoalScreen;

        popupOverlay.RegisterCallback<MouseDownEvent>((e) => { UIControllerState = UIState.MAIN; });

        ui.Q<Label>("VersionLabel").text = $"v{Application.version} | MAGIC Spell Studios";

        ui.Q<Button>("CraveSmashButton").clicked += ( ) => { FadeToScene(1); };
        ui.Q<Button>("MatchAndCatchButton").clicked += ( ) => { FadeToScene(2); };
        ui.Q<Button>("NotSoTastyButton").clicked += ( ) => { FadeToScene(4); };
        ui.Q<Button>("PuffDodgeButton").clicked += ( ) => { FadeToScene(5); };

        //ui.Q<Button>("MenuButton").clicked += ( ) => { UIControllerState = UIState.MENU; };
        //menuSubscreen.RegisterCallback<MouseDownEvent>((e) => { UIControllerState = UIState.MAIN; });

        //ui.Q<Button>("ResetButton").clicked += ( ) => { UIControllerState = UIState.RESET; };
        //resetSubscreen.RegisterCallback<MouseDownEvent>((e) => { UIControllerState = UIState.MAIN; });
        //ui.Q<Button>("CancelResetButton").clicked += ( ) => { UIControllerState = UIState.MAIN; };
        //ui.Q<Button>("ConfirmResetButton").clicked += ( ) => {
        //    DataManager.AppSessionData.TotalPointsEarnedValue = 0;
        //    DataManager.AppSessionData.TotalTimeSecondsValue = 0;
        //    UIControllerState = UIState.MAIN;
        //};

        //ui.Q<Button>("RITchCodeButton").clicked += ( ) => { UIControllerState = UIState.RITCHCODE; };
        //ui.Q<Button>("RITchCodeBackButton").clicked += ( ) => { UIControllerState = UIState.MAIN; };
        //ui.Q<Button>("RITchCodeClearButton").clicked += ClearRITchCodeTextFields;
        //ui.Q<Button>("RITchCodeSubmitButton").clicked += SubmitRITchCode;

        //ritchCodeTextFields = ritchCodeScreen.Query<TextField>( ).ToList( );
        //for (int i = 0; i < ritchCodeTextFields.Count; i++) {
        //    ritchCodeTextFields[i].RegisterValueChangedCallback(CheckTextFieldForAlphanumericValue);
        //}

        ui.Q<Button>("PlayGoalButton").clicked += ( ) => { UIControllerState = UIState.PLAYGOAL; };
        ui.Q<Button>("PlayGoalBackButton").clicked += ( ) => { UIControllerState = UIState.MAIN; };

        greetingLabel = ui.Q<Label>("GreetingLabel");
        DateTime currentTime = DateTime.Now;
        if (currentTime.Hour >= 5 && currentTime.Hour < 12) {
            greetingLabel.text = "Good morning!";
        } else if (currentTime.Hour >= 12 && currentTime.Hour < 17) {
            greetingLabel.text = "Good afternoon!";
        } else {
            greetingLabel.text = "Good evening!";
        }

        DataManager.AppSessionData.OnTotalTimeSecondsChange += ( ) => {
            int secondsRemaining = (int) (playGoalSeconds - DataManager.AppSessionData.TotalTimeSeconds);
            string timerString = string.Format("{0:0}:{1:00}", secondsRemaining / 60, secondsRemaining % 60);
            ui.Q<Label>("RadialProgressBarLabel").text = timerString;
            ui.Q<RadialProgress>("RadialProgressBar").Progress = DataManager.AppSessionData.TotalTimeSeconds / playGoalSeconds * 100f;
        };
        DataManager.AppSessionData.OnTotalPointsEarnedChange += ( ) => {
            ui.Q<Label>("TotalScoreLabel").text = $"{DataManager.AppSessionData.TotalPointsEarned} pts";
        };
        DataManager.AppSessionData.InvokeAllDelegates( );
    }

    protected override void Start( ) {
        base.Start( );
        UIControllerState = UIState.MAIN;
    }

    /// <summary>
    /// Clear all of the RITch code text fields
    /// </summary>
    //private void ClearRITchCodeTextFields( ) {
    //    for (int i = 0; i < ritchCodeTextFields.Count; i++) {
    //        ritchCodeTextFields[i].value = "";
    //    }

    //    ritchCodeTextFields[0].Focus( );
    //}

    /// <summary>
    /// Submit the currently typed RITch code and load its data
    /// </summary>
    //private void SubmitRITchCode( ) {
    //    string newRITchCode = "";
    //    for (int i = 0; i < ritchCodeTextFields.Count; i++) {
    //        newRITchCode += ritchCodeTextFields[i].value;
    //    }

    //    if (newRITchCode.Length != 6) {
    //        return;
    //    }

    //    //JSONManager.Instance.LoadNewRITchCode(newRITchCode.ToUpper( ));

    //    UIControllerState = UIState.SPLASH;
    //}

    /// <summary>
    /// Check a text field to ensure it has an alphanumeric value. If not, then clear its value. If yes, then move focus the new RITch code text field
    /// </summary>
    /// <param name="e">Event information about the changed value of the text field</param>
    //private void CheckTextFieldForAlphanumericValue(ChangeEvent<string> e) {
    //    TextField textField = (TextField) e.currentTarget;
    //    int textFieldIndex = ritchCodeTextFields.IndexOf(textField);

    //    if (e.newValue == "") {
    //        ritchCodeTextFields[Mathf.Max(textFieldIndex - 1, 0)].Focus( );
    //        return;
    //    }

    //    if (e.newValue.All(x => char.IsLetterOrDigit(x))) {
    //        ritchCodeTextFields[Mathf.Min(textFieldIndex + 1, ritchCodeTextFields.Count - 1)].Focus( );
    //    } else {
    //        textField.value = "";
    //    }
    //}

    //protected override void UpdateSubscreens( ) {
    //    SetElementVisibility(menuSubscreen, UIControllerState == UIState.MENU);
    //    SetElementVisibility(resetSubscreen, UIControllerState == UIState.RESET);
    //}

    protected override void FadeToScene(int sceneBuildIndex) {
        DataManager.AppSessionData.ClearAllDelegates( );
        base.FadeToScene(sceneBuildIndex);
    }
}
