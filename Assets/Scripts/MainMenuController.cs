using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : UIController {
    private VisualElement mainScreen;
    private VisualElement resetScreen;

    private Label greetingLabel;

    protected override void Awake( ) {
        base.Awake( );

        // Get all screens within the game
        mainScreen = ui.Q<VisualElement>("MainScreen");
        resetScreen = ui.Q<VisualElement>("ResetScreen");

        // Set the states of the screens and subscreens based on what game controller state is active
        screens[(int) UIState.MAIN] = mainScreen;
        screens[(int) UIState.RESET] = resetScreen;

        // Set up menu button functionality
        ui.Q<Button>("CraveSmashButton").clicked += ( ) => { FadeToScene(1); };
        ui.Q<Button>("MatchAndCatchButton").clicked += ( ) => { FadeToScene(2); };
        ui.Q<Button>("CheckInButton").clicked += ( ) => { FadeToScene(3); };
        ui.Q<Button>("ResetButton").clicked += ( ) => { UIControllerState = UIState.RESET; };
        ui.Q<Button>("CancelResetButton").clicked += ( ) => { UIControllerState = UIState.MAIN; };
        ui.Q<Button>("ConfirmResetButton").clicked += ( ) => {
            // Reset the total points value
            playerData.TotalPoints = 0;
            ui.Q<Label>("TotalScoreLabel").text = $"{playerData.TotalPoints} points";

            UIControllerState = UIState.MAIN;
        };

        // Update UI elements based on the player's score and check-in actions
        ui.Q<Label>("CheckInLabel").text = (playerData.HasCompletedInitialCheckIn ? "Check-in completed!" : "Complete your check-in!");
        ui.Q<VisualElement>("CheckInCheckmark").style.display = (playerData.HasCompletedInitialCheckIn ? DisplayStyle.Flex : DisplayStyle.None);
        ui.Q<Label>("TotalScoreLabel").text = $"{playerData.TotalPoints} points";

        // Update the label at the top to give a new message based on the time of day
        greetingLabel = ui.Q<Label>("GreetingLabel");
        DateTime currentTime = DateTime.Now;
        if (currentTime.Hour >= 5 && currentTime.Hour < 12) {
            greetingLabel.text = "Good morning!";
        } else if (currentTime.Hour >= 12 && currentTime.Hour < 17) {
            greetingLabel.text = "Good afternoon!";
        } else {
            greetingLabel.text = "Good evening!";
        }
    }

    protected override void Start( ) {
        base.Start( );

        // The main menu controller will always start on the main menu UI state
        UIControllerState = UIState.MAIN;
    }
}
