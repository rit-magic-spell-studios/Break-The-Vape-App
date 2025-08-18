using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Unity.VisualScripting;
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
    [SerializeField, Min(0)] private int minFruitChainLength;
    [SerializeField, Range(1, 4)] private int minSecretTileSize;
    [SerializeField, Range(1, 4)] private int maxSecretTileSize;
    [SerializeField] private List<Sprite> secretTileSprites;

    private Label remainingSecretTilesLabel;

    public Tile[ , ] Tiles { get; private set; }

    public List<Rect> SecretTiles { get; private set; }
    private List<int> secretTileClearCount;
    private List<Sprite> currentSecretTileSprites;

    public List<Tile> ChainedTiles { get; private set; }
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

                Tile tile = Instantiate(tilePrefab, new Vector2(tilePositionX, -tilePositionY), Quaternion.identity).GetComponent<Tile>( );
                tile.transform.SetParent(objectContainer, false);
                tile.BoardPosition = new Vector2Int(x, y);
                tile.transform.localScale = new Vector2(tileSize, tileSize);
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
        currentSecretTileSprites = new List<Sprite>( );
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
            currentSecretTileSprites.Add(secretTileSprites[Random.Range(0, secretTileSprites.Count)]);
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
                chainLineRenderer.positionCount = ChainedTiles.Count;
                chainLineRenderer.SetPositions(ChainedTiles.Select(tile => tile.transform.position).ToArray( ));
            }

            return false;
        }

        if (ChainedTiles.Count > 0 && ChainedTiles[^1].FruitSprite != tile.FruitSprite) {
            return false;
        }

        ChainedTiles.Add(tile);
        chainLineRenderer.positionCount = ChainedTiles.Count;
        chainLineRenderer.SetPositions(ChainedTiles.Select(tile => tile.transform.position).ToArray( ));
        return true;
    }

    /// <summary>
    /// Clear the current chain of tiles, score points, and update the board
    /// </summary>
    public void ClearChain( ) {
        if (ChainedTiles.Count >= minFruitChainLength) {
            AddPoints(ChainedTiles[^1].transform.position, ChainedTiles.Count * 5);
            UncoverSecretTiles( );
            UpdateBoard( );
        }

        ChainedTiles.Clear( );
        chainLineRenderer.positionCount = 0;
        chainLineRenderer.SetPositions(new Vector3[0]);
    }

    /// <summary>
    /// Uncover secret tiles on the board based on where the chained tiles were matched
    /// </summary>
    public void UncoverSecretTiles( ) {
        for (int i = 0; i < ChainedTiles.Count; i++) {
            ChainedTiles[i].NeedsUpdating = true;

            if (ChainedTiles[i].IsUncovered) {
                continue;
            }
            ChainedTiles[i].IsUncovered = true;

            for (int j = 0; j < SecretTiles.Count; j++) {
                if (!SecretTiles[j].Contains(ChainedTiles[i].BoardPosition)) {
                    continue;
                }

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
                secretTileTexture.SetPixels(currentSecretTileSprites[j].texture.GetPixels(textureX, textureY, textureWidth, textureHeight));
                secretTileTexture.Apply( );

                // Set the style of the tile background
                ChainedTiles[i].TileSprite = Sprite.Create(secretTileTexture, new Rect(0, 0, secretTileTexture.width, secretTileTexture.height), new Vector2(0.5f, 0.5f), secretTileTexture.width);

                // Remove the secret tile if it has been fully uncovered
                if (++secretTileClearCount[j] >= secretTileSize * secretTileSize) {
                    SecretTiles.RemoveAt(j);
                    secretTileClearCount.RemoveAt(j);
                    currentSecretTileSprites.RemoveAt(j);

                    remainingSecretTilesLabel.text = $"{SecretTiles.Count} secret tiles left!";
                    AddPoints(ChainedTiles[i].transform.position, 50);
                }

                break;
            }
        }

        if (SecretTiles.Count == 0) {
            WinGame( );
        }
    }

    /// <summary>
    /// Update all of the tiles and fruits on the board
    /// </summary>
    public void UpdateBoard( ) {
        Queue<Tile> needsUpdatingTiles = new Queue<Tile>( );

        for (int x = 0; x < tileGridWidth; x++) {
            for (int y = tileGridHeight - 1; y >= 0; y--) {
                // If the fruit is already enabled, then skip it
                // This only works because we are going from the bottom to the top of the board
                if (!Tiles[x, y].NeedsUpdating) {
                    if (needsUpdatingTiles.Count > 0) {
                        // Get the earliest missing fruit in the queue
                        needsUpdatingTiles.Dequeue( ).AnimateFruit(y, Tiles[x, y].FruitSprite);

                        // Since this fruit has been transferred to another tile, this tile is now missing
                        needsUpdatingTiles.Enqueue(Tiles[x, y]);
                    }

                    continue;
                }

                // If this fruit is missing, then enqueue it into the missing fruits list until there is a fruit that can fill the emptiness
                needsUpdatingTiles.Enqueue(Tiles[x, y]);
                Tiles[x, y].NeedsUpdating = false;
            }

            // If there are still fruits left in the missing queue, then that means new fruits need to be generated above the board
            int i = 0;
            while (needsUpdatingTiles.Count > 0) {
                needsUpdatingTiles.Dequeue( ).AnimateFruit(-i - 4);
                i++;
            }
        }
    }
}
