using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : UIController {
    private VisualElement mainScreen;
    private VisualElement playGoalInfoScreen;

    private Label greetingLabel;
    private bool isPlayGoalComplete;

    protected override void Awake( ) {
        base.Awake( );

        // Get all screens within the game
        mainScreen = ui.Q<VisualElement>("MainScreen");
        playGoalInfoScreen = ui.Q<VisualElement>("PlayGoalInfoScreen");

        ui.Q<Label>("VersionLabel").text = $"v{Application.version} | MAGIC Spell Studios";

        ui.Q<Button>("CraveSmashButton").clicked += ( ) => { GoToScene("CraveSmash"); };
        ui.Q<Button>("MatchAndCatchButton").clicked += ( ) => { GoToScene("MatchAndCatch"); };
        ui.Q<Button>("NotSoTastyButton").clicked += ( ) => { GoToScene("NotSoTasty"); };
        ui.Q<Button>("PuffDodgeButton").clicked += ( ) => { GoToScene("PuffDodge"); };

        ui.Q<Button>("PlayGoalInfoButton").clicked += ( ) => DisplayScreen(playGoalInfoScreen);
        ui.Q<Button>("PlayGoalInfoBackButton").clicked += ( ) => DisplayScreen(mainScreen);
        isPlayGoalComplete = false;

        popupOverlay.RegisterCallback<MouseDownEvent>((e) => { HideCurrentPopup( ); });
        ui.Q<Button>("MenuButton").clicked += ( ) => DisplayPopup(ui.Q<VisualElement>("MenuPopup"), Vector2.zero, new Vector2(Screen.width / 2f, 0));

        ui.Q<Button>("LogOutButton").clicked += ( ) => DisplayBasicPopup(ui.Q<VisualElement>("LogOutPopup"));
        ui.Q<Button>("ConfirmLogOutButton").clicked += ( ) => {
            // Send the user back to the splash screen
            // Clear app session data
        };
        ui.Q<Button>("CancelLogOutButton").clicked += ( ) => HideCurrentPopup( );

        ui.Q<Button>("DeleteUserDataButton").clicked += ( ) => DisplayBasicPopup(ui.Q<VisualElement>("DeleteUserDataPopup"));
        ui.Q<Button>("ConfirmDeleteUserDataButton").clicked += ( ) => {
            HideCurrentPopup( );
            DataManager.Instance.RemoveUserData(DataManager.AppSessionData.RITchCode);
        };
        ui.Q<Button>("CancelDeleteUserDataButton").clicked += ( ) => HideCurrentPopup( );

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
            if (isPlayGoalComplete) {
                return;
            }

            float secondsRemaining = Mathf.Max(0, PLAY_GOAL_SECONDS - DataManager.AppSessionData.TotalTimeSeconds);
            if (secondsRemaining == 0) {
                isPlayGoalComplete = true;
                DisplayBasicPopup(ui.Q<VisualElement>("PlayGoalCompletePopup"));
            } else {
                string timerString = string.Format("{0:0}:{1:00}", (int) secondsRemaining / 60, (int) secondsRemaining % 60);
                ui.Q<Label>("RadialProgressBarLabel").text = timerString;
                ui.Q<RadialProgress>("RadialProgressBar").Progress = DataManager.AppSessionData.TotalTimeSeconds / PLAY_GOAL_SECONDS * 100f;
            }
        };
        DataManager.AppSessionData.OnTotalPointsEarnedChange += ( ) => {
            ui.Q<Label>("TotalScoreLabel").text = $"{DataManager.AppSessionData.TotalPointsEarned} pts";
        };
        DataManager.AppSessionData.InvokeAllDelegates( );
    }

    protected override void Start( ) {
        base.Start( );
        DisplayScreen(mainScreen);
    }
}
