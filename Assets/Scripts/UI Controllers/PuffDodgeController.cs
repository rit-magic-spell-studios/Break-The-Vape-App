using System.Collections.Generic;
using UnityEngine;

public class PuffDodgeController : GameController {
    [Header("PuffDodgeController")]
    [SerializeField] private GameObject vapeItemPrefab;
    [SerializeField] private Camera mainCamera;
    [SerializeField, Range(0f, 3f)] private float itemSpawnRate;
    [SerializeField, Range(1, 5)] private int itemSpawnBurst;
    [SerializeField] private int sliceLength;

    private float itemSpawnTimer;
    private List<Vector2> spawnPositions;
    private Queue<Vector2> slicePositions;

    protected override void Awake( ) {
        base.Awake( );

        float cameraHalfHeight = Camera.main.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;

        spawnPositions = new List<Vector2>( ) {
            new Vector2(-cameraHalfWidth * 1.5f, -cameraHalfWidth * 0.5f),
            new Vector2(cameraHalfWidth * 1.5f, -cameraHalfWidth * 0.5f)
        };
        slicePositions = new Queue<Vector2>( );
    }

    protected override void Update( ) {
        base.Update( );

        if (UIControllerState != UIState.GAME) {
            return;
        }

        // Get inputs from either touch or mouse
        if (Input.touchCount == 1) {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved) {
                ExtendSlice(touch.position);
            }
        } else if (Input.GetMouseButton(0)) {
            ExtendSlice(Input.mousePosition);
        } else if (slicePositions.Count > 0) {
            slicePositions.Clear( );
        }

        // Spawn vape items periodically
        itemSpawnTimer += Time.deltaTime;
        if (itemSpawnTimer >= itemSpawnRate) {
            for (int i = 0; i < itemSpawnBurst; i++) {
                Vector2 spawnPosition = spawnPositions[Random.Range(0, spawnPositions.Count)];
                Quaternion spawnRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                Instantiate(vapeItemPrefab, spawnPosition, spawnRotation);
            }

            itemSpawnTimer -= itemSpawnRate;
        }
    }

    /// <summary>
    /// Extend the slice as the user drags the mouse/touch input across the screen
    /// </summary>
    /// <param name="position">The position to add</param>
    private void ExtendSlice(Vector2 position) {
        // Remove the oldest position in the queue if it is too big
        if (slicePositions.Count == sliceLength) {
            slicePositions.Dequeue( );
        }

        slicePositions.Enqueue(mainCamera.ScreenToWorldPoint(position));
    }
}
