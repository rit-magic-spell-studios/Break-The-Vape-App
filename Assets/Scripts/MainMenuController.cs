using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : UIController {
    private VisualElement mainScreen;
    private VisualElement playGoalInfoScreen;
    private VisualElement aboutScreen;

    private Label greetingLabel;

    private bool isPlayGoalComplete;

    protected override void Awake( ) {
        base.Awake( );

        // Get all screens within the game
        mainScreen = ui.Q<VisualElement>("MainScreen");
        mainScreen.style.display = DisplayStyle.None;
        playGoalInfoScreen = ui.Q<VisualElement>("PlayGoalInfoScreen");
        playGoalInfoScreen.style.display = DisplayStyle.None;

        ui.Q<Label>("VersionLabel").text = $"v{Application.version} | 8-18-25";

        ui.Q<Button>("CraveSmashButton").clicked += ( ) => { GoToScene("CraveSmash"); };
        ui.Q<Button>("MatchAndCatchButton").clicked += ( ) => { GoToScene("MatchAndCatch"); };
        ui.Q<Button>("NotSoTastyButton").clicked += ( ) => { GoToScene("NotSoTasty"); };
        ui.Q<Button>("PuffDodgeButton").clicked += ( ) => { GoToScene("PuffDodge"); };

        ui.Q<Button>("PlayGoalInfoButton").clicked += ( ) => { DisplayScreen(playGoalInfoScreen); };
        ui.Q<Button>("PlayGoalInfoBackButton").clicked += ( ) => { DisplayScreen(mainScreen); };
        isPlayGoalComplete = false;

        ui.Q<Button>("AboutButton").clicked += ( ) => { DisplayScreen(aboutScreen); };
        ui.Q<Button>("AboutBackButton").clicked += ( ) => { DisplayScreen(mainScreen); };

        popupOverlay.RegisterCallback<MouseDownEvent>((e) => { HideCurrentPopup( ); });
        ui.Q<Button>("MenuButton").clicked += ( ) => { DisplayPopup(ui.Q<VisualElement>("MenuPopup"), Vector2.zero, new Vector2(Screen.width / 2f, 0)); };

        ui.Q<Button>("LogOutButton").clicked += ( ) => { DisplayBasicPopup(ui.Q<VisualElement>("LogOutPopup")); };
        ui.Q<Button>("ConfirmLogOutButton").clicked += ( ) => {
            DataManager.AppSessionData.ResetData( );
            GoToScene("CheckIn");
        };
        ui.Q<Button>("CancelLogOutButton").clicked += ( ) => { HideCurrentPopup( ); };

        ui.Q<Button>("RestartSessionButton").clicked += ( ) => {
            isPlayGoalComplete = false;
            DataManager.AppSessionData.TotalPointsEarnedValue = 0;
            DataManager.AppSessionData.TotalTimeSecondsValue = 0;
            GoToScene("MainMenu");
            HideCurrentPopup(checkForAnimations: false);
        };
        ui.Q<Button>("FinishSessionButton").clicked += ( ) => {
            DataManager.AppSessionData.ResetData( );
            GoToScene("CheckIn");
        };

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
                DisplayBasicPopup(ui.Q<VisualElement>("PlayGoalCompletePopup"), checkForAnimations: false);
            } else {
                string timerString = string.Format("{0:0}:{1:00}", (int) secondsRemaining / 60, (int) secondsRemaining % 60);
                ui.Q<Label>("RadialProgressBarLabel").text = timerString;
                ui.Q<RadialProgress>("RadialProgressBar").Progress = DataManager.AppSessionData.TotalTimeSeconds / PLAY_GOAL_SECONDS * 100f;
            }
        };
        DataManager.AppSessionData.OnTotalPointsEarnedChange += ( ) => {
            ui.Q<Label>("TotalScoreLabel").text = $"{DataManager.AppSessionData.TotalPointsEarned:N0} pts";
            ui.Q<Label>("FinalScoreLabel").text = $"{DataManager.AppSessionData.TotalPointsEarned:N0} pts";
        };
    }

    protected override void Start( ) {
        base.Start( );

        if (LastSceneName == "CheckIn") {
            DisplayScreen(playGoalInfoScreen);
        } else {
            DisplayScreen(mainScreen);
        }
        DataManager.AppSessionData.InvokeAllDelegates( );
    }
}
