using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MatchAndCatchController : MonoBehaviour {
    private VisualElement ui;

    private List<Button> cards;
    private List<int> numbers;

    public bool HasWon {
        get => _hasWon;
        set {
            _hasWon = value;


        }
    }
    private bool _hasWon;

    private void Awake( ) {
        ui = GetComponent<UIDocument>( ).rootVisualElement;

        // Get a list of all the cards in the UI
        cards = ui.Query<Button>("Card").ToList( );

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
            cards[i].clickable.clickedWithEventInfo += (EventBase e) => {
                Label cardLabel = ((VisualElement) e.target).Q<Label>( );
                Debug.Log( cardLabel );
                int cardIndex = cards.IndexOf((Button) e.target);
                Debug.Log(cardIndex );
                cardLabel.text = $"{numbers[cardIndex]}";
                cardLabel.style.visibility = Visibility.Visible;
                Debug.Log(cardLabel);
            };
        }

        HasWon = false;
    }

    /// <summary>
    /// Check to see if the player has won the game
    /// </summary>
    private void CheckForWin( ) {
        // If there are any cards that are still visible, then the player has not won yet
        foreach (Button card in cards) {
            if (card.style.visibility == Visibility.Visible) {
                return;
            }
        }

        // If no cards are visible and the code makes it to this point, then the player has won
        HasWon = true;
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
