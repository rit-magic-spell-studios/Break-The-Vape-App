using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PointsPopup : MonoBehaviour {
    [Header("PointsPopup")]
    [SerializeField] private TextMeshPro textComponent;
    [SerializeField, Range(0f, 5f)] private float lifetime;
    [SerializeField, Range(0f, 5f)] private float minDriftDistance;
    [SerializeField, Range(0f, 5f)] private float maxDriftDistance;
    [SerializeField, Range(-360f, 360f)] private float minRotateAngle;
    [SerializeField, Range(-360f, 360f)] private float maxRotateAngle;
    [SerializeField, Range(0f, 500f)] private int minScalePoints;
    [SerializeField, Range(0f, 500f)] private int maxScalePoints;
    [SerializeField] private Gradient pointGradient;

    private float timer;
    private int _points;

    /// <summary>
    /// The current points for this points popup
    /// </summary>
    public int Points {
        get => _points;
        set {
            _points = value;
            float i = (float) (_points - minScalePoints) / (maxScalePoints - minScalePoints);

            textComponent.text = $"+{_points}";
            textComponent.color = pointGradient.Evaluate(i);
            transform.localScale = Mathf.Lerp(0.5f, 1.5f, i) * Vector2.one;
        }
    }

    private void Awake( ) {
        transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(minRotateAngle, maxRotateAngle));
    }

    private void Start( ) {
        Vector3 toPosition = transform.position + (Vector3) (Random.insideUnitCircle.normalized * Random.Range(minDriftDistance, maxDriftDistance));
        Quaternion toRotation = transform.rotation * Quaternion.Euler(0, 0, Random.Range(minRotateAngle, maxRotateAngle));
        transform.DOMove(toPosition, lifetime);
        transform.DORotateQuaternion(toRotation, lifetime);
        textComponent.DOFade(0f, lifetime)
            .SetEase(Ease.InCirc)
            .OnUpdate(( ) => { textComponent.sortingOrder = (int) (100f * (lifetime - timer) / lifetime); });
    }

    private void Update( ) {
        timer += Time.deltaTime;
        if (timer >= lifetime) {
            transform.DOKill( );
            textComponent.DOKill( );
            Destroy(gameObject);
        }
    }
}
