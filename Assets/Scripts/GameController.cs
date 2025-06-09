using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public enum GameControllerState {
    NULL, TUTORIAL, GAME, PAUSE, WIN
}

public abstract class GameController : MonoBehaviour {
    [Header("GameController")]
    [SerializeField, Range(0f, 10f)] private float tutorialTime;
    [SerializeField, Range(0f, 2f)] private float menuTransitionTime;

    private float tutorialTimer;

    protected VisualElement ui;

    protected Label scoreLabel;
    protected Label finalScoreLabel;

    private Dictionary<GameControllerState, VisualElement> subscreenStates;
    protected VisualElement gameSubscreen;
    protected VisualElement tutorialSubscreen;

    private Dictionary<GameControllerState, VisualElement> screenStates;
    protected VisualElement gameScreen;
    protected VisualElement pauseScreen;
    protected VisualElement winScreen;

    protected Coroutine menuTransition;

    /// <summary>
    /// The current score of the game
    /// </summary>
    public int Score {
        get => _score;
        set {
            _score = value;

            // Update the score label text based on the new score value
            scoreLabel.text = $"Score: <b>{_score} points</b>";
            finalScoreLabel.text = $"{_score} points";
        }
    }
    private int _score;

    /// <summary>
    /// The current state of this game controller
    /// </summary>
    public GameControllerState GameControllerState {
        get => _gameControllerState;
        set {
            // If there is currently a menu transition taking place, then do not change the game controller state
            if (menuTransition != null) {
                return;
            }

            _lastControllerState = _gameControllerState;
            _gameControllerState = value;

            // Only transition between menus if the state was changed
            if (_lastControllerState == _gameControllerState) {
                return;
            }

            // Transition between the different menus
            VisualElement fromScreen = screenStates.GetValueOrDefault(_lastControllerState);
            VisualElement toScreen = screenStates.GetValueOrDefault(_gameControllerState);

            // If the two screens are the same, then there must be a transition between subscreens instead of full screens
            // Subscreens are placed within screens in the UI. They are used when the entire screen does not need to transition
            if (fromScreen != toScreen) {
                menuTransition = StartCoroutine(FadeScreenTransition(
                    screenStates.GetValueOrDefault(_lastControllerState),
                    screenStates.GetValueOrDefault(_gameControllerState)
                ));
            } else {
                menuTransition = StartCoroutine(FadeScreenTransition(
                    subscreenStates.GetValueOrDefault(_lastControllerState),
                    subscreenStates.GetValueOrDefault(_gameControllerState),
                    subscreens: true
                ));
            }

            // Since the game controller state was set to something new, call this update function
            OnGameControllerStateChanged( );
        }
    }
    private GameControllerState _gameControllerState;
    private GameControllerState _lastControllerState;

    protected virtual void Awake( ) {
        ui = GetComponent<UIDocument>( ).rootVisualElement;

        // Get all screens within the game
        // Each game should have the same types of screens, but more functionality will need to be added individually
        gameSubscreen = ui.Q<VisualElement>("GameSubscreen");
        tutorialSubscreen = ui.Q<VisualElement>("TutorialSubscreen");
        gameScreen = ui.Q<VisualElement>("GameScreen");
        pauseScreen = ui.Q<VisualElement>("PauseScreen");
        winScreen = ui.Q<VisualElement>("WinScreen");

        // Set the states of the screens and subscreens based on what game controller state is active
        screenStates = new Dictionary<GameControllerState, VisualElement>( ) {
            { GameControllerState.TUTORIAL, gameScreen },
            { GameControllerState.GAME, gameScreen },
            { GameControllerState.PAUSE, pauseScreen},
            { GameControllerState.WIN, winScreen}
        };
        subscreenStates = new Dictionary<GameControllerState, VisualElement>( ) {
            { GameControllerState.TUTORIAL, tutorialSubscreen },
            { GameControllerState.GAME, gameSubscreen }
        };

        // Set all common button functions
        // Each game should also have these buttons
        ui.Q<Button>("ResumeButton").clicked += ( ) => { GameControllerState = _lastControllerState; };
        ui.Q<Button>("QuitButton").clicked += ( ) => { SceneManager.LoadScene(0); };
        ui.Q<Button>("HomeButton").clicked += ( ) => { SceneManager.LoadScene(0); };
        ui.Q<Button>("PlayAgainButton").clicked += ( ) => { SceneManager.LoadScene(SceneManager.GetActiveScene( ).buildIndex); };
        ui.Q<Button>("PauseButton").clicked += ( ) => { GameControllerState = GameControllerState.PAUSE; };

        // Get references to other important UI elements
        scoreLabel = ui.Q<Label>("ScoreLabel");
        finalScoreLabel = ui.Q<Label>("FinalScoreLabel");

        // When the game starts, the tutorial should be shown first
        GameControllerState = GameControllerState.TUTORIAL;

        // Set default values for some of the variables
        tutorialTimer = 0f;
        Score = 0;
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
    /// <param name="subscreens">Whether or not the screens passed in as arguments for this function are subscreens</param>
    /// <returns></returns>
    private IEnumerator FadeScreenTransition(VisualElement fromScreen, VisualElement toScreen, bool subscreens = false) {
        // Fade the from screen out if it exists
        if (fromScreen != null) {
            yield return StartCoroutine(FadeVisualElementOpacity(fromScreen, menuTransitionTime, false));
        }

        // Set subscreen visibility only if screens are transitioning
        if (!subscreens) {
            switch (GameControllerState) {
                case GameControllerState.TUTORIAL:
                    gameSubscreen.style.opacity = 0f;
                    gameSubscreen.style.display = DisplayStyle.None;
                    tutorialSubscreen.style.opacity = 1f;
                    tutorialSubscreen.style.display = DisplayStyle.Flex;

                    break;
                case GameControllerState.GAME:
                    gameSubscreen.style.opacity = 1f;
                    gameSubscreen.style.display = DisplayStyle.Flex;
                    tutorialSubscreen.style.opacity = 0f;
                    tutorialSubscreen.style.display = DisplayStyle.None;

                    break;
            }
        }

        // Fade in the to screen if it exists
        if (toScreen != null) {
            yield return StartCoroutine(FadeVisualElementOpacity(toScreen, menuTransitionTime, true));
        }

        menuTransition = null;
    }

    /// <summary>
    /// Fade a visual element's opacity
    /// </summary>
    /// <param name="element">The visual element to fade</param>
    /// <param name="duration">The duration of the fade in seconds</param>
    /// <param name="fadeIn">Whether or not the fade the element in or out. If this value is true, the element will start at 0 opacity and fade in all the way to 1 opacity</param>
    /// <returns></returns>
    private IEnumerator FadeVisualElementOpacity(VisualElement element, float duration, bool fadeIn) {
        // Get the opacity values based on if the element is fading in or out
        float fromOpacity = (fadeIn ? 0f : 1f);
        float toOpacity = (fadeIn ? 1f : 0f);

        // Set the display of the visual element to always be visible at the beginning
        // This is because the transition is either going to fade in the element or fade out the element, both of which the element needs to be visible
        element.style.opacity = fromOpacity;
        element.style.display = DisplayStyle.Flex;

        // Get the direction that this function needs to fade based on if the element is fading in or out
        int fadeDirection = (fadeIn ? 1 : -1);

        // Linearly interpolate between the two opacity values
        float opacity = fromOpacity;
        while ((opacity > toOpacity && fadeDirection == -1) || (opacity < toOpacity && fadeDirection == 1)) {
            opacity += fadeDirection * Time.deltaTime / duration;
            element.style.opacity = opacity;

            yield return null;
        }

        // Set the final states of the visual element based on the opacity
        element.style.opacity = toOpacity;
        element.style.display = (fadeIn ? DisplayStyle.Flex : DisplayStyle.None);
    }

    /// <summary>
    /// Called whenever the game controller state is changed
    /// </summary>
    protected virtual void OnGameControllerStateChanged( ) { }
}
