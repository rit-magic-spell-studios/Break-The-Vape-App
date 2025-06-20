using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MatchAndCatchController : GameController {
    [Header("MatchAndCatchController")]
    [SerializeField, Range(0f, 2f)] private float cardFlipTime;
    [SerializeField, Range(0f, 2f)] private float cardCheckDelay;
    [SerializeField] private List<Sprite> cardFrontSprites;

    private List<Button> cards;
    private List<Button> flippedCards;
    private List<int> numbers;

    private Label matchesLabel;

    private Coroutine checkMatchCoroutine;

    protected override void Awake( ) {
        base.Awake( );

        // Get a list of all the cards in the UI
        cards = ui.Query<Button>("Card").ToList( );
        flippedCards = new List<Button>( );

        // Generate the list of matches
        numbers = new List<int>( );
        for (int i = 0; i < cards.Count / 2; i++) {
            // Each card needs a match, so add two of the same number to the list
            numbers.Add(i);
            numbers.Add(i);
        }

        // Shuffle the numbers up to make the matching randomized
        // The numbers and cards list should be the same size so all indices of the numbers list corresponds to the cards list
        Shuffle(numbers);

        // Set up the on-click functions for each of the cards
        for (int i = 0; i < cards.Count; i++) {
            cards[i].clickable.clickedWithEventInfo += FlipCard;

            // Set the card back to be visible
            VisualElement cardBack = cards[i].Q<VisualElement>("CardBack");
            cardBack.style.display = DisplayStyle.Flex;

            // Set the card front to not be visible
            // Also make sure it displays the right sprite
            VisualElement cardFront = cards[i].Q<VisualElement>("CardFront");
            cardFront.style.display = DisplayStyle.None;
            cardFront.style.backgroundImage = new StyleBackground(cardFrontSprites[numbers[i]]);
        }

        // Get references to other important UI elements
        matchesLabel = ui.Q<Label>("MatchesLabel");
        matchesLabel.text = $"{(cards.Count - flippedCards.Count) / 2} matches left!";
    }

    protected override void Start( ) {
        base.Start( );

        // When the game starts, the tutorial should be shown first
        UIControllerState = UIState.TUTORIAL;
    }

    /// <summary>
    /// Flip a card over to see what it has written on its face
    /// </summary>
    /// <param name="e">Event data about the click event from the card button</param>
    private void FlipCard(EventBase e) {
        // If the game is currently checking for a match, then do not flip a card
        // Need to wait until after the match is checked to flip a new card
        if (checkMatchCoroutine != null) {
            return;
        }

        Button card = (Button) e.target;

        // If this card is currently flipped, then return and do not try to flip it again
        if (flippedCards.Contains(card)) {
            return;
        }

        // Since this card is now flipped over, add it to the flipped cards list
        flippedCards.Add(card);

        // Play an animation of the card flipping over
        StartCoroutine(FlipCardAnimation(card, true));
    }

    /// <summary>
    /// An animation that plays of the card flipping over
    /// </summary>
    /// <param name="card">The card button element to flip over</param>
    /// <param name="faceVisible">Whether or not the face of the card is visible after the animation</param>
    /// <returns></returns>
    private IEnumerator FlipCardAnimation(Button card, bool faceVisible) {
        // If there was an even number of cards flipped over, make sure all cards have a match
        // Also make sure that the cards face is being turned to visible since this method is reused for flipping the cards back over
        if (faceVisible && flippedCards.Count > 0 && flippedCards.Count % 2 == 0) {
            checkMatchCoroutine = StartCoroutine(CheckMatches( ));
        }

        float scale = 1f;
        bool setFlipStyles = false;

        while (scale > -1f) {
            scale -= Time.deltaTime / (cardFlipTime / 2f);
            card.style.scale = new StyleScale(new Vector2(Mathf.Abs(scale), 1f));

            // Once the card has flipped over, change the background color to give the illusion that the card has flipped
            if (scale <= 0f && !setFlipStyles) {
                // Whether or not the card is now visible, set the label and background color of the card
                if (faceVisible) {
                    card.Q<VisualElement>("CardBack").style.display = DisplayStyle.None;
                    card.Q<VisualElement>("CardFront").style.display = DisplayStyle.Flex;
                } else {
                    card.Q<VisualElement>("CardBack").style.display = DisplayStyle.Flex;
                    card.Q<VisualElement>("CardFront").style.display = DisplayStyle.None;
                }

                // Ensure that this if statement is not called multiple times within the while loop, only when the card is initially turned over
                setFlipStyles = true;
            }

            yield return null;
        }

        card.style.scale = new StyleScale(Vector2.one);
    }

    /// <summary>
    /// Check for a match after 2 cards have been turned over (with a slight delay)
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckMatches( ) {
        // Get the index of the last two cards
        Button card1 = flippedCards[^1];
        Button card2 = flippedCards[^2];

        // If the numbers of the last two cards do not match, then flip them back over
        if (numbers[cards.IndexOf(card1)] != numbers[cards.IndexOf(card2)]) {
            // Wait before checking the card match so the player can see what they turned over
            yield return new WaitForSeconds(cardCheckDelay);

            // Since this card is now not flipped over, remove it from the flipped cards
            flippedCards.Remove(card1);
            flippedCards.Remove(card2);

            // Flip both of the cards over
            // We only need to wait for the second card to be finished because the card animations take the same amount of time
            StartCoroutine(FlipCardAnimation(card1, false));
            yield return StartCoroutine(FlipCardAnimation(card2, false));
        } else {
            // Update the matches left label
            matchesLabel.text = $"{(cards.Count - flippedCards.Count) / 2} matches left!";

            // Add points for the correct match
            AddPoints(100);

            // If all of the cards have been flipped over, then the player has won
            if (flippedCards.Count == cards.Count) {
                // Wait a second to give the player a chance to look at the cards
                yield return new WaitForSeconds(cardCheckDelay);

                UIControllerState = UIState.WIN;
            }
        }

        checkMatchCoroutine = null;
    }

    /// https://discussions.unity.com/t/clever-way-to-shuffle-a-list-t-in-one-line-of-c-code/535113
    /// <summary>
	/// Shuffles the element order of the specified list.
	/// </summary>
    /// <param name="list">The list to shuffle</param>
	private void Shuffle(IList list) {
        var count = list.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i) {
            var randomIndex = Random.Range(i, count);
            var tmp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = tmp;
        }
    }
}
