using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BackgroundBubbleManager : Singleton<BackgroundBubbleManager> {
    [SerializeField] private GameObject backgroundBubblePrefab;
    [SerializeField, Range(0, 5)] private int backgroundBubbleCount;

    private List<BackgroundBubble> backgroundBubbles;
    private Gradient backgroundBubbleGradient;

    protected override void Awake( ) {
        base.Awake( );

        backgroundBubbles = new List<BackgroundBubble>( );
        backgroundBubbleGradient = new Gradient( );

        for (int i = 0; i < backgroundBubbleCount; i++) {
            backgroundBubbles.Add(Instantiate(backgroundBubblePrefab, transform).GetComponent<BackgroundBubble>( ));
        }
    }

    /// <summary>
    /// Set the colors of the background bubbles based on the new gradient colors
    /// </summary>
    /// <param name="gradientColors">A list of the gradient color keys to set</param>
    public static void SetBackgroundBubbleGradient(GradientColorKey[ ] gradientColors) {
        Instance.backgroundBubbleGradient.SetKeys(gradientColors, null);

        for (int i = 0; i < Instance.backgroundBubbleCount; i++) {
            Instance.backgroundBubbles[i].Color = Instance.backgroundBubbleGradient.Evaluate((float) i / (Instance.backgroundBubbleCount - 1));
        }
    }

    /// <summary>
    /// Set the alpha of all the background bubbles
    /// </summary>
    /// <param name="alpha">The alpha to set the background bubbles to</param>
    private void SetBackgroundBubbleAlpha(float alpha) {
        for (int i = 0; i < backgroundBubbleCount; i++) {
            backgroundBubbles[i].Alpha = alpha;
        }
    }

    /// <summary>
    /// Randomize all the values of the background bubbles
    /// </summary>
    public void RandomizeBackgroundBubbles( ) {
        for (int i = 0; i < backgroundBubbleCount; i++) {
            backgroundBubbles[i].RandomizeValues( );
        }
    }

    /// <summary>
    /// Transition the alpha of the background bubbles to either fade it in or out
    /// </summary>
    /// <param name="duration">The duration of the fade</param>
    /// <param name="fadeIn">Whether or not the background bubbles should fade in or out</param>
    public void FadeBackgroundBubblesAlpha(float duration, bool fadeIn) {
        StartCoroutine(FadeBackgroundBubblesTransition(duration, fadeIn));
    }

    private IEnumerator FadeBackgroundBubblesTransition(float duration, bool fadeIn) {
        float fromAlpha = (fadeIn ? 0f : 1f);
        float toAlpha = (fadeIn ? 1f : 0f);
        int fadeDirection = (fadeIn ? 1 : -1);
        float alpha = fromAlpha;

        while ((alpha > toAlpha && fadeDirection == -1) || (alpha < toAlpha && fadeDirection == 1)) {
            alpha += fadeDirection * Time.deltaTime / duration;
            SetBackgroundBubbleAlpha(alpha);
            yield return null;
        }

        SetBackgroundBubbleAlpha(toAlpha);
    }
}
