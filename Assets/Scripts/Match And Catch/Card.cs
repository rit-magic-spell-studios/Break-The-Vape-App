using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour {
    [Header("Card")]
    [SerializeField] private MatchAndCatchController matchAndCatchController;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite cardBack;
    [SerializeField, Range(0f, 5f)] private float cardFlipSeconds;

    public Sprite CardFront { get; set; }

    public float Size {
        get => _size;
        set {
            _size = value;
            transform.localScale = new Vector2(_size, _size);
        }
    }
    private float _size;

    public bool IsFlippedOver {
        get => _isFlippedOver;
        private set {
            _isFlippedOver = value;
            spriteRenderer.sprite = (_isFlippedOver ? CardFront : cardBack);
        }
    }
    private bool _isFlippedOver;

    private void Awake( ) {
        IsFlippedOver = false;
        matchAndCatchController = FindFirstObjectByType<MatchAndCatchController>( );
    }

    private void OnMouseDown( ) {
        if (matchAndCatchController.IsPlayingGame && matchAndCatchController.CanFlipCard && !IsFlippedOver) {
            FlipCard( );
            matchAndCatchController.FlippedCards.Add(this);
            matchAndCatchController.CheckForMatch( );
        }
    }

    public void FlipCard( ) {
        DOTween.Sequence( )
            .Append(transform.DOScaleX(0, cardFlipSeconds / 2f).SetEase(Ease.InOutQuad))
            .AppendCallback(( ) => { IsFlippedOver = !IsFlippedOver; })
            .Append(transform.DOScaleX(Size, cardFlipSeconds / 2f).SetEase(Ease.InOutQuad));
    }
}
