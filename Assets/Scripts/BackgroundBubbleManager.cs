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
    }

    private void Start( ) {
        for (int i = 0; i < backgroundBubbleCount; i++) {
            backgroundBubbles.Add(Instantiate(backgroundBubblePrefab, transform).GetComponent<BackgroundBubble>( ));
        }

        SetBackgroundBubbleGradient(new GradientColorKey[ ] {
             new GradientColorKey(new Color(1f, 0.25f, 0.611f), 0f),
             new GradientColorKey(new Color(1f, 0.85f, 0.25f), 0.5f),
             new GradientColorKey(new Color(0.25f, 0.26f, 1f), 1f)
        });
    }

    /// <summary>
    /// Set the colors of the background bubbles based on the new gradient colors
    /// </summary>
    /// <param name="gradientColors">A list of the gradient color keys to set</param>
    private void SetBackgroundBubbleGradient(GradientColorKey[ ] gradientColors) {
        backgroundBubbleGradient.SetKeys(gradientColors, null);

        for (int i = 0; i < backgroundBubbleCount; i++) {
            backgroundBubbles[i].Color = backgroundBubbleGradient.Evaluate((float) i / (backgroundBubbleCount - 1));
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
