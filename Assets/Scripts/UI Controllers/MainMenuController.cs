using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : UIController {
    [Header("MainMenuController")]
    [SerializeField, Range(0f, 5f)] private float notificationTime;
    [SerializeField, Range(600f, 1800f)] private float playGoalSeconds;

    private VisualElement mainScreen;
    private VisualElement ritchCodeScreen;
    private VisualElement splashScreen;
    private VisualElement playGoalScreen;
    private VisualElement viewDataScreen;

    private VisualElement menuSubscreen;
    private VisualElement resetSubscreen;

    private List<TextField> ritchCodeTextFields;
    private VisualElement loadSuccessfulNotification;
    private float notificationTimer;

    private Label greetingLabel;

    protected override void Awake( ) {
        base.Awake( );

        // Get all screens within the game
        mainScreen = ui.Q<VisualElement>("MainScreen");
        ritchCodeScreen = ui.Q<VisualElement>("RITchCodeScreen");
        splashScreen = ui.Q<VisualElement>("SplashScreen");
        playGoalScreen = ui.Q<VisualElement>("PlayGoalScreen");
        viewDataScreen = ui.Q<VisualElement>("ViewDataScreen");

        menuSubscreen = ui.Q<VisualElement>("MenuSubscreen");
        resetSubscreen = ui.Q<VisualElement>("ResetSubscreen");

        // Set the states of the screens and subscreens based on what game controller state is active
        screens[(int) UIState.SPLASH] = splashScreen;
        screens[(int) UIState.MAIN] = mainScreen;
        screens[(int) UIState.MENU] = mainScreen;
        screens[(int) UIState.RESET] = mainScreen;
        screens[(int) UIState.RITCHCODE] = ritchCodeScreen;
        screens[(int) UIState.PLAYGOAL] = playGoalScreen;
        screens[ (int) UIState.DATA] = viewDataScreen;

        subscreens[(int) UIState.MENU] = menuSubscreen;
        subscreens[(int) UIState.RESET] = resetSubscreen;

        splashScreen.RegisterCallback<MouseDownEvent>((e) => { UIControllerState = UIState.MAIN; });

        ui.Q<VisualElement>("CheckInCheckmark").style.display = (JSONManager.HasCompletedCheckIn ? DisplayStyle.Flex : DisplayStyle.None);
        ui.Q<Label>("VersionLabel").text = $"v{Application.version} | MAGIC Spell Studios";

        ui.Q<Button>("CraveSmashButton").clicked += ( ) => { FadeToScene(1); };
        ui.Q<Button>("MatchAndCatchButton").clicked += ( ) => { FadeToScene(2); };
        ui.Q<Button>("CheckInButton").clicked += ( ) => { FadeToScene(3); };
        ui.Q<Button>("NotSoTastyButton").clicked += ( ) => { FadeToScene(4); };
        //ui.Q<Button>("PuffDodgeButton").clicked += ( ) => { FadeToScene(5); };

        ui.Q<Button>("MenuButton").clicked += ( ) => { UIControllerState = UIState.MENU; };
        menuSubscreen.RegisterCallback<MouseDownEvent>((e) => { UIControllerState = UIState.MAIN; });

        ui.Q<Button>("ResetButton").clicked += ( ) => { UIControllerState = UIState.RESET; };
        resetSubscreen.RegisterCallback<MouseDownEvent>((e) => { UIControllerState = UIState.MAIN; });
        ui.Q<Button>("CancelResetButton").clicked += ( ) => { UIControllerState = UIState.MAIN; };
        ui.Q<Button>("ConfirmResetButton").clicked += ( ) => {
            JSONManager.ActiveAppSession.TotalPoints = 0;
            JSONManager.ActiveAppSession.PlaytimeSeconds = 0;
            JSONManager.Instance.SavePlayerData( );

            UIControllerState = UIState.MAIN;
        };

        ui.Q<Button>("RITchCodeButton").clicked += ( ) => { UIControllerState = UIState.RITCHCODE; };
        ui.Q<Button>("RITchCodeBackButton").clicked += ( ) => { UIControllerState = UIState.MAIN; };
        ui.Q<Button>("RITchCodeClearButton").clicked += ClearRITchCodeTextFields;
        ui.Q<Button>("RITchCodeSubmitButton").clicked += SubmitRITchCode;

        loadSuccessfulNotification = ritchCodeScreen.Q<VisualElement>("LoadSuccessfulNotification");
        ritchCodeTextFields = ritchCodeScreen.Query<TextField>( ).ToList( );
        for (int i = 0; i < ritchCodeTextFields.Count; i++) {
            ritchCodeTextFields[i].RegisterValueChangedCallback(CheckTextFieldForAlphanumericValue);
        }

        ui.Q<Button>("PlayGoalButton").clicked += ( ) => { UIControllerState = UIState.PLAYGOAL; };
        ui.Q<Button>("PlayGoalBackButton").clicked += ( ) => { UIControllerState = UIState.MAIN; };

        ui.Q<Button>("ViewDataButton").clicked += ( ) => { UIControllerState = UIState.DATA; };
        ui.Q<Button>("ViewDataBackButton").clicked += ( ) => { UIControllerState = UIState.MAIN; };

        greetingLabel = ui.Q<Label>("GreetingLabel");
        DateTime currentTime = DateTime.Now;
        if (currentTime.Hour >= 5 && currentTime.Hour < 12) {
            greetingLabel.text = "Good morning!";
        } else if (currentTime.Hour >= 12 && currentTime.Hour < 17) {
            greetingLabel.text = "Good afternoon!";
        } else {
            greetingLabel.text = "Good evening!";
        }

        AddEventHandlers( );
        JSONManager.InvokeAllDelegates( );

        notificationTimer = notificationTime;
    }

    protected override void Start( ) {
        base.Start( );

        UIControllerState = (LAST_SCENE == -1 ? UIState.SPLASH : UIState.MAIN);
    }

    protected override void Update( ) {
        base.Update( );

        // Update the timer for notifications to be visible
        notificationTimer += Time.deltaTime;
        if (notificationTimer >= notificationTime) {
            loadSuccessfulNotification.style.visibility = Visibility.Hidden;
        }
    }

    protected override void AddEventHandlers( ) {
        JSONManager.ActiveAppSession.OnPlaytimeSecondsChange += ( ) => {
            int secondsRemaining = (int) (playGoalSeconds - JSONManager.ActiveAppSession.PlaytimeSeconds);
            string timerString = string.Format("{0:0}:{1:00}", secondsRemaining / 60, secondsRemaining % 60);
            ui.Q<Label>("PlayGoalLabel").text = (secondsRemaining > 0) ? $"{timerString} to your play goal!" : "Play goal complete!";
            ui.Q<ProgressBar>("PlayGoalProgressBar").value = JSONManager.ActiveAppSession.PlaytimeSeconds / playGoalSeconds;
        };

        JSONManager.ActiveAppSession.OnTotalPointsChange += ( ) => {
            ui.Q<Label>("TotalScoreLabel").text = $"{JSONManager.ActiveAppSession.TotalPoints} pts";
        };
    }

    /// <summary>
    /// Clear all of the RITch code text fields
    /// </summary>
    private void ClearRITchCodeTextFields( ) {
        for (int i = 0; i < ritchCodeTextFields.Count; i++) {
            ritchCodeTextFields[i].value = "";
        }
    }

    /// <summary>
    /// Submit the currently typed RITch code and load its data
    /// </summary>
    private void SubmitRITchCode( ) {
        string newRITchCode = "";
        for (int i = 0; i < ritchCodeTextFields.Count; i++) {
            newRITchCode += ritchCodeTextFields[i].value;
        }

        if (newRITchCode.Length != 6) {
            return;
        }

        JSONManager.Instance.LoadNewRITchCode(newRITchCode.ToUpper( ));

        notificationTimer = 0;
        loadSuccessfulNotification.style.visibility = Visibility.Visible;
    }

    /// <summary>
    /// Check a text field to ensure it has an alphanumeric value. If not, then clear its value. If yes, then move focus the new RITch code text field
    /// </summary>
    /// <param name="e">Event information about the changed value of the text field</param>
    private void CheckTextFieldForAlphanumericValue(ChangeEvent<string> e) {
        TextField textField = (TextField) e.currentTarget;

        if (e.newValue == "") {
            return;
        }

        if (e.newValue.All(x => char.IsLetterOrDigit(x))) {
            int textFieldIndex = ritchCodeTextFields.IndexOf(textField);
            ritchCodeTextFields[Mathf.Min(textFieldIndex + 1, ritchCodeTextFields.Count - 1)].Focus( );
        } else {
            textField.value = "";
        }
    }

    protected override void UpdateSubscreens( ) {
        SetElementVisibility(menuSubscreen, UIControllerState == UIState.MENU);
        SetElementVisibility(resetSubscreen, UIControllerState == UIState.RESET);
    }
}
