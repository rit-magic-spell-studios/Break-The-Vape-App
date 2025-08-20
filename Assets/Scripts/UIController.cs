using DG.Tweening;
using DG.Tweening.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public abstract class UIController : MonoBehaviour {
    [Header("UIController")]
    [SerializeField] protected Camera mainCamera;
    [Space]
    [SerializeField] private Gradient backgroundGradient;

    public const float SCREEN_TRANSITION_SECONDS = 0.25f;
    public const float POPUP_TRANSITION_SECONDS = 0.5f;
    public const float GAME_WIN_DELAY_SECONDS = 1.5f;
    public const int PLAY_GOAL_SECONDS = 900;
    public static string LastSceneName { get; private set; }

    protected float cameraHalfWidth;
    protected float cameraHalfHeight;
    protected float safeAreaTopPadding;
    protected float safeAreaBottomPadding;

    protected VisualElement ui;
    private List<VisualElement> animatingVisualElements;

    protected VisualElement popupOverlay;
    private Vector2 currentPopupOnScreen;
    private Vector2 currentPopupOffScreen;

    protected VisualElement transitionOverlay;
    private VisualElement previousScreen;

    public VisualElement CurrentScreen { get; private set; }
    public VisualElement CurrentPopup { get; private set; }
    public bool IsTouchingScreen { get; private set; }

    /// <summary>
    /// The last position of touch input or the last mouse position on the screen
    /// </summary>
    public Vector3 LastTouchWorldPosition { get; private set; }

    protected virtual void Awake( ) {
        ui = GetComponent<UIDocument>( ).rootVisualElement;
        ui.Query(className: "uofr-safe-area__top-padding").ForEach(x => x.RegisterCallback((GeometryChangedEvent e) => {
            VisualElement element = (VisualElement) e.target;
            element.style.paddingTop = Mathf.Max(element.resolvedStyle.paddingTop, safeAreaTopPadding);
        }));
        ui.Query(className: "uofr-safe-area__bottom-padding").ForEach(x => x.RegisterCallback((GeometryChangedEvent e) => {
            VisualElement element = (VisualElement) e.target;
            element.style.paddingBottom = Mathf.Max(element.resolvedStyle.paddingBottom, safeAreaBottomPadding);
        }));

        popupOverlay = ui.Q<VisualElement>("PopupOverlay");
        transitionOverlay = ui.Q<VisualElement>("TransitionOverlay");
        animatingVisualElements = new List<VisualElement>( );

        cameraHalfHeight = mainCamera.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * mainCamera.aspect;
        safeAreaBottomPadding = Screen.safeArea.y;
        safeAreaTopPadding = Screen.height - Screen.safeArea.height - safeAreaBottomPadding;
    }

    protected virtual void Start( ) {
        BackgroundBubbleManager.SetBackgroundBubbleGradient(backgroundGradient.colorKeys);
    }

    protected virtual void Update( ) {
        DataManager.AppSessionData.TotalTimeSecondsValue += Time.deltaTime;
        UpdateTouchInput( );
    }

    /// <summary>
    /// Update the touch input position based on either a touchscreen or mouse input
    /// </summary>
    public void UpdateTouchInput( ) {
        if (Input.touchCount == 1) {
            Touch touch = Input.GetTouch(0);
            LastTouchWorldPosition = (Vector2) mainCamera.ScreenToWorldPoint(touch.position);
            IsTouchingScreen = true;
        } else if (Input.GetMouseButton(0)) {
            LastTouchWorldPosition = (Vector2) mainCamera.ScreenToWorldPoint(Input.mousePosition);
            IsTouchingScreen = true;
        } else {
            IsTouchingScreen = false;
        }
    }

    /// <summary>
    /// Set a specific screen to be displayed
    /// </summary>
    /// <param name="screen">The screen to display</param>
    /// <param name="onComplete">An optional callback function that will be called when the screen transition is complete</param>
    protected void DisplayScreen(VisualElement screen, Action onHalfway = null, Action onComplete = null) {
        // Stop new transitions if there is a transition currently happening
        if (animatingVisualElements.Count > 0) {
            return;
        }

        previousScreen = CurrentScreen;
        CurrentScreen = screen;

        Sequence transitionSequence = DOTween.Sequence( )
            .OnComplete(( ) => { onComplete?.Invoke( ); });

        if (previousScreen != null) {
            transitionSequence.Append(AnimateElementOpacity(transitionOverlay, 0f, 1f, SCREEN_TRANSITION_SECONDS));
            transitionSequence.AppendCallback(( ) => { previousScreen.style.visibility = Visibility.Hidden; });
        }

        transitionSequence.AppendCallback(( ) => { onHalfway?.Invoke( ); });

        if (CurrentScreen != null) {
            transitionSequence.AppendCallback(( ) => { CurrentScreen.style.visibility = Visibility.Visible; });
            transitionSequence.Append(AnimateElementOpacity(transitionOverlay, 1f, 0f, SCREEN_TRANSITION_SECONDS, disableOnComplete: true, setDefaultsBeforeTweenStart: previousScreen == null));
        }
    }

    /// <summary>
    /// Transition to a specific scene
    /// </summary>
    /// <param name="sceneName">The name of the scene to transition to</param>
    protected virtual void GoToScene(string sceneName) {
        DisplayScreen(null, onComplete: ( ) => {
            DataManager.AppSessionData.ClearAllDelegates( );
            BackgroundBubbleManager.Instance.RandomizeBackgroundBubbles( );
            LastSceneName = SceneManager.GetActiveScene( ).name;
            SceneManager.LoadScene(sceneName);
        });
    }

    /// <summary>
    /// Display a basic popup on the screen. This means that the popup will start below the screen and transition to the center of the screen
    /// </summary>
    /// <param name="popup">The popup to display</param>
    /// <param name="checkForAnimations">Whether or not to check for animations before displaying a new popup. If there are other animations playing and this is set to true, then the new popup will not be displayed</param>
    protected void DisplayBasicPopup(VisualElement popup, bool checkForAnimations = true, Action onComplete = null) {
        DisplayPopup(popup, Vector2.zero, new Vector2(0, Screen.height), checkForAnimations: checkForAnimations, onComplete: onComplete);
    }

    /// <summary>
    /// Hide the currently displayed popup
    /// </summary>
    /// <param name="checkForAnimations">Whether or not to check for animations before displaying a new popup. If there are other animations playing and this is set to true, then the new popup will not be displayed</param>
    protected void HideCurrentPopup(bool checkForAnimations = true, Action onComplete = null) {
        DisplayPopup(null, checkForAnimations: checkForAnimations, onComplete: onComplete);
    }

    /// <summary>
    /// Display a popup on the screen
    /// </summary>
    /// <param name="popup">The popup to display</param>
    /// <param name="onScreen">The translation position of the popup where it should be when it is on the screen</param>
    /// <param name="offScreen">The translation position of the popup where it should be when it is off the screen</param>
    /// <param name="checkForAnimations">Whether or not to check for animations before displaying a new popup. If there are other animations playing and this is set to true, then the new popup will not be displayed</param>
    protected void DisplayPopup(VisualElement popup, Vector2 onScreen = default, Vector2 offScreen = default, bool checkForAnimations = true, Action onComplete = null) {
        // Stop new transitions if there is a transition currently happening
        if ((checkForAnimations && animatingVisualElements.Count > 0) || popup == CurrentPopup) {
            return;
        }

        if (popup != null) {
            // If the new popup is not null and there is not a current popup, then transition the popup and the background in
            // if the new popup is not null and there is a current popup, then transition the current popup out and the new popup in without touching the background
            if (CurrentPopup == null) {
                AnimateElementOpacity(popupOverlay, 0f, 1f, POPUP_TRANSITION_SECONDS);
                AnimateElementTranslation(popup, offScreen, onScreen, POPUP_TRANSITION_SECONDS, onComplete: onComplete);
            } else {
                AnimateElementTranslation(CurrentPopup, currentPopupOnScreen, currentPopupOffScreen, POPUP_TRANSITION_SECONDS, disableOnComplete: true);
                AnimateElementTranslation(popup, offScreen, onScreen, POPUP_TRANSITION_SECONDS, onComplete: onComplete);
            }
        } else {
            // If the new popup is null and there is a current popup, then transition the current popup out as well as the background because there will be no more popup visible
            // If the new popup is nulla nd there is not a current popup, then do nothing because no popup is visible
            if (CurrentPopup != null) {
                AnimateElementOpacity(popupOverlay, 1f, 0f, POPUP_TRANSITION_SECONDS, disableOnComplete: true);
                AnimateElementTranslation(CurrentPopup, currentPopupOnScreen, currentPopupOffScreen, POPUP_TRANSITION_SECONDS, disableOnComplete: true, onComplete: onComplete);
            }
        }

        // Set the current popup to whatever the new popup is
        CurrentPopup = popup;
        currentPopupOnScreen = onScreen;
        currentPopupOffScreen = offScreen;
    }

    /// <summary>
    /// Animate a visual element's translation style variable using DOTween
    /// </summary>
    /// <param name="element">The visual element to animate</param>
    /// <param name="startTranslation">The starting translation of the visual element</param>
    /// <param name="endTranslation">The ending translation of the visual element</param>
    /// <param name="durationSeconds">The time in seconds it takes to complete the animation</param>
    /// <param name="disableOnComplete">Whether or not to disable the visual element when the animation is completed</param>
    /// <param name="setDefaultsBeforeTweenStart">Whether or not to set default values of the visual element inside this function or inside the tween's OnStart function</param>
    /// <returns>A reference to the Tween that is animating the visual element</returns>
    private Tween AnimateElementTranslation(VisualElement element, Vector2 startTranslation, Vector2 endTranslation, float durationSeconds, bool disableOnComplete = false, bool setDefaultsBeforeTweenStart = true, Action onComplete = null) {
        Action onStart = ( ) => {
            element.style.display = DisplayStyle.Flex;
            element.style.translate = new StyleTranslate(new Translate(startTranslation.x, startTranslation.y));
            animatingVisualElements.Add(element);
        };

        if (setDefaultsBeforeTweenStart) {
            onStart.Invoke( );
        }

        return DOTween.To(
            ( ) => (Vector2) element.resolvedStyle.translate,
            (v) => element.style.translate = new StyleTranslate(new Translate(v.x, v.y)),
            endTranslation,
            durationSeconds)
            .SetEase(Ease.InOutCubic)
            .OnStart(( ) => {
                if (!setDefaultsBeforeTweenStart) {
                    onStart.Invoke( );
                }
            })
            .OnComplete(( ) => {
                if (disableOnComplete) {
                    element.style.display = DisplayStyle.None;
                }
                animatingVisualElements.Remove(element);
                onComplete?.Invoke( );
            });
    }

    /// <summary>
    /// Animate a visual element's background color style variable using DOTween
    /// </summary>
    /// <param name="element">The visual element to animate</param>
    /// <param name="startColor">The starting background color of the visual element</param>
    /// <param name="endColor">The ending background color of the visual element</param>
    /// <param name="durationSeconds">The time in seconds it takes to complete the animation</param>
    /// <param name="disableOnComplete">Whether or not to disable the visual element when the animation is completed</param>
    /// <param name="setDefaultsBeforeTweenStart">Whether or not to set default values of the visual element inside this function or inside the tween's OnStart function</param>
    /// <returns>A reference to the Tween that is animating the visual element</returns>
    private Tween AnimateElementBackgroundColor(VisualElement element, Color startColor, Color endColor, float durationSeconds, bool disableOnComplete = false, bool setDefaultsBeforeTweenStart = true, Action onComplete = null) {
        Action onStart = ( ) => {
            element.style.display = DisplayStyle.Flex;
            element.style.backgroundColor = new StyleColor(startColor);
            animatingVisualElements.Add(element);
        };

        if (setDefaultsBeforeTweenStart) {
            onStart.Invoke( );
        }

        return DOTween.To(
            ( ) => element.resolvedStyle.backgroundColor,
            (v) => element.style.backgroundColor = new StyleColor(v),
            endColor,
            durationSeconds)
            .SetEase(Ease.InOutCubic)
            .OnStart(( ) => {
                if (!setDefaultsBeforeTweenStart) {
                    onStart.Invoke( );
                }
            })
            .OnComplete(( ) => {
                if (disableOnComplete) {
                    element.style.display = DisplayStyle.None;
                }
                animatingVisualElements.Remove(element);
                onComplete?.Invoke( );
            });
    }

    /// <summary>
    /// Animate a visual element's opacity style variable using DOTween
    /// </summary>
    /// <param name="element">The visual element to animate</param>
    /// <param name="startOpacity">The starting opacity of the visual element</param>
    /// <param name="endOpacity">The ending opacity of the visual element</param>
    /// <param name="durationSeconds">The time in seconds it takes to complete the animation</param>
    /// <param name="disableOnComplete">Whether or not to disable the visual element when the animation is completed</param>
    /// <param name="setDefaultsBeforeTweenStart">Whether or not to set default values of the visual element inside this function or inside the tween's OnStart function</param>
    /// <returns>A reference to the Tween that is animating the visual element</returns>
    private Tween AnimateElementOpacity(VisualElement element, float startOpacity, float endOpacity, float durationSeconds, bool disableOnComplete = false, bool setDefaultsBeforeTweenStart = true, Action onComplete = null) {
        Action onStart = ( ) => {
            element.style.display = DisplayStyle.Flex;
            element.style.opacity = startOpacity;
            animatingVisualElements.Add(element);
        };

        if (setDefaultsBeforeTweenStart) {
            onStart.Invoke( );
        }

        return DOTween.To(
            ( ) => element.resolvedStyle.opacity,
            (v) => element.style.opacity = v,
            endOpacity,
            durationSeconds)
            .SetEase(Ease.InOutCubic)
            .OnStart(( ) => {
                if (!setDefaultsBeforeTweenStart) {
                    onStart.Invoke( );
                }
            })
            .OnComplete(( ) => {
                if (disableOnComplete) {
                    element.style.display = DisplayStyle.None;
                }
                animatingVisualElements.Remove(element);
                onComplete?.Invoke( );
            });
    }

    /// <summary>
    /// Delay an action by a specific amount of seconds
    /// </summary>
    /// <param name="action">The function to be called after a delay</param>
    /// <param name="delaySeconds">The delay in seconds after which to call the function</param>
    public void DelayAction(Action action, float delaySeconds) {
        StartCoroutine(DelayActionCoroutine(action, delaySeconds));
    }

    private IEnumerator DelayActionCoroutine(Action action, float delaySeconds) {
        yield return new WaitForSeconds(delaySeconds);
        action?.Invoke( );
    }
}
