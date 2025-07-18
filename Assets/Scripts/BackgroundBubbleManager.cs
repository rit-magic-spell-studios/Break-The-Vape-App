using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundBubbleManager : Singleton<BackgroundBubbleManager> {
    [SerializeField] private GameObject backgroundBubblePrefab;
    [SerializeField, Range(0, 5)] private int backgroundBubbleCount;

    private List<SpriteRenderer> backgroundBubbles;
    private Gradient backgroundBubbleGradient;

    protected override void Awake( ) {
        base.Awake( );

        backgroundBubbles = new List<SpriteRenderer>( );
        backgroundBubbleGradient = new Gradient( );
    }

    private void Start( ) {

        for (int i = 0; i < backgroundBubbleCount; i++) {
            backgroundBubbles.Add(Instantiate(backgroundBubblePrefab, transform).GetComponent<SpriteRenderer>( ));
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
            backgroundBubbles[i].color = backgroundBubbleGradient.Evaluate((float) i / (backgroundBubbleCount - 1));
        }
    }

    /// <summary>
    /// Set the alpha of all the background bubbles
    /// </summary>
    /// <param name="alpha">The alpha to set the background bubbles to</param>
    private void SetBackgroundBubbleAlpha(float alpha) {
        for (int i = 0; i < backgroundBubbleCount; i++) {
            Color bubbleColor = backgroundBubbles[i].color;
            bubbleColor.a = alpha;
            backgroundBubbles[i].color = bubbleColor;
        }
    }
}
