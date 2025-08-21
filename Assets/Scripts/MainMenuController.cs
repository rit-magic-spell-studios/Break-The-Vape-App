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
    private VisualElement menuPopup;

    private bool isPlayGoalComplete;

    protected override void Awake( ) {
        base.Awake( );

        // Get all screens within the game
        mainScreen = ui.Q<VisualElement>("MainScreen");
        playGoalInfoScreen = ui.Q<VisualElement>("PlayGoalInfoScreen");
        aboutScreen = ui.Q<VisualElement>("AboutScreen");
        mainScreen.style.visibility = Visibility.Hidden;
        playGoalInfoScreen.style.visibility = Visibility.Hidden;
        aboutScreen.style.visibility = Visibility.Hidden;

        ui.Q<Label>("VersionLabel").text = $"v{Application.version}\t\t| 8-18-25";

        ui.Q<Button>("CraveSmashButton").clicked += ( ) => { GoToScene("CraveSmash"); };
        ui.Q<Button>("MatchAndCatchButton").clicked += ( ) => { GoToScene("MatchAndCatch"); };
        ui.Q<Button>("NotSoTastyButton").clicked += ( ) => { GoToScene("NotSoTasty"); };
        ui.Q<Button>("PuffDodgeButton").clicked += ( ) => { GoToScene("PuffDodge"); };

        ui.Q<Button>("PlayGoalInfoButton").clicked += ( ) => { DisplayScreen(playGoalInfoScreen); };
        ui.Q<Button>("PlayGoalInfoBackButton").clicked += ( ) => { DisplayScreen(mainScreen); };
        isPlayGoalComplete = false;

        ui.Q<Button>("AboutButton").clicked += ( ) => {
            DisplayScreen(aboutScreen);
            HideCurrentPopup(checkForAnimations: false);
        };
        ui.Q<Button>("AboutBackButton").clicked += ( ) => { DisplayScreen(mainScreen); };

        greetingLabel = ui.Q<Label>("GreetingLabel");
        DateTime currentTime = DateTime.Now;
        if (currentTime.Hour >= 5 && currentTime.Hour < 12) {
            greetingLabel.text = "Good morning!";
        } else if (currentTime.Hour >= 12 && currentTime.Hour < 17) {
            greetingLabel.text = "Good afternoon!";
        } else {
            greetingLabel.text = "Good evening!";
        }

        popupOverlay.RegisterCallback<ClickEvent>((e) => {
            if ((VisualElement) e.target == popupOverlay) {
                HideCurrentPopup( );
            }
        });
        ui.Q<Button>("MenuButton").clicked += ( ) => { DisplayPopup(ui.Q<VisualElement>("MenuPopup"), new Vector2(0, greetingLabel.worldBound.y), new Vector2(Screen.width / 2f, greetingLabel.worldBound.y)); };

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
