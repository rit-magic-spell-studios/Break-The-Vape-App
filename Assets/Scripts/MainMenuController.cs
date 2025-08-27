using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : UIController {
    [Header("MainMenuController")]
    [SerializeField] private Sprite volumeOnSprite;
    [SerializeField] private Sprite volumeOffSprite;

    private VisualElement mainScreen;
    private VisualElement aboutScreen;

    private Label greetingLabel;
    private VisualElement playGoalInfoPopup;

    private bool isPlayGoalComplete;

    protected override void Awake( ) {
        base.Awake( );

        // Get all screens within the game
        mainScreen = ui.Q<VisualElement>("MainScreen");
        mainScreen.style.visibility = Visibility.Hidden;

        popupOverlay.RegisterCallback<ClickEvent>((e) => {
            if ((VisualElement) e.target == popupOverlay) {
                HideCurrentPopup( );
            }
        });

        ui.Q<Button>("CraveSmashButton").clicked += ( ) => { GoToScene("CraveSmash"); };
        ui.Q<Button>("MatchAndCatchButton").clicked += ( ) => { GoToScene("MatchAndCatch"); };
        ui.Q<Button>("NotSoTastyButton").clicked += ( ) => { GoToScene("NotSoTasty"); };
        ui.Q<Button>("PuffDodgeButton").clicked += ( ) => { GoToScene("PuffDodge"); };

        playGoalInfoPopup = ui.Q<VisualElement>("PlayGoalInfoPopup");
        ui.Q<Button>("PlayGoalInfoButton").clicked += ( ) => { DisplayBasicPopup(playGoalInfoPopup); };
        ui.Q<Button>("PlayGoalInfoContinueButton").clicked += ( ) => { HideCurrentPopup( ); };
        isPlayGoalComplete = false;

        ui.Q<Button>("MenuButton").clicked += ( ) => { DisplayPopup(ui.Q<VisualElement>("MenuPopup"), new Vector2(0, greetingLabel.worldBound.y), new Vector2(Screen.width, greetingLabel.worldBound.y)); };

        ui.Q<Button>("MusicToggleButton").clicked += ( ) => {
            SoundManager.Instance.IsPlayingBackgroundMusic = !SoundManager.Instance.IsPlayingBackgroundMusic;
            ui.Q<VisualElement>("MusicToggleIcon").style.backgroundImage = new StyleBackground(SoundManager.Instance.IsPlayingBackgroundMusic ? volumeOnSprite : volumeOffSprite);
        };
        ui.Q<Button>("SoundEffectToggleButton").clicked += ( ) => {
            SoundManager.Instance.IsPlayingSoundEffects = !SoundManager.Instance.IsPlayingSoundEffects;
            ui.Q<VisualElement>("SoundEffectToggleIcon").style.backgroundImage = new StyleBackground(SoundManager.Instance.IsPlayingSoundEffects ? volumeOnSprite : volumeOffSprite);
        };

        aboutScreen = ui.Q<VisualElement>("AboutScreen");
        aboutScreen.style.visibility = Visibility.Hidden;
        ui.Q<Button>("AboutButton").clicked += ( ) => {
            DisplayScreen(aboutScreen);
            HideCurrentPopup(checkForAnimations: false);
        };
        ui.Q<Button>("AboutBackButton").clicked += ( ) => { DisplayScreen(mainScreen); };
        ui.Q<Label>("VersionLabel").text = $"v{Application.version}\t\t| Unreleased";

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

        DisplayScreen(mainScreen);
        if (LastSceneName == "CheckIn") {
            DisplayBasicPopup(playGoalInfoPopup, checkForAnimations: false);
        }

        DataManager.AppSessionData.InvokeAllDelegates( );
    }
}
