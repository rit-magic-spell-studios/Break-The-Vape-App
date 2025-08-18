using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
    [SerializeField] private SpriteRenderer tileSpriteRenderer;
    [SerializeField] private SpriteRenderer fruitSpriteRenderer;
    [SerializeField] private List<Sprite> fruitSprites;
    [SerializeField] private NotSoTastyController notSoTastyController;
    [SerializeField, Range(0f, 2f)] private float fruitFallSpeed;
    [SerializeField] private Vector2Int _boardPosition;

    public Sprite FruitSprite { get => fruitSpriteRenderer.sprite; set => fruitSpriteRenderer.sprite = value; }
    public Sprite TileSprite {
        get => tileSpriteRenderer.sprite;
        set {
            tileSpriteRenderer.sprite = value;
            tileSpriteRenderer.color = (value == null ? Color.clear : Color.white);
        }
    }
    public bool IsAnimating { get; private set; }
    public bool IsUncovered { get; set; }
    public bool NeedsUpdating { get; set; }
    public Vector2Int BoardPosition { get => _boardPosition; set => _boardPosition = value; }

    private void Awake( ) {
        notSoTastyController = FindFirstObjectByType<NotSoTastyController>( );

        FruitSprite = fruitSprites[Random.Range(0, fruitSprites.Count)];
    }

    private void OnMouseDown( ) {
        if (notSoTastyController.IsChainingFruits || !notSoTastyController.CanMatchFruit) {
            return;
        }

        notSoTastyController.AddToTileChain(this);
    }

    private void OnMouseEnter( ) {
        if (!notSoTastyController.IsChainingFruits || !notSoTastyController.CanMatchFruit) {
            return;
        }

        Vector2Int lastTilePosition = notSoTastyController.ChainedTiles[^1].BoardPosition;
        if (Mathf.Abs(BoardPosition.x - lastTilePosition.x) <= 1 && Mathf.Abs(BoardPosition.y - lastTilePosition.y) <= 1) {
            notSoTastyController.AddToTileChain(this);
        }
    }

    private void OnMouseUp( ) {
        if (!notSoTastyController.IsChainingFruits || !notSoTastyController.CanMatchFruit) {
            return;
        }

        notSoTastyController.ClearChain( );
    }

    /// <summary>
    /// Animate this tile's fruit falling a certain height
    /// </summary>
    /// <param name="fruitFallHeight">The height in world units the the fruit should fall</param>
    /// <param name="newFruitSprite">The new fruit sprite to be set to this tile. If no image is given, a random fruit sprite will be assigned</param>
    public void AnimateFruit(float fruitFallHeight, Sprite newFruitSprite = null) {
        // Set the new fruit sprite
        if (newFruitSprite == null) {
            FruitSprite = fruitSprites[Random.Range(0, fruitSprites.Count)];
        } else {
            FruitSprite = newFruitSprite;
        }

        IsAnimating = true;
        float fallDistance = BoardPosition.y - fruitFallHeight;
        fruitSpriteRenderer.transform.localPosition = new Vector3(fruitSpriteRenderer.transform.localPosition.x, fallDistance, 0f);
        fruitSpriteRenderer.transform.DOLocalMoveY(0, fruitFallSpeed * fallDistance)
            .SetEase(Ease.InQuad)
            .OnComplete(( ) => { IsAnimating = false; });
    }
}
