using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MatchAndCatchController : MonoBehaviour {
    private VisualElement ui;
    private VisualElement gameContainer;
    private VisualElement endContainer;

    private List<Button> cards;
    private List<Button> flippedCards;
    private List<int> numbers;

    private Coroutine checkMatchCoroutine;

    /// <summary>
    /// Whether or not the player has won
    /// </summary>
    public bool HasWon {
        get => _hasWon;
        set {
            _hasWon = value;

            // Enabled/disable UI based on if the player has won or not
            if (_hasWon) {
                gameContainer.AddToClassList("container-hidden");
                endContainer.RemoveFromClassList("container-hidden");
            } else {
                gameContainer.RemoveFromClassList("container-hidden");
                endContainer.AddToClassList("container-hidden");
            }
        }
    }
    private bool _hasWon;

    private void Awake( ) {
        ui = GetComponent<UIDocument>( ).rootVisualElement;
        gameContainer = ui.Q<VisualElement>("GameContainer");
        endContainer = ui.Q<VisualElement>("EndContainer");

        // Get a list of all the cards in the UI
        cards = ui.Query<Button>("Card").ToList( );
        flippedCards = new List<Button>( );

        ui.Q<Button>("MainMenuButton").clicked += ( ) => { SceneManager.LoadScene(0); };

        // Generate the list of matches
        numbers = new List<int>( );
        for (int i = 1; i <= cards.Count / 2; i++) {
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
        }

        HasWon = false;
    }

    /// <summary>
    /// Unflip a card that has already been flipped
    /// </summary>
    /// <param name="card">The card to unflip</param>
    public void UnflipCard(Button card) {
        // Get a reference to the card index
        int cardIndex = cards.IndexOf(card);

        // Get a reference to the card label
        Label cardLabel = card.Q<Label>( );

        // Update the card label appearance
        cardLabel.style.visibility = Visibility.Hidden;

        // Since this card was flipped over, increment the cards flipped
        flippedCards.Remove(card);
    }

    /// <summary>
    /// Flip a card over to reveal its number
    /// </summary>
    /// <param name="e">The card button click even information</param>
    public void FlipCard(EventBase e) {
        // If the game is currently checking for a match, then do not flip a card
        // Need to wait until after the match is checked to flip a new card
        if (checkMatchCoroutine != null) {
            return;
        }

        // Get a reference to this card
        Button card = (Button) e.target;

        // If this card is currently flipped, then return and do not try to flip it again
        if (flippedCards.Contains(card)) {
            return;
        }

        // Get a reference to the card index
        int cardIndex = cards.IndexOf(card);

        // Get a reference to the card label
        Label cardLabel = card.Q<Label>( );

        // Update the card label appearance
        cardLabel.text = $"{numbers[cardIndex]}";
        cardLabel.style.visibility = Visibility.Visible;

        // Since this card was flipped over, increment the cards flipped
        flippedCards.Add(card);

        // If there was an even number of cards flipped over, make sure all cards have a match
        if (flippedCards.Count > 0 && flippedCards.Count % 2 == 0) {
            checkMatchCoroutine = StartCoroutine(CheckMatch( ));
        }
    }

    /// <summary>
    /// Check for a match after 2 cards have been turned over (with a slight delay)
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckMatch( ) {
        // Get the index of the last two cards
        Button card1 = flippedCards[flippedCards.Count - 1];
        Button card2 = flippedCards[flippedCards.Count - 2];

        // If the numbers of the last two cards do not match, then flip them back over
        if (numbers[cards.IndexOf(card1)] != numbers[cards.IndexOf(card2)]) {
            // Wait before checking the card match so the player can see what they turned over
            yield return new WaitForSeconds(0.5f);

            UnflipCard(card1);
            UnflipCard(card2);
        }

        // If all of the cards have been flipped over, then the player has won
        if (flippedCards.Count == cards.Count) {
            // Wait before checking the card match so the player can see what they turned over
            yield return new WaitForSeconds(1f);

            HasWon = true;
        }

        checkMatchCoroutine = null;
    }

    /// https://discussions.unity.com/t/clever-way-to-shuffle-a-list-t-in-one-line-of-c-code/535113
    /// <summary>
	/// Shuffles the element order of the specified list.
	/// </summary>
	private void Shuffle(IList ts) {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i) {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}
