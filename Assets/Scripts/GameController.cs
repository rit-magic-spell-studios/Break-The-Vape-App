using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public enum GameControllerState {
    TUTORIAL, GAME, PAUSE, WIN
}

public abstract class GameController : MonoBehaviour {
    [Header("GameController")]
    [SerializeField, Range(0f, 10f)] private float tutorialTime;

    private float tutorialTimer;

    protected VisualElement ui;

    protected VisualElement gameSubscreen;
    protected VisualElement tutorialSubscreen;
    protected VisualElement gameScreen;
    protected VisualElement pauseScreen;
    protected VisualElement winScreen;

    /// <summary>
    /// The current state of this game controller
    /// </summary>
    public GameControllerState GameControllerState {
        get => _gameControllerState;
        set {
            lastControllerState = _gameControllerState;
            _gameControllerState = value;

            // Update UI elements to be visible or invisible based on the new game state
            gameSubscreen.style.display = (_gameControllerState == GameControllerState.GAME ? DisplayStyle.Flex : DisplayStyle.None);
            tutorialSubscreen.style.display = (_gameControllerState == GameControllerState.TUTORIAL ? DisplayStyle.Flex : DisplayStyle.None);
            gameScreen.style.display = (_gameControllerState == GameControllerState.GAME || _gameControllerState == GameControllerState.TUTORIAL ? DisplayStyle.Flex : DisplayStyle.None);
            pauseScreen.style.display = (_gameControllerState == GameControllerState.PAUSE ? DisplayStyle.Flex : DisplayStyle.None);
            winScreen.style.display = (_gameControllerState == GameControllerState.WIN ? DisplayStyle.Flex : DisplayStyle.None);

            OnGameControllerStateChanged( );
        }
    }
    private GameControllerState _gameControllerState;
    private GameControllerState lastControllerState;

    protected virtual void Awake( ) {
        ui = GetComponent<UIDocument>( ).rootVisualElement;

        // Get all screens within the game
        // Each game should have the same types of screens, but more functionality will need to be added individually
        gameSubscreen = ui.Q<VisualElement>("GameSubscreen");
        tutorialSubscreen = ui.Q<VisualElement>("TutorialSubscreen");
        gameScreen = ui.Q<VisualElement>("GameScreen");
        pauseScreen = ui.Q<VisualElement>("PauseScreen");
        winScreen = ui.Q<VisualElement>("WinScreen");

        // Set all common button functions
        // Each game should also have these buttons
        ui.Q<Button>("ResumeButton").clicked += ( ) => { GameControllerState = lastControllerState; };
        ui.Q<Button>("QuitButton").clicked += ( ) => { SceneManager.LoadScene(0); };
        ui.Q<Button>("MainMenuButton").clicked += ( ) => { SceneManager.LoadScene(0); };
        ui.Q<Button>("PauseButton").clicked += ( ) => { GameControllerState = GameControllerState.PAUSE; };

        // When the game starts, the tutorial should be shown first
        GameControllerState = GameControllerState.TUTORIAL;
        tutorialTimer = 0f;
    }

    private void Update( ) {
        // If the current game controller state is not the tutorial, then do not update the tutorial timer
        if (GameControllerState != GameControllerState.TUTORIAL) {
            return;
        }

        tutorialTimer += Time.deltaTime;

        // Once the tutorial timer has reached the alloted tutorial time, then switch the game state to show the game
        if (tutorialTimer > tutorialTime) {
            GameControllerState = GameControllerState.GAME;
        }
    }

    /// <summary>
    /// Called whenever the game controller state is changed
    /// </summary>
    protected virtual void OnGameControllerStateChanged( ) { }
}
