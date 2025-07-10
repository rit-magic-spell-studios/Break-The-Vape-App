using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

public class NotSoTastyController : GameController {
    [Header("NotSoTastyController")]
    [SerializeField] private Vector2Int boardSize;
    [SerializeField, Range(0f, 2f)] private float fruitFallSpeed;
    [SerializeField, Range(1, 3)] private int minFruitChainLength;
    [SerializeField, Range(10f, 200f)] private float minFruitHoverDistance;
    [SerializeField, Range(1, 4)] private int minSecretTileSize;
    [SerializeField, Range(1, 4)] private int maxSecretTileSize;
    [SerializeField] private List<Sprite> fruitSprites;
    [SerializeField] private List<Sprite> secretTileSprites;

    private List<VisualElement> tiles;
    private List<VisualElement> fruits;
    private Label remainingSecretTilesLabel;
    private float calculatedTileSize;

    private List<Rect> secretTiles;
    private List<int> secretTileClearCount;

    private List<VisualElement> lineElements;
    private List<VisualElement> chainedFruits;
    private VisualElement lastDragFruit;
    private Vector2 lastMousePosition;
    private bool isChainingFruits;
    private int lineElementIndex;

    public bool CanMatchFruit {
        get {
            // If the current state is not the game, then the player cannot match fruit
            if (UIControllerState != UIState.GAME) {
                return false;
            }

            // Check to see if there are still fruit animating before allowing the player to match more fruit
            for (int i = 0; i < fruits.Count; i++) {
                if (fruits[i].resolvedStyle.translate.y > 0f) {
                    return false;
                }
            }

            // If all fruit are currently on their current tiles and the controller state is the game, then the player can now match fruits again
            return true;
        }
    }

    protected override void Awake( ) {
        base.Awake( );

        // Get all of the tiles and fruit that are on the board
        tiles = ui.Query<VisualElement>("Tile").ToList( );
        fruits = ui.Query<VisualElement>("Fruit").ToList( );
        remainingSecretTilesLabel = ui.Q<Label>("SecretLabel");

        // Add clicked events for the game to function
        gameScreen.RegisterCallback<MouseDownEvent>((e) => {
            // If for some reason the player is already chaining fruits, do not start the chain again
            if (isChainingFruits) {
                return;
            }

            // Get a reference to the fruit that was closest to the mouse position
            VisualElement fruit = GetClosestFruit(e.mousePosition);
            lastMousePosition = e.localMousePosition;

            // If there was no fruit close to the mouse position, then return and do not start a chain
            if (fruit == null) {
                return;
            }

            // Since a fruit was pressed down, the player is starting to chain fruits
            isChainingFruits = true;

            // Add this fruit to the chained fruits
            AddToFruitChain(fruit);
            lastDragFruit = fruit;
        });

        gameScreen.RegisterCallback<MouseMoveEvent>((e) => {
            // If fruits are not being chained, then return and do nothing
            if (!isChainingFruits) {
                return;
            }

            // Get a reference to the fruit that was closest to the mouse position
            VisualElement fruit = GetClosestFruit(e.mousePosition);
            lastMousePosition = e.localMousePosition;

            // Make sure the same fruit does not get dragged over again before the mouse leaves
            // Also make sure there is a closest fruit to add to the chain
            if (fruit == null || fruit == lastDragFruit) {
                return;
            }

            // Check to make sure the fruit is next to the previous fruit
            // If they are adjacent to each other, add the fruit to the chained fruits
            Vector2Int fruitPosition = Get2DIndex(fruits, fruit);
            Vector2Int lastFruitPosition = Get2DIndex(fruits, chainedFruits[^1]);

            if (Mathf.Abs(fruitPosition.x - lastFruitPosition.x) <= 1 && Mathf.Abs(fruitPosition.y - lastFruitPosition.y) <= 1) {
                AddToFruitChain(fruit);
                lastDragFruit = fruit;

                // Chained fruits can be removed when adding if the added fruit is last in the chain
                // In this case, stop chaining fruits
                if (chainedFruits.Count == 0) {
                    isChainingFruits = false;
                }
            }
        });

        gameScreen.RegisterCallback<MouseUpEvent>((e) => {
            // If the player was not already chaining fruits, then return and do nothing
            if (!isChainingFruits) {
                return;
            }

            // Since the mouse was lifted up, the player is no longer chaining fruits
            isChainingFruits = false;

            // If the chain was long enough, destroy the fruit that were in the chain
            if (chainedFruits.Count >= minFruitChainLength) {
                // Add points for each of the chained fruits that were connected
                GamePoints += chainedFruits.Count * 5;

                // Set the display of all the fruits to be invisible
                for (int i = 0; i < chainedFruits.Count; i++) {
                    // Get the position of the fruit on the board
                    Vector2Int fruitPosition = Get2DIndex(fruits, chainedFruits[i]);
                    VisualElement fruitTile = tiles[Get1DIndex(fruitPosition.x, fruitPosition.y)];

                    // If a secret tile contains the fruits position, then display that section of the secret tile
                    if (fruitTile.resolvedStyle.backgroundImage == null) {
                        for (int j = 0; j < secretTiles.Count; j++) {
                            // Translate the fruit position so it uses the secret tile rect coordinates
                            // Right now the fruit (0, 0) position is at the bottom right corner of the screen due to how the lists are set up
                            // The secret tiles have their (0, 0) at the top left position of the board
                            Vector2Int fruitTilePosition = boardSize - Vector2Int.one - fruitPosition;
                            if (secretTiles[j].Contains(fruitTilePosition)) {
                                // Get secret tile variables
                                float secretTileSize = secretTiles[j].width;
                                Vector2 secretTilePosition = secretTiles[j].position;

                                // Get the position and size of the new texture
                                int textureWidth = Mathf.FloorToInt(secretTileSprites[0].texture.width / secretTileSize);
                                int textureHeight = Mathf.FloorToInt(secretTileSprites[0].texture.height / secretTileSize);
                                int textureX = (fruitTilePosition.x - (int) secretTilePosition.x) * textureWidth;
                                // Texture's have (0, 0) in the bottom left corner (so confusing)
                                int textureY = ((int) (secretTilePosition.y + secretTileSize - 1) - fruitTilePosition.y) * textureHeight;

                                // Create the new texture that is a chunk of the secret tile image based on the fruit's position
                                Texture2D secretTileTexture = new Texture2D(textureWidth, textureHeight);
                                secretTileTexture.SetPixels(secretTileSprites[0].texture.GetPixels(textureX, textureY, textureWidth, textureHeight));
                                secretTileTexture.Apply( );

                                // Set the style of the tile background
                                fruitTile.style.backgroundImage = new StyleBackground(secretTileTexture);

                                // Since this is a new tile that has been set, check to see if all of the tiles with this secret tile have been cleared
                                if (++secretTileClearCount[j] >= secretTileSize * secretTileSize) {
                                    // Remove the secret tile from the list
                                    secretTiles.RemoveAt(j);
                                    secretTileClearCount.RemoveAt(j);

                                    // Decrease the amount of secret tiles remaining
                                    remainingSecretTilesLabel.text = $"{secretTiles.Count} secret tiles left!";

                                    // Give the player more points for getting an entire secret tile
                                    GamePoints += 50;
                                }
                                break;
                            }
                        }
                    }

                    // Set the fruit to be invisible so the board knows to have the fruit above this one fall down
                    chainedFruits[i].style.display = DisplayStyle.None;
                }

                // Update the board and have the fruits fall to fill in the gaps
                UpdateBoard( );

                if (secretTiles.Count == 0) {
                    UIControllerState = UIState.WIN;
                }
            }
            chainedFruits.Clear( );

            // Destroy all of the line elements that were created for the fruit chain
            lineElementIndex = -1;
            lastDragFruit = null;
        });

        // Set each fruit to a random fruit image at the start of the game
        // The tile and fruit array should be the same in length, each tile has a fruit attached to it
        for (int i = 0; i < fruits.Count; i++) {
            fruits[i].style.backgroundImage = new StyleBackground(fruitSprites[Random.Range(0, fruitSprites.Count)]);
        }

        // Generate all of the secret tiles on the board
        GenerateSecretTiles( );
        remainingSecretTilesLabel.text = $"{secretTiles.Count} secret tiles left!";

        lineElements = new List<VisualElement>( );
        chainedFruits = new List<VisualElement>( );
        isChainingFruits = false;
        lineElementIndex = -1;
        lastDragFruit = null;

        gameData.Name = "Not So Tasty";
    }

    protected override void Start( ) {
        base.Start( );

        // When the game starts, the tutorial should be shown first
        UIControllerState = UIState.GAME;
    }

    protected override void Update( ) {
        base.Update( );

        // Get the world bounds of the screens that the game takes place in
        // This will be used for repositioning and scaling in the calculations below
        Rect subscreenRect = gameSubscreen.worldBound;

        // If the line element index is equal to the size of the line elements list, then a new line element needs to be added
        // If the line element index is less than the last index in the line elements list, then the last line element needs to be removed
        // If the line element index is equal to the last index, then update its position in real time with the mouse position
        if (lineElementIndex >= lineElements.Count) {
            CreateNewLineElement( );

            // If this is the second line element to be created, then update the previous one
            if (lineElementIndex > 0) {
                // Get the position of the last two fruits to be put into the chain
                Vector2 chainedFruitPosition1 = chainedFruits[^1].worldBound.center - subscreenRect.position;
                Vector2 chainedFruitPosition2 = chainedFruits[^2].worldBound.center - subscreenRect.position;

                SetLineElementStyles(lineElements[lineElementIndex - 1], chainedFruitPosition1, chainedFruitPosition2);
            }
        } else if (lineElementIndex < lineElements.Count - 1) {
            gameSubscreen.Remove(lineElements[^1]);
            lineElements.Remove(lineElements[^1]);
        } else if (lineElementIndex == lineElements.Count - 1 && lineElementIndex != -1) {
            // Get the position of the last fruit in the chain
            Vector2 chainedFruitPosition = chainedFruits[^1].worldBound.center - subscreenRect.position;

            // Set the last line element's size, position, and rotation
            SetLineElementStyles(lineElements[lineElementIndex], chainedFruitPosition, lastMousePosition - subscreenRect.position);
        }
    }

    protected override void OnScreenChange( ) {
        base.OnScreenChange( );

        // Get the width of the tiles in pixels after the game screen has been updated
        if (UIControllerState == UIState.GAME) {
            calculatedTileSize = ui.Q<VisualElement>("Tile").resolvedStyle.width;
        }
    }

    /// <summary>
    /// Generate all of the secret tiles that will be on the board
    /// </summary>
    private void GenerateSecretTiles( ) {
        // The total area that the secret tiles take up should not exceed 1/2 the size of the game board
        int totalAreaCount = Mathf.FloorToInt(boardSize.x * boardSize.y * 0.5f);

        // Generate all secret tiles on the board
        secretTiles = new List<Rect>( );
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
            for (int x = 0; x < boardSize.x - (size - 1); x++) {
                for (int y = 0; y < boardSize.y - (size - 1); y++) {
                    // Check to see if the current position interferes with already placed secret tiles
                    bool hasOverlappingTile = false;
                    for (int k = 0; k < secretTiles.Count; k++) {
                        Rect tempRect = new Rect(x, y, size, size);

                        // If the temp rect (where the new secret tile would be placed) overlaps with another tile, then the position is not valid
                        if (tempRect.Overlaps(secretTiles[k])) {
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
            secretTiles.Add(new Rect(position.x, position.y, size, size));
            secretTileClearCount.Add(0);
            totalAreaCount -= size * size;
        }

        // Log a warning to the console if the iteration count exceeded 20 and broke out of the loop
        if (iterationCount >= 20) {
            Debug.LogWarning("Secret tile generation loop exceeded 20 iterations");
        }
    }

    /// <summary>
    /// Get the closest fruit to the input position
    /// </summary>
    /// <param name="position">The position on the UI screen</param>
    /// <returns>A reference to the closest fruit if it is closer than the minimum fruit hover distance, false otherwise</returns>
    private VisualElement GetClosestFruit(Vector2 position) {
        float minDistance = Mathf.Infinity;
        VisualElement closestFruit = null;

        // Get the minimum distance to a fruit from the input position
        for (int i = 0; i < fruits.Count; i++) {
            // Get the distance to the current fruit
            float distance = Vector2.Distance(fruits[i].worldBound.center, position);

            // Check to see if a new minimum distance was found
            if (distance < minDistance) {
                minDistance = distance;
                closestFruit = fruits[i];
            }
        }

        // If the minimum distance is not lower than the minimum fruit hover distance, then the position is too far away from any fruit
        return (minDistance <= minFruitHoverDistance ? closestFruit : null);
    }

    /// <summary>
    /// Add a fruit to the current fruit chain
    /// </summary>
    /// <param name="fruit">The fruit to add</param>
    /// <returns>Whether or not adding the fruit was successful</returns>
    private bool AddToFruitChain(VisualElement fruit) {
        // If the chain already contains the fruit do not add it
        if (chainedFruits.Contains(fruit)) {
            // If the last fruit in the chain is hovered over, remove it from the list
            // This will allow the player to backtrack and redo their chain
            if (chainedFruits.Count > 1 && chainedFruits[^2] == fruit) {
                // Also remove the line element that was connected to that fruit
                lineElementIndex--;
                chainedFruits.Remove(chainedFruits[^1]);
            }

            return false;
        }

        // If there is more than one fruit in the chained fruit
        if (chainedFruits.Count > 0 && chainedFruits[^1].resolvedStyle.backgroundImage.sprite != fruit.resolvedStyle.backgroundImage.sprite) {
            return false;
        }

        lineElementIndex++;
        chainedFruits.Add(fruit);

        return true;
    }

    /// <summary>
    /// Set the size, position, and rotation of the last line element
    /// </summary>
    /// <param name="lineElement">The line element to update</param>
    /// <param name="point1">The starting point for the line to connect to</param>
    /// <param name="point2">The ending point for the line to connect to</param>
    private void SetLineElementStyles(VisualElement lineElement, Vector2 point1, Vector2 point2) {
        Vector2 midpoint = (point1 + point2) / 2f;
        float distance = (point2 - point1).magnitude + 20f;
        float angle = -Vector2.SignedAngle(point2 - point1, Vector2.up);

        // Set the styles of the line element
        lineElement.style.left = midpoint.x;
        lineElement.style.top = midpoint.y - (distance / 2f);
        lineElement.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
        lineElement.style.height = distance;
    }

    /// <summary>
    /// Create a new line element that will show the connection between two fruits
    /// </summary>
    private void CreateNewLineElement( ) {
        // Create a new line element
        VisualElement lineElement = new VisualElement {
            style = {
                position = Position.Absolute,
                left = 0f,
                top = 0f,
                width = 20,
                minWidth = 20,
                height = 20,
                minHeight = 20,
                backgroundColor = new StyleColor(Color.black),
                borderBottomLeftRadius = 10f,
                borderBottomRightRadius = 10f,
                borderTopLeftRadius = 10f,
                borderTopRightRadius = 10f
            }
        };

        lineElements.Add(lineElement);
        gameSubscreen.Add(lineElement);
    }

    /// <summary>
    /// Convert a 2D position into an index for a 1D list
    /// </summary>
    /// <param name="x">The x position</param>
    /// <param name="y">The y position</param>
    /// <returns>The 1D index that corresponds to the 2D position in the list</returns>
    private int Get1DIndex(int x, int y) {
        // (0, 0) is the bottom right corner of the list
        return (boardSize.x - x - 1) + ((boardSize.y - y - 1) * boardSize.x);
    }

    /// <summary>
    /// Get a 2D index from the index of an element within a list
    /// </summary>
    /// <param name="list">The list that the element is in</param>
    /// <param name="element">The element to get the 2D position of</param>
    /// <returns>A 2D position of the element based on its 1D list index</returns>
    private Vector2Int Get2DIndex(IList list, VisualElement element) {
        // (0, 0) is the bottom right corner of the list
        int index = list.Count - list.IndexOf(element) - 1;
        return new Vector2Int(index % boardSize.x, index / boardSize.x);
    }

    /// <summary>
    /// Update all of the tiles and fruits on the board
    /// </summary>
    private void UpdateBoard( ) {
        Queue<VisualElement> missingFruits = new Queue<VisualElement>( );

        for (int x = 0; x < boardSize.x; x++) {
            for (int y = 0; y < boardSize.y; y++) {
                VisualElement fruit = fruits[Get1DIndex(x, y)];

                // If the fruit is already enabled, then skip it
                // This only works because we are going from the bottom to the top of the board
                if (fruit.resolvedStyle.display == DisplayStyle.Flex) {
                    if (missingFruits.Count > 0) {
                        // Get the earliest missing fruit in the queue
                        StartFallingFruitAnimation(missingFruits.Dequeue( ), fruit.resolvedStyle.backgroundImage.sprite, y);

                        // Since this fruit has been transferred to another tile, this tile is now missing
                        missingFruits.Enqueue(fruit);
                    }

                    continue;
                }

                // If this fruit is missing, then enqueue it into the missing fruits list until there is a fruit that can fill the emptiness
                missingFruits.Enqueue(fruit);
            }

            // If there are still fruits left in the missing queue, then that means new fruits need to be generated above the board
            int i = 0;
            while (missingFruits.Count > 0) {
                StartFallingFruitAnimation(missingFruits.Dequeue( ), fruitSprites[Random.Range(0, fruitSprites.Count)], boardSize.y + i);

                i++;
            }
        }
    }

    /// <summary>
    /// Start a fruit animation of it falling
    /// </summary>
    /// <param name="animatingFruit">The fruit element to animate</param>
    /// <param name="fruitSprite">The sprite that the animating fruit should be</param>
    /// <param name="tileFallHeight">The height from which the fruit should fall</param>
    private void StartFallingFruitAnimation(VisualElement animatingFruit, Sprite fruitSprite, int tileFallHeight) {
        // Get the difference in heights of the earliest empty fruit and this current fruit
        int heightDifference = tileFallHeight - Get2DIndex(fruits, animatingFruit).y;

        // Set the translate position of that fruit to the position of this fruit
        // This will create the illusion that the fruit is falling between the tiles, when in reality it is always the same fruit object on each tile
        animatingFruit.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(heightDifference * -calculatedTileSize)));
        animatingFruit.style.backgroundImage = new StyleBackground(fruitSprite);

        // Start the fruit animation
        StartCoroutine(FallingFruitAnimation(animatingFruit));
    }

    /// <summary>
    /// Animate a fruit falling into place on its tile
    /// </summary>
    /// <param name="fruit">The fruit to animate</param>
    /// <returns></returns>
    private IEnumerator FallingFruitAnimation(VisualElement fruit) {
        // Get the starting height of the fruit
        float fromHeight = fruit.resolvedStyle.translate.y;
        int tileFallCount = Mathf.RoundToInt(fromHeight / -calculatedTileSize);

        // Make sure the fruit is visible as it animates
        fruit.style.display = DisplayStyle.Flex;

        float t = 0f;
        while (t < 1) {
            // Based on the tile fall count, lerp the fruit faster or slower so it falls a consistent speed
            t += Time.deltaTime / (fruitFallSpeed * tileFallCount);

            // Smoothly move the translation of the fruit to 0 from its height
            fruit.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(Mathf.Lerp(fromHeight, 0f, t))));

            yield return null;
        }

        // Set the final translation of the fruit
        fruit.style.translate = new StyleTranslate(new Translate(new Length(0), new Length(0)));
    }

    private void SetTileBackground( ) {

    }
}
