using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : UIController {
    private VisualElement mainScreen;
    private VisualElement ritchCodeScreen;
    private VisualElement splashScreen;

    private VisualElement menuSubscreen;
    private VisualElement resetSubscreen;

    private List<TextField> ritchCodeTextFields;

    private Label greetingLabel;

    protected override void Awake( ) {
        base.Awake( );

        // Get all screens within the game
        mainScreen = ui.Q<VisualElement>("MainScreen");
        ritchCodeScreen = ui.Q<VisualElement>("RITchCodeScreen");
        splashScreen = ui.Q<VisualElement>("SpashScreen");

        menuSubscreen = ui.Q<VisualElement>("MenuSubscreen");
        resetSubscreen = ui.Q<VisualElement>("ResetSubscreen");

        ritchCodeTextFields = ritchCodeScreen.Query<TextField>( ).ToList( );
        
        // Set the states of the screens and subscreens based on what game controller state is active
        screens[(int) UIState.MAIN] = mainScreen;
        screens[(int) UIState.MENU] = mainScreen;
        screens[(int) UIState.RESET] = mainScreen;
        screens[(int) UIState.RITCHCODE] = ritchCodeScreen;
        screens[(int) UIState.SPLASH] = splashScreen;

        subscreens[(int) UIState.MENU] = menuSubscreen;
        subscreens[(int) UIState.RESET] = resetSubscreen;

        // Set up menu button functionality
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
            jsonManager.ActiveAppSession.TotalPoints = 0;
            ui.Q<Label>("TotalScoreLabel").text = $"0 points";

            UIControllerState = UIState.MAIN;
        };

        ui.Q<Button>("RITchCodeButton").clicked += ( ) => { UIControllerState = UIState.RITCHCODE; };
        ui.Q<Button>("BackButton").clicked += ( ) => { UIControllerState = UIState.MAIN; };
        ui.Q<Button>("RITchCodeClearButton").clicked += ( ) => {
            for (int i = 0; i < ritchCodeTextFields.Count; i++) {
                ritchCodeTextFields[i].value = "";
            }
        };
        ui.Q<Button>("RITchCodeSubmitButton").clicked += ( ) => {
            string newRITchCode = "";
            for (int i = 0; i < ritchCodeTextFields.Count; i++) {
                newRITchCode += ritchCodeTextFields[i].value;
            }

            UIControllerState = UIState.SPLASH;
        };

        // Update UI elements based on the player's score and check-in actions
        //ui.Q<Label>("CheckInLabel").text = (playerData.HasCompletedInitialCheckIn ? "Check-in completed!" : "Complete your check-in!");
        //ui.Q<VisualElement>("CheckInCheckmark").style.display = (playerData.HasCompletedInitialCheckIn ? DisplayStyle.Flex : DisplayStyle.None);
        ui.Q<Label>("TotalScoreLabel").text = $"{jsonManager.ActiveAppSession.TotalPoints} points";

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

    protected void Update( ) {

    }
}
