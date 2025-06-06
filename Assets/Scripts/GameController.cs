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
    [SerializeField, Range(0f, 2f)] private float menuTransitionTime;

    private float tutorialTimer;

    protected VisualElement ui;

    protected VisualElement gameSubscreen;
    protected VisualElement tutorialSubscreen;
    protected VisualElement gameScreen;
    protected VisualElement pauseScreen;
    protected VisualElement winScreen;

    private Dictionary<GameControllerState, VisualElement> screenStates;

    protected Coroutine menuTransition;

    /// <summary>
    /// Whether or not the menu is currently transitioning
    /// </summary>
    public bool IsMenuTransitioning => menuTransition != null;

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

            // Transition the menu if the state has changed
            // menuTransition = StartCoroutine(FadeScreenTransition(screenStates[lastControllerState], screenStates[_gameControllerState]));

            // Only call this function when the controller state is actually changed
            if (lastControllerState != _gameControllerState) {
                OnGameControllerStateChanged( );
            }
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

        // Set the states of the screen based on what game controller state is active
        screenStates = new Dictionary<GameControllerState, VisualElement>( ) {
            { GameControllerState.TUTORIAL, gameScreen },
            { GameControllerState.GAME, gameScreen },
            { GameControllerState.PAUSE, pauseScreen},
            { GameControllerState.WIN, winScreen}
        };

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
    /// Transition from one screen to another by fading one out and the other in
    /// </summary>
    /// <param name="fromScreen">The screen to transition from, that is currently visible</param>
    /// <param name="toScreen">The screen to transition to</param>
    /// <returns></returns>
    private IEnumerator FadeScreenTransition(VisualElement fromScreen, VisualElement toScreen) {
        float opacity;

        // Fade the from screen out
        if (fromScreen != null) {
            opacity = fromScreen.resolvedStyle.opacity / 100f;

            while (opacity > 0f) {
                opacity -= Time.deltaTime / menuTransitionTime;
                fromScreen.style.opacity = opacity * 100f;

                yield return null;
            }

            fromScreen.style.opacity = 0f;
            fromScreen.style.display = DisplayStyle.None;
        }

        opacity = 0f;

        // Fade the to screen in
        if (toScreen != null) {
            toScreen.style.display = DisplayStyle.Flex;

            while (opacity < 1f) {
                opacity += Time.deltaTime / menuTransitionTime;
                toScreen.style.opacity = opacity * 100f;

                yield return null;
            }

            toScreen.style.opacity = 100f;
        }

        menuTransition = null;
    }

    /// <summary>
    /// Called whenever the game controller state is changed
    /// </summary>
    protected virtual void OnGameControllerStateChanged( ) { }
}
