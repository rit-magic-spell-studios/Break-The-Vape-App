using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
    [SerializeField] private SpriteRenderer tileSpriteRenderer;
    [SerializeField] private SpriteRenderer fruitSpriteRenderer;
    [SerializeField] private List<Sprite> fruitSprites;
    [SerializeField] private NotSoTastyController notSoTastyController;
    [SerializeField, Range(0f, 2f)] private float fruitFallSpeed;
    [SerializeField, Min(0)] private int fruitPoints;
    [SerializeField] private Vector2Int _boardPosition;

    public Sprite FruitSprite { get => fruitSpriteRenderer.sprite; set => fruitSpriteRenderer.sprite = value; }
    public Sprite TileSprite { get => tileSpriteRenderer.sprite; set => tileSpriteRenderer.sprite = value; }
    public bool IsAnimating { get; private set; }
    public bool IsUncovered { get; set; }
    public bool NeedsUpdating { get; set; }
    public Vector2Int BoardPosition { get => _boardPosition; set => _boardPosition = value; }

    private void Awake( ) {
        notSoTastyController = FindFirstObjectByType<NotSoTastyController>( );

        FruitSprite = fruitSprites[Random.Range(0, fruitSprites.Count)];
    }

    private void OnMouseDown( ) {
        if (notSoTastyController.IsChainingFruits) {
            return;
        }

        notSoTastyController.AddToTileChain(this);
    }

    private void OnMouseEnter( ) {
        if (!notSoTastyController.IsChainingFruits) {
            return;
        }

        Vector2Int lastTilePosition = notSoTastyController.LastChainedTile.BoardPosition;

        if (Mathf.Abs(BoardPosition.x - lastTilePosition.x) <= 1 && Mathf.Abs(BoardPosition.y - lastTilePosition.y) <= 1) {
            notSoTastyController.AddToTileChain(this);
        }
    }

    private void OnMouseUp( ) {
        if (!notSoTastyController.IsChainingFruits) {
            return;
        }

        if (notSoTastyController.ChainedTiles.Count >= notSoTastyController.MinFruitChainLength) {
            notSoTastyController.AddPoints(notSoTastyController.LastChainedTile.transform.position, notSoTastyController.ChainedTiles.Count * fruitPoints);
            notSoTastyController.UncoverSecretTiles( );
            notSoTastyController.UpdateBoard( );
        }

        notSoTastyController.ChainedTiles.Clear( );
    }
}
