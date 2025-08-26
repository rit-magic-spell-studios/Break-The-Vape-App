using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FruitType {
    MANGO, MINT, PEACH, RASPBERRY, STRAWBERRY, WATERMELON, NONE = -1
}

public enum FruitSpriteType {
    NORMAL, EYES, ROTTEN
}

public class Tile : MonoBehaviour {
    [SerializeField] private SpriteRenderer tileSpriteRenderer;
    [SerializeField] private SpriteRenderer fruitSpriteRenderer;
    [SerializeField] private Transform fruitTransform;
    [SerializeField] private List<Sprite> normalFruitSprites;
    [SerializeField] private List<Sprite> eyesFruitSprites;
    [SerializeField] private List<Sprite> rottenFruitSprites;
    [SerializeField] private NotSoTastyController notSoTastyController;
    [Space]
    [SerializeField, Range(0f, 2f)] private float fruitFallSpeed;
    [SerializeField, Range(0f, 2f)] private float fruitEnlargedScale;
    [SerializeField, Range(0f, 2f)] private float fruitShrunkScale;
    [SerializeField, Range(0f, 2f)] private float fruitStateAnimationSpeed;

    private Sequence stateAnimationSequence;

    public FruitType FruitType { get; private set; }
    public FruitSpriteType FruitSpriteType { get; set; }
    public bool IsAnimating { get; private set; }
    public bool IsUncovered { get; set; }
    public bool NeedsUpdating { get; set; }
    public Vector2Int BoardPosition { get; set; }

    public Sprite TileSprite {
        get => tileSpriteRenderer.sprite;
        set {
            tileSpriteRenderer.sprite = value;
            tileSpriteRenderer.color = (value == null ? Color.clear : Color.white);
        }
    }

    private void Awake( ) {
        notSoTastyController = FindFirstObjectByType<NotSoTastyController>( );

        FruitType = (FruitType) UnityEngine.Random.Range(0, normalFruitSprites.Count);
        FruitSpriteType = FruitSpriteType.NORMAL;
        UpdateFruitVisual(skipAnimation: true);
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

    public void UpdateFruitVisual(bool skipAnimation = false) {
        if (!skipAnimation) {
            stateAnimationSequence?.Kill( );
            IsAnimating = true;
        }

        switch (FruitSpriteType) {
            case FruitSpriteType.NORMAL:
                if (skipAnimation) {
                    fruitSpriteRenderer.sprite = normalFruitSprites[(int) FruitType];
                } else {
                    stateAnimationSequence = DOTween.Sequence( )
                        .Append(fruitTransform.DOScale(Vector3.one, fruitStateAnimationSpeed))
                        .InsertCallback(fruitStateAnimationSpeed / 2f, ( ) => {fruitSpriteRenderer.sprite = normalFruitSprites[(int) FruitType];})
                        .OnComplete(( ) => { IsAnimating = false; });
                }

                break;
            case FruitSpriteType.EYES:
                if (skipAnimation) {
                    fruitSpriteRenderer.sprite = eyesFruitSprites[(int) FruitType];
                } else {
                    stateAnimationSequence = DOTween.Sequence( )
                        .Append(fruitTransform.DOScale(new Vector3(fruitEnlargedScale, fruitEnlargedScale, 1f), fruitStateAnimationSpeed))
                        .InsertCallback(fruitStateAnimationSpeed / 2f, ( ) => {fruitSpriteRenderer.sprite = eyesFruitSprites[(int) FruitType];})
                        .OnComplete(( ) => { IsAnimating = false; });
                }

                break;
            case FruitSpriteType.ROTTEN:
                if (skipAnimation) {
                    fruitSpriteRenderer.sprite = rottenFruitSprites[(int) FruitType];
                } else {
                    stateAnimationSequence = DOTween.Sequence( )
                        .Append(fruitTransform.DOScale(new Vector3(fruitShrunkScale, fruitShrunkScale, 1f), fruitStateAnimationSpeed / 2f))
                        .InsertCallback(fruitStateAnimationSpeed / 2f, ( ) => {
                            NeedsUpdating = true;
                            fruitSpriteRenderer.sprite = rottenFruitSprites[(int) FruitType];

                            if (IsUncovered) {
                                return;
                            }
                            IsUncovered = true;
                            TileSprite = notSoTastyController.GetSecretTileAtPosition(this);
                        })
                        .Append(fruitTransform.DOScale(new Vector3(fruitEnlargedScale, fruitEnlargedScale, 1f), fruitStateAnimationSpeed / 2f))
                        .Append(fruitSpriteRenderer.DOFade(0f, fruitStateAnimationSpeed / 2f))
                        .OnComplete(( ) => { IsAnimating = false; });
                }

                break;
        }
    }

    /// <summary>
    /// Animate this tile's fruit falling a certain height
    /// </summary>
    public void AnimateFruitFalling(float fruitFallHeight, FruitType newFruitType) {
        // Set the new fruit sprite
        if (newFruitType == FruitType.NONE) {
            FruitType = (FruitType) UnityEngine.Random.Range(0, normalFruitSprites.Count);
        } else {
            FruitType = newFruitType;
        }

        IsAnimating = true;
        float fallDistance = BoardPosition.y - fruitFallHeight;
        fruitSpriteRenderer.transform.localPosition = new Vector3(fruitSpriteRenderer.transform.localPosition.x, fallDistance, 0f);
        fruitSpriteRenderer.transform.DOLocalMoveY(0, fruitFallSpeed * fallDistance)
            .SetEase(Ease.InQuad)
            .OnComplete(( ) => { IsAnimating = false; });
    }
}
