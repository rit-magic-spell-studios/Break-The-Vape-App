using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class NotSoTastyController : GameController {
    [Header("NotSoTastyController")]
    [SerializeField] private Vector2Int boardSize;
    [SerializeField, Range(0f, 2f)] private float fruitFallSpeed;
    [SerializeField] private List<Sprite> fruitSprites;

    private List<VisualElement> tiles;
    private List<VisualElement> fruits;
    private float calculatedTileSize;

    private VisualElement lineElement;

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

        // Add clicked events for all of the tiles on the board
        for (int i = 0; i < tiles.Count; i++) {
            tiles[i].RegisterCallback<MouseDownEvent>((e) => {
                // If the player can match fruit, then delete the fruit and update the board
                // This is just to test the board updating
                if (CanMatchFruit) {
                    fruits[tiles.IndexOf((VisualElement) e.currentTarget)].style.display = DisplayStyle.None;
                    UpdateBoard( );
                }
            });

            tiles[i].RegisterCallback<MouseOverEvent>((e) => {
            });

            tiles[i].RegisterCallback<MouseUpEvent>((e) => {
            });

            // Set each fruit to a random fruit image at the start of the game
            // The tile and fruit array should be the same in length, each tile has a fruit attached to it
            fruits[i].style.backgroundImage = new StyleBackground(fruitSprites[Random.Range(0, fruitSprites.Count)]);
        }

        lineElement = new VisualElement {
            style = {
                position = Position.Absolute,
                left = 0f,
                top = 0f,
                width = 20,
                height = 20,
                backgroundColor = new StyleColor(Color.black)
            }
        };
        gameSubscreen.Add(lineElement);
    }

    protected override void Start( ) {
        base.Start( );

        // When the game starts, the tutorial should be shown first
        UIControllerState = UIState.GAME;
    }

    protected override void Update( ) {
        base.Update( );

        Vector2 mousePosition = Mouse.current.position.ReadValue( );
        Rect subscreenRect = gameSubscreen.worldBound;
        Rect screenRect = gameScreen.worldBound;
        Debug.Log(mousePosition + " | " + gameSubscreen.worldBound + " | " + gameScreen.worldBound);
        lineElement.style.left = mousePosition.x * (screenRect.width / Screen.width) - subscreenRect.x;
        lineElement.style.top = -mousePosition.y * (screenRect.height / Screen.height) + subscreenRect.height;
        Debug.Log(lineElement.resolvedStyle.top);
        //lineElement.style.translate = new StyleTranslate(new Translate(new Length(scaledMousePosition.x), new Length(-scaledMousePosition.y)));
    }

    protected override void OnScreenChange( ) {
        base.OnScreenChange( );

        // Get the width of the tiles in pixels after the game screen has been updated
        if (UIControllerState == UIState.GAME) {
            calculatedTileSize = tiles[0].resolvedStyle.width;
        }
    }

    /// <summary>
    /// Convert a 2D position into an index for a 1D list
    /// </summary>
    /// <param name="x">The x position</param>
    /// <param name="y">The y position</param>
    /// <returns>The 1D index that corresponds to the 2D position in the list</returns>
    private int Get1DIndex(int x, int y) {
        // (0, 0) is the bottom right corner of the list
        return x + ((boardSize.y - y - 1) * boardSize.x);
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
}
