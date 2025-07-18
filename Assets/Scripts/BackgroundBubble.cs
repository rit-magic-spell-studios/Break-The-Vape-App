using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundBubble : MonoBehaviour {
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField, Range(0f, 4f)] private float minMoveSpeed;
    [SerializeField, Range(0f, 4f)] private float maxMoveSpeed;
    [SerializeField, Range(0f, 50f)] private float minRotateSpeed;
    [SerializeField, Range(0f, 50f)] private float maxRotateSpeed;
    [SerializeField, Range(0f, 5f)] private float boundsPadding;

    private float cameraHalfWidth;
    private float cameraHalfHeight;
    private Vector3 direction;
    private float moveSpeed;
    private float rotateSpeed;

    /// <summary>
    /// The current color of this background bubble
    /// </summary>
    public Color Color { get => spriteRenderer.color; set => spriteRenderer.color = value; }

    /// <summary>
    /// The current alpha of this background bubble
    /// </summary>
    public float Alpha {
        get => Color.a;
        set {
            Color color = Color;
            color.a = value;
            Color = color;
        }
    }

    /// <summary>
    /// Whether or not this background bubble is inside the bounds of the camera on the x axis
    /// </summary>
    private bool IsInBoundsX => transform.position.x >= -cameraHalfWidth + boundsPadding && transform.position.x <= cameraHalfWidth - boundsPadding;

    /// <summary>
    /// Whether or not this background bubble is inside the bounds of the camera on the y axis
    /// </summary>
    private bool IsInBoundsY => transform.position.y >= -cameraHalfHeight + boundsPadding && transform.position.y <= cameraHalfHeight - boundsPadding;

    private void Awake( ) {
        spriteRenderer = GetComponent<SpriteRenderer>( );

        cameraHalfHeight = Camera.main.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;

        RandomizeValues( );
    }

    private void Update( ) {
        transform.position += moveSpeed * Time.deltaTime * direction;
        transform.rotation *= Quaternion.Euler(0, 0, rotateSpeed * Time.deltaTime);

        if (!IsInBoundsX) {
            direction.x *= -1;
        }
        if (!IsInBoundsY) {
            direction.y *= -1;
        }
    }

    /// <summary>
    /// Randomize all the values of this background bubble
    /// </summary>
    public void RandomizeValues( ) {
        direction = Random.insideUnitCircle.normalized;
        moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        rotateSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);

        float positionX = Random.Range(-cameraHalfWidth + boundsPadding, cameraHalfWidth - boundsPadding);
        float positionY = Random.Range(-cameraHalfHeight + boundsPadding, cameraHalfHeight - boundsPadding);
        Vector3 position = new Vector3(positionX, positionY);
        Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        transform.SetPositionAndRotation(position, rotation);
    }
}
