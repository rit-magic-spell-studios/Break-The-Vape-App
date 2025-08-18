using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MatchAndCatchController : GameController {
    [Header("MatchAndCatchController")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField, Range(0f, 2f)] private float cardCheckDelay;
    [SerializeField, Range(0, 5)] private int cardGridWidth;
    [SerializeField, Range(0, 5)] private int cardGridHeight;
    [SerializeField, Range(0f, 3f)] private float cardSpacing;
    [SerializeField] private List<Sprite> cardFrontSprites;

    private List<Card> cards;
    private List<int> cardMatchIndices;

    private Label matchesLabel;
    private Coroutine checkMatchDelay;

    public List<Card> FlippedCards { get; private set; }
    public bool CanFlipCard => checkMatchDelay == null;

    protected override void Awake( ) {
        base.Awake( );

        cardMatchIndices = GenerateRandomMatchList( );
        cards = GenerateCardGrid(cardGridWidth, cardGridHeight);
        FlippedCards = new List<Card>( );

        // Get references to other important UI elements
        matchesLabel = ui.Q<Label>("MatchesLabel");
        matchesLabel.text = $"{(cards.Count - FlippedCards.Count) / 2} matches left!";
    }

    /// <summary>
    /// Generate a random list of indices that map to different sprites within the card sprites list
    /// </summary>
    /// <returns>An integer list of random indices</returns>
    private List<int> GenerateRandomMatchList( ) {
        List<int> matchIndexList = new List<int>( );

        List<int> availableIndices = new List<int>( );
        for (int i = 0; i < cardFrontSprites.Count; i++) {
            availableIndices.Add(i);
        }

        for (int i = 0; i < cardGridWidth * cardGridHeight / 2; i++) {
            int randomAvailableIndex = Random.Range(0, availableIndices.Count);
            int randomSpriteIndex = availableIndices[randomAvailableIndex];

            matchIndexList.Add(randomSpriteIndex);
            matchIndexList.Add(randomSpriteIndex);

            availableIndices.RemoveAt(randomAvailableIndex);
        }

        Utils.Shuffle(matchIndexList);
        return matchIndexList;
    }

    /// <summary>
    /// Generate a grid of card objects that the player will click on to flip over
    /// </summary>
    /// <param name="gridWidth">The grid width in cards</param>
    /// <param name="gridHeight">The grid height in cards</param>
    /// <returns>A list of all the card objects generated and placed into the grid</returns>
    private List<Card> GenerateCardGrid(int gridWidth, int gridHeight) {
        List<Card> cards = new List<Card>( );

        float cardSize = ((cameraHalfWidth * 2f) - ((gridWidth + 1) * cardSpacing)) / gridWidth;
        float gridWorldHeight = (gridHeight * (cardSize + cardSpacing)) - cardSpacing;
        float gridWorldWidth = (gridWidth * (cardSize + cardSpacing)) - cardSpacing;

        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                float cardPositionX = (x * (cardSize + cardSpacing)) - (gridWorldWidth / 2f) + (cardSize / 2f);
                float cardPositionY = (y * (cardSize + cardSpacing)) - (gridWorldHeight / 2f) + (cardSize / 2f);

                Card card = Instantiate(cardPrefab, new Vector2(cardPositionX, cardPositionY), Quaternion.identity, objectContainer).GetComponent<Card>( );
                card.CardFront = cardFrontSprites[cardMatchIndices[x + (gridWidth * y)]];
                card.Size = cardSize;

                cards.Add(card);
            }
        }

        return cards;
    }

    /// <summary>
    /// Check to see if a match has been found
    /// </summary>
    public void CheckForMatch( ) {
        // Only start checking for a match if the right number of cards have been flipped over
        if (FlippedCards.Count > 0 && FlippedCards.Count % 2 == 0) {
            checkMatchDelay = StartCoroutine(CheckMatches( ));
        }
    }

    /// <summary>
    /// Check for a match after 2 cards have been turned over (with a slight delay)
    /// </summary>
    private IEnumerator CheckMatches( ) {
        Card card1 = FlippedCards[^1];
        Card card2 = FlippedCards[^2];

        // If the sprites of the last two cards do not match, then flip them back over
        // If the sprites of the last two cards do match, then keep them flipped over
        if (card1.CardFront != card2.CardFront) {
            yield return new WaitForSeconds(cardCheckDelay);

            card1.FlipCard( );
            FlippedCards.Remove(card1);

            card2.FlipCard( );
            FlippedCards.Remove(card2);
        } else {
            matchesLabel.text = $"{(cards.Count - FlippedCards.Count) / 2} matches left!";
            AddPoints(Vector3.zero, 100);

            if (FlippedCards.Count == cards.Count) {
                WinGame( );
            }
        }

        checkMatchDelay = null;
    }
}
