using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public abstract class GameController : UIController {
    [Header("GameController")]
    [SerializeField] private RenderTexture tutorialVisual;
    [SerializeField, TextArea] private string tutorialText;

    /// <summary>
    /// The current game points for this game
    /// </summary>
    public int GamePoints {
        get => jsonManager.ActiveGameSession.Points;
        set {
            // Make sure to add the points that were gained to the total app session points
            jsonManager.ActiveAppSession.TotalPoints += value - jsonManager.ActiveGameSession.Points;
            jsonManager.ActiveGameSession.Points = value;

            // Update the score label text based on the new score value
            scoreLabel.text = $"Score: <b>{jsonManager.ActiveGameSession.Points} points</b>";
            finalScoreLabel.text = $"{jsonManager.ActiveGameSession.Points} points";
            totalScoreLabel.text = $"{jsonManager.ActiveAppSession.TotalPoints} points";
        }
    }

    protected Label scoreLabel;
    protected Label finalScoreLabel;
    protected Label totalScoreLabel;

    protected VisualElement gameScreen;
    protected VisualElement pauseScreen;
    protected VisualElement winScreen;

    protected VisualElement gameSubscreen;
    protected VisualElement tutorialSubscreen;


    protected override void Awake( ) {
        base.Awake( );

        // Get all screens within the game
        // Each game should have the same types of screens, but more functionality will need to be added individually
        gameSubscreen = ui.Q<VisualElement>("GameSubscreen");
        tutorialSubscreen = ui.Q<VisualElement>("TutorialSubscreen");
        gameScreen = ui.Q<VisualElement>("GameScreen");
        pauseScreen = ui.Q<VisualElement>("PauseScreen");
        winScreen = ui.Q<VisualElement>("WinScreen");

        // Set the states of the screens and subscreens based on what game controller state is active
        screens[(int) UIState.TUTORIAL] = gameScreen;
        screens[(int) UIState.GAME] = gameScreen;
        screens[(int) UIState.PAUSE] = pauseScreen;
        screens[(int) UIState.WIN] = winScreen;

        subscreens[(int) UIState.TUTORIAL] = tutorialSubscreen;
        subscreens[(int) UIState.GAME] = gameSubscreen;

        // Set all common button functions
        // Each game should also have these buttons
        ui.Q<Button>("ResumeButton").clicked += ( ) => { UIControllerState = LastUIControllerState; };
        ui.Q<Button>("QuitButton").clicked += ( ) => { FadeToScene(0); };
        ui.Q<Button>("HomeButton").clicked += ( ) => { FadeToScene(0); };
        ui.Q<Button>("PlayAgainButton").clicked += ( ) => { FadeToScene(SceneManager.GetActiveScene( ).buildIndex); };
        ui.Q<Button>("PauseButton").clicked += ( ) => { UIControllerState = UIState.PAUSE; };
        ui.Q<Button>("PlayButton").clicked += ( ) => { UIControllerState = UIState.GAME; };
        ui.Q<Button>("HowToPlayButton").clicked += ( ) => { UIControllerState = UIState.TUTORIAL; };

        // Get references to other important UI elements
        scoreLabel = ui.Q<Label>("ScoreLabel");
        finalScoreLabel = ui.Q<Label>("FinalScoreLabel");
        totalScoreLabel = ui.Q<Label>("TotalScoreLabel");

        // Set tutorial information
        ui.Q<VisualElement>("TutorialVisual").style.backgroundImage = new StyleBackground(Background.FromRenderTexture(tutorialVisual));
        ui.Q<Label>("TutorialLabel").text = tutorialText;
        ui.Q<Label>("TitleLabel").text = name;

        // Create a new game session data entry for this game
        jsonManager.ActiveAppSession.GameSessionData.Add(new GameSessionData( ));
        jsonManager.ActiveGameSession.Name = name;

        // Set default values for some of the variables
        GamePoints = 0;
    }

    protected override void FadeToScene(int sceneBuildIndex) {
        jsonManager.SavePlayerData( );

        base.FadeToScene(sceneBuildIndex);
    }

    /// <summary>
    /// Update all of the subscreens in this game controller based on the current game controller state
    /// </summary>
    protected override void UpdateSubscreens( ) {
        SetSubscreenVisibility(gameSubscreen, UIControllerState == UIState.GAME);
        SetSubscreenVisibility(tutorialSubscreen, UIControllerState == UIState.TUTORIAL);
    }
}
