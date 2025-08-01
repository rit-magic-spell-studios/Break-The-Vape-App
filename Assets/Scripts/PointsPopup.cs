using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PointsPopup : MonoBehaviour {
    [Header("PointsPopup")]
    [SerializeField] private TextMeshPro text;
    [SerializeField, Range(0f, 5f)] private float lifetime;
    [SerializeField, Range(0f, 5f)] private float driftDistance;
    [SerializeField, Range(0f, 180f)] private float rotateAngleRange;

    private float timer;

    private void Awake( ) {
        transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-rotateAngleRange, rotateAngleRange));
    }

    private void Start( ) {
        Vector3 toPosition = transform.position + (Vector3) (Random.insideUnitCircle.normalized * driftDistance);
        Quaternion toRotation = transform.rotation * Quaternion.Euler(0, 0, Random.Range(-rotateAngleRange, rotateAngleRange));
        transform.DOMove(toPosition, lifetime * 0.95f);
        transform.DORotateQuaternion(toRotation, lifetime * 0.95f);
        text.DOFade(0f, lifetime * 0.95f).SetEase(Ease.InCirc);
    }

    private void Update( ) {
        timer += Time.deltaTime;
        if (timer >= lifetime) {
            Destroy(gameObject);
        }
    }
}
