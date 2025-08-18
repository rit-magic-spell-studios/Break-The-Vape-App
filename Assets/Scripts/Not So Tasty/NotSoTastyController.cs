using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

public class NotSoTastyController : GameController {
    [Header("NotSoTastyController")]
    [SerializeField] private LineRenderer chainLineRenderer;
    [SerializeField] private GameObject tilePrefab;
    [Space]
    [SerializeField, Range(0, 15)] private int tileGridWidth;
    [SerializeField, Range(0, 15)] private int tileGridHeight;
    [SerializeField, Range(0f, 3f)] private float tileEdgeSpacing;
    [SerializeField, Min(0)] public int MinFruitChainLength;
    [SerializeField, Range(1, 4)] private int minSecretTileSize;
    [SerializeField, Range(1, 4)] private int maxSecretTileSize;
    [SerializeField] private List<Sprite> secretTileSprites;

    private Label remainingSecretTilesLabel;

    public Tile[ , ] Tiles { get; private set; }

    public List<Rect> SecretTiles { get; private set; }
    private List<int> secretTileClearCount;

    public List<Tile> ChainedTiles { get; private set; }
    public Tile LastChainedTile {
        get {
            if (ChainedTiles.Count > 0) {
                return ChainedTiles[^1];
            }

            return null;
        }
    }
    public bool IsChainingFruits => ChainedTiles.Count > 0;

    public bool CanMatchFruit {
        get {
            if (!IsPlayingGame) {
                return false;
            }

            for (int x = 0; x < tileGridWidth; x++) {
                for (int y = 0; y < tileGridHeight; y++) {
                    if (Tiles[x, y].IsAnimating) {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    protected override void Awake( ) {
        base.Awake( );

        GenerateTileGrid( );
        GenerateSecretTiles( );
        remainingSecretTilesLabel = ui.Q<Label>("SecretLabel");
        remainingSecretTilesLabel.text = $"{SecretTiles.Count} secret tiles left!";

        ChainedTiles = new List<Tile>( );
    }

    /// <summary>
    /// Generate a grid of tiles that the player will interact with
    /// </summary>
    private void GenerateTileGrid( ) {
        Tiles = new Tile[tileGridWidth, tileGridHeight];
        float tileSize = ((cameraHalfWidth * 2f) - (tileEdgeSpacing * 2f)) / tileGridWidth;

        for (int x = 0; x < tileGridWidth; x++) {
            for (int y = 0; y < tileGridHeight; y++) {
                float tilePositionX = (x * tileSize) - (tileGridWidth * tileSize / 2f) + (tileSize / 2f);
                float tilePositionY = (y * tileSize) - (tileGridHeight * tileSize / 2f) + (tileSize / 2f);

                Tile tile = Instantiate(tilePrefab, new Vector2(tilePositionX, tilePositionY), Quaternion.identity).GetComponent<Tile>( );
                tile.transform.SetParent(objectContainer, false);
                tile.BoardPosition = new Vector2Int(x, y);
                tile.transform.localScale = new Vector2(tileSize, tileSize);
                Debug.Log(tile.BoardPosition);
                Tiles[x, y] = tile;
            }
        }
    }


    /// <summary>
    /// Generate all of the secret tiles that will be on the board
    /// </summary>
    private void GenerateSecretTiles( ) {
        // The total area that the secret tiles take up should not exceed 1/2 the size of the game board
        int totalAreaCount = Mathf.FloorToInt(tileGridWidth * tileGridHeight * 0.5f);

        // Generate all secret tiles on the board
        SecretTiles = new List<Rect>( );
        secretTileClearCount = new List<int>( );
        List<int> unavailableSizes = new List<int>( );
        List<int> availableSizes = new List<int>( );
        List<Vector2> availablePositions = new List<Vector2>( );

        // The loop that generates the secret tiles onto the board has a chance of infinitely looping depending on starting variables, so put a limit on the number of iterations that can take place
        int iterationCount = 0;
        for (; iterationCount < 20; iterationCount++) {
            availableSizes.Clear( );
            availablePositions.Clear( );

            // Get a list of all the available sizes that the secret tile can be
            for (int j = minSecretTileSize; j <= maxSecretTileSize; j++) {
                // If the area of the secret tile, when added to the board, does not exceed 1/2 of the total game board area, then it can be chosen as a secret tile size
                if (totalAreaCount - (j * j) >= 0 && !unavailableSizes.Contains(j)) {
                    availableSizes.Add(j);
                }
            }

            // If there are no more available sizes, then break from the loop and stop generating tiles
            if (availableSizes.Count == 0) {
                break;
            }

            // Get a random size for the secret tile based on the sizes available
            int size = availableSizes[Random.Range(0, availableSizes.Count)];

            // Get a list of all the available positions a secret tile can spawn at
            // Loop through all of the possible positions on the board that the tile could fit on
            for (int x = 0; x < tileGridWidth - (size - 1); x++) {
                for (int y = 0; y < tileGridHeight - (size - 1); y++) {
                    // Check to see if the current position interferes with already placed secret tiles
                    bool hasOverlappingTile = false;
                    for (int k = 0; k < SecretTiles.Count; k++) {
                        Rect tempRect = new Rect(x, y, size, size);

                        // If the temp rect (where the new secret tile would be placed) overlaps with another tile, then the position is not valid
                        if (tempRect.Overlaps(SecretTiles[k])) {
                            hasOverlappingTile = true;
                            break;
                        }
                    }

                    // After checking all of the currently placed secret tiles, if there are none that overlap the current position, then add the position to the available positions list
                    if (!hasOverlappingTile) {
                        availablePositions.Add(new Vector2(x, y));
                    }
                }
            }

            // If there are no available positions, then add the size to the unavailable size list
            // This prevents that size from being generated again
            if (availablePositions.Count == 0) {
                unavailableSizes.Add(size);
                continue;
            }

            // Get a random position from the list of available positions
            Vector2 position = availablePositions[Random.Range(0, availablePositions.Count)];

            // Now that we have a position and a size, create a new secret tile
            SecretTiles.Add(new Rect(position.x, position.y, size, size));
            secretTileClearCount.Add(0);
            totalAreaCount -= size * size;
        }

        // Log a warning to the console if the iteration count exceeded 20 and broke out of the loop
        if (iterationCount >= 20) {
            Debug.LogWarning("Secret tile generation loop exceeded 20 iterations");
        }
    }

    /// <summary>
    /// Add a tile to the current tile chain
    /// </summary>
    /// <param name="tile">The tile to add</param>
    /// <returns>Whether or not adding the tile was successful</returns>
    public bool AddToTileChain(Tile tile) {
        if (ChainedTiles.Contains(tile)) {
            // If the last tile in the chain is hovered over, remove it from the list
            // This will allow the player to backtrack and redo their chain
            if (ChainedTiles.Count > 1 && ChainedTiles[^2] == tile) {
                ChainedTiles.Remove(ChainedTiles[^1]);
            }

            return false;
        }

        if (ChainedTiles.Count > 0 && ChainedTiles[^1].FruitSprite != tile.FruitSprite) {
            return false;
        }

        ChainedTiles.Add(tile);
        return true;
    }

    public void UncoverSecretTiles( ) {
        for (int i = 0; i < ChainedTiles.Count; i++) {
            if (!ChainedTiles[i].IsUncovered) {
                for (int j = 0; j < SecretTiles.Count; j++) {
                    if (SecretTiles[j].Contains(ChainedTiles[i].BoardPosition)) {
                        // Get secret tile variables
                        float secretTileSize = SecretTiles[j].width;
                        Vector2 secretTilePosition = SecretTiles[j].position;

                        // Get the position and size of the new texture
                        int textureWidth = Mathf.FloorToInt(secretTileSprites[0].texture.width / secretTileSize);
                        int textureHeight = Mathf.FloorToInt(secretTileSprites[0].texture.height / secretTileSize);
                        int textureX = (ChainedTiles[i].BoardPosition.x - (int) secretTilePosition.x) * textureWidth;
                        // Texture's have (0, 0) in the bottom left corner (so confusing)
                        int textureY = ((int) (secretTilePosition.y + secretTileSize - 1) - ChainedTiles[i].BoardPosition.y) * textureHeight;

                        // Create the new texture that is a chunk of the secret tile image based on the fruit's position
                        Texture2D secretTileTexture = new Texture2D(textureWidth, textureHeight);
                        secretTileTexture.SetPixels(secretTileSprites[0].texture.GetPixels(textureX, textureY, textureWidth, textureHeight));
                        secretTileTexture.Apply( );

                        // Set the style of the tile background
                        ChainedTiles[i].TileSprite = Sprite.Create(secretTileTexture, new Rect(0, 0, secretTileTexture.width, secretTileTexture.height), new Vector2(0.5f, 0.5f));

                        // Remove the secret tile if it has been fully uncovered
                        if (++secretTileClearCount[j] >= secretTileSize * secretTileSize) {
                            SecretTiles.RemoveAt(j);
                            secretTileClearCount.RemoveAt(j);

                            remainingSecretTilesLabel.text = $"{SecretTiles.Count} secret tiles left!";
                            AddPoints(ChainedTiles[i].transform.position, 50);
                        }

                        break;
                    }

                    ChainedTiles[i].IsUncovered = true;
                }
            }

            ChainedTiles[i].NeedsUpdating = true;
        }

        if (SecretTiles.Count == 0) {
            WinGame( );
        }
    }

    /// <summary>
    /// Update all of the tiles and fruits on the board
    /// </summary>
    public void UpdateBoard( ) {
        Queue<VisualElement> missingFruits = new Queue<VisualElement>( );

        //for (int x = 0; x < tileGridWidth; x++) {
        //    for (int y = tileGridHeight - 1; y >= 0; y--) {
        //        // If the fruit is already enabled, then skip it
        //        // This only works because we are going from the bottom to the top of the board
        //        if (Tiles[x, y].NeedsUpdating) {
        //            if (missingFruits.Count > 0) {
        //                // Get the earliest missing fruit in the queue
        //                StartFallingFruitAnimation(missingFruits.Dequeue( ), fruit.resolvedStyle.backgroundImage.sprite, y);

        //                // Since this fruit has been transferred to another tile, this tile is now missing
        //                missingFruits.Enqueue(fruit);
        //            }

        //            continue;
        //        }

        //        // If this fruit is missing, then enqueue it into the missing fruits list until there is a fruit that can fill the emptiness
        //        missingFruits.Enqueue(fruit);
        //    }

        //    // If there are still fruits left in the missing queue, then that means new fruits need to be generated above the board
        //    int i = 0;
        //    while (missingFruits.Count > 0) {
        //        StartFallingFruitAnimation(missingFruits.Dequeue( ), fruitSprites[Random.Range(0, fruitSprites.Count)], boardSize.y + i);

        //        i++;
        //    }
        //}
    }

    ///// <summary>
    ///// Start a fruit animation of it falling
    ///// </summary>
    ///// <param name="animatingFruit">The fruit element to animate</param>
    ///// <param name="fruitSprite">The sprite that the animating fruit should be</param>
    ///// <param name="tileFallHeight">The height from which the fruit should fall</param>
    //private void StartFallingFruitAnimation(VisualElement animatingFruit, Sprite fruitSprite, int tileFallHeight) {
    //    // Get the difference in heights of the earliest empty fruit and this current fruit
    //    int heightDifference = tileFallHeight - Get2DIndex(fruits, animatingFruit).y;

    //    // Set the translate position of that fruit to the position of this fruit
    //    // This will create the illusion that the fruit is falling between the tiles, when in reality it is always the same fruit object on each tile
    //    animatingFruit.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(heightDifference * -calculatedTileSize)));
    //    animatingFruit.style.backgroundImage = new StyleBackground(fruitSprite);

    //    // Start the fruit animation
    //    StartCoroutine(FallingFruitAnimation(animatingFruit));
    //}

    ///// <summary>
    ///// Animate a fruit falling into place on its tile
    ///// </summary>
    ///// <param name="fruit">The fruit to animate</param>
    ///// <returns></returns>
    //private IEnumerator FallingFruitAnimation(VisualElement fruit) {
    //    // Get the starting height of the fruit
    //    float fromHeight = fruit.resolvedStyle.translate.y;
    //    int tileFallCount = Mathf.RoundToInt(fromHeight / -calculatedTileSize);

    //    // Make sure the fruit is visible as it animates
    //    fruit.style.display = DisplayStyle.Flex;

    //    float t = 0f;
    //    while (t < 1) {
    //        // Based on the tile fall count, lerp the fruit faster or slower so it falls a consistent speed
    //        t += Time.deltaTime / (fruitFallSpeed * tileFallCount);

    //        // Smoothly move the translation of the fruit to 0 from its height
    //        fruit.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(Mathf.Lerp(fromHeight, 0f, t))));

    //        yield return null;
    //    }

    //    // Set the final translation of the fruit
    //    fruit.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(0)));
    //}

    //private void SetTileBackground( ) {

    //}
}
