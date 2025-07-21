using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// A full list of all states that the UI can be. This includes the main menu and all of the games. Each state is not guarenteed to be used for each type of controller
/// </summary>
public enum UIState {
    NULL,
    MAIN, MENU, RESET, RITCHCODE, SPLASH, PLAYGOAL,
    TUTORIAL, GAME, PAUSE, WIN,
    CRAVE, CAUSE, COMPLETE
}

public abstract class UIController : MonoBehaviour {
    public static int LAST_SCENE = -1;
    public const float FADE_TRANSITION_SECONDS = 0.1f;

    private Coroutine menuTransition;

    protected VisualElement ui;
    protected VisualElement[ ] screens;
    protected VisualElement[ ] subscreens;

    /// <summary>
    /// The current screen that is visible on this controller
    /// </summary>
    public VisualElement CurrentScreen => screens[(int) UIControllerState];

    /// <summary>
    /// The current subscreen that is visible on this controller
    /// </summary>
    public VisualElement CurrentSubscreen => subscreens[(int) UIControllerState];

    /// <summary>
    /// The last screen that was visible on this controller
    /// </summary>
    public VisualElement LastScreen => screens[(int) LastUIControllerState];

    /// <summary>
    /// The last subscreen that was visible on this controller
    /// </summary>
    public VisualElement LastSubscreen => subscreens[(int) LastUIControllerState];

    /// <summary>
    /// The current state of this UI controller
    /// </summary>
    public UIState UIControllerState {
        get => _controllerState;
        set {
            // If there is currently a menu transition taking place, then do not change the game controller state
            if (IsTransitioningUI) {
                return;
            }

            LastUIControllerState = _controllerState;
            _controllerState = value;

            // Only transition between menus if the state was changed
            //if (LastUIControllerState == _controllerState) {
            //    return;
            //}

            // If the last controller state was null, then this is the start of the scene
            // Make sure to do one last update of all the subscreens to make sure they are properly visible/invisible
            if (LastUIControllerState == UIState.NULL) {
                UpdateSubscreens( );
            }

            // Transition between two screens
            // If the two screens are the same, then there must be a transition between subscreens instead of full screens
            // Subscreens are placed within screens in the UI. They are used when the entire screen does not need to transition
            if (LastScreen != CurrentScreen) {
                FadeToCurrentScreen( );
            } else {
                FadeToCurrentSubscreen( );
            }
        }
    }
    private UIState _controllerState;

    /// <summary>
    /// The last controller state
    /// </summary>
    public UIState LastUIControllerState { get => _lastControllerState; private set => _lastControllerState = value; }
    private UIState _lastControllerState;

    /// <summary>
    /// Whether or not the UI is currently doing a screen transition
    /// </summary>
    public bool IsTransitioningUI => (menuTransition != null);

    protected virtual void Awake( ) {
        ui = GetComponent<UIDocument>( ).rootVisualElement;

        // Create arrays that hold references to the screens and subscreens of each UI state
        screens = new VisualElement[Enum.GetValues(typeof(UIState)).Length];
        subscreens = new VisualElement[Enum.GetValues(typeof(UIState)).Length];
    }

    protected virtual void Start( ) {
        // Loop through all of the screens and subscreens and disable them at the start of the scene
        // This will allow for you to edit the UI without having to disable it manually each time you want to test the app
        for (int i = 0; i < screens.Length; i++) {
            // If the screen is not null, then disable it
            if (screens[i] != null) {
                screens[i].style.opacity = 0f;
                screens[i].style.display = DisplayStyle.None;
            }

            // If the subscreen is not null, then disable it
            if (subscreens[i] != null) {
                subscreens[i].style.opacity = 0f;
                subscreens[i].style.display = DisplayStyle.None;
            }
        }
    }

    protected virtual void Update( ) {
        JSONManager.ActiveAppSession.PlaytimeSeconds += Time.deltaTime;
    }

    /// <summary>
    /// Add functions to the various event handlers within the code. These will automatically be removed when the scene changes
    /// </summary>
    protected virtual void AddEventHandlers( ) { }


    /// <summary>
    /// Update all of the subscreens in this controller based on the current controller state
    /// </summary>
    protected virtual void UpdateSubscreens( ) { }

    /// <summary>
    /// Called when a screen has finished transitioning
    /// </summary>
    protected virtual void OnScreenChange( ) { }

    /// <summary>
    /// Set a visual element's visibility
    /// </summary>
    /// <param name="element">The visual element to set the visibility of</param>
    /// <param name="isVisible">Whether or not the visual element should be visible</param>
    protected void SetElementVisibility(VisualElement element, bool isVisible) {
        element.style.opacity = (isVisible ? 1f : 0f);
        element.style.display = (isVisible ? DisplayStyle.Flex : DisplayStyle.None);
    }

    /// <summary>
    /// Transition from one screen to another by fading one out and the other in
    /// </summary>
    /// <param name="fromScreen">The screen to transition from, that is currently visible</param>
    /// <param name="toScreen">The screen to transition to</param>
    /// <param name="SubscreenUpdate">A function that is called when the subscreens are needed to be updated in the middle of the transition. This could take place when you are transitioning between two different subscreens within the same screen</param>
    protected void FadeToCurrentScreen( ) {
        // If the UI is currently transitioning, then return and do not fade the screen
        if (IsTransitioningUI) {
            return;
        }

        menuTransition = StartCoroutine(FadeToCurrentScreenTransition( ));

        if (LastUIControllerState == UIState.NULL) {
            BackgroundBubbleManager.Instance.FadeBackgroundBubblesAlpha(FADE_TRANSITION_SECONDS, true);
        } else if (UIControllerState == UIState.NULL) {
            BackgroundBubbleManager.Instance.FadeBackgroundBubblesAlpha(FADE_TRANSITION_SECONDS, false);
        }
    }

    private IEnumerator FadeToCurrentScreenTransition( ) {
        // Fade the from screen out if it exists
        if (LastScreen != null) {
            yield return StartCoroutine(FadeVisualElementOpacity(LastScreen, FADE_TRANSITION_SECONDS, false));
        }

        UpdateSubscreens( );

        // Fade in the to screen if it exists
        if (CurrentScreen != null) {
            yield return StartCoroutine(FadeVisualElementOpacity(CurrentScreen, FADE_TRANSITION_SECONDS, true));
        }

        OnScreenChange( );
        menuTransition = null;
    }

    /// <summary>
    /// Fade from the last subscreen to the current subscreen
    /// </summary>
    protected void FadeToCurrentSubscreen( ) {
        // If the UI is currently transitioning, then return and do not fade the screen
        if (IsTransitioningUI) {
            return;
        }

        menuTransition = StartCoroutine(FadeToCurrentSubscreenTransition( ));
    }

    private IEnumerator FadeToCurrentSubscreenTransition( ) {
        // Fade the from screen out if it exists
        if (LastSubscreen != null) {
            yield return StartCoroutine(FadeVisualElementOpacity(LastSubscreen, FADE_TRANSITION_SECONDS, false));
        }

        UpdateSubscreens( );

        // Fade in the to screen if it exists
        if (CurrentSubscreen != null) {
            yield return StartCoroutine(FadeVisualElementOpacity(CurrentSubscreen, FADE_TRANSITION_SECONDS, true));
        }

        OnScreenChange( );
        menuTransition = null;
    }

    /// <summary>
    /// Fade from the current screen to another Unity scene
    /// </summary>
    /// <param name="sceneBuildIndex">The build index of the scene to transition to</param>
    protected virtual void FadeToScene(int sceneBuildIndex) {
        // If the UI is currently transitioning, then return and do not fade the screen
        if (IsTransitioningUI) {
            return;
        }

        menuTransition = StartCoroutine(FadeToSceneTransition(sceneBuildIndex));
    }

    private IEnumerator FadeToSceneTransition(int sceneBuildIndex) {
        // We want all of the UI to fade out so we can transition the scenes
        UIControllerState = UIState.NULL;

        // Wait until the UI is finished transitioning
        yield return new WaitUntil(( ) => !IsTransitioningUI);

        JSONManager.ClearAllDelegates( );
        BackgroundBubbleManager.Instance.RandomizeBackgroundBubbles( );

        menuTransition = null;
        LAST_SCENE = SceneManager.GetActiveScene( ).buildIndex;
        SceneManager.LoadScene(sceneBuildIndex);
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
    /// Delay an action by a specific amount of seconds
    /// </summary>
    /// <param name="action">The function to be called after a delay</param>
    /// <param name="delaySeconds">The delay in seconds after which to call the function</param>
    protected void DelayAction(Action action, float delaySeconds) {
        StartCoroutine(DelayActionSeconds(action, delaySeconds));
    }

    private IEnumerator DelayActionSeconds(Action action, float delaySeconds) {
        yield return new WaitForSeconds(delaySeconds);

        action?.Invoke( );
    }
}
