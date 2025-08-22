using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PuffDodgeController : GameController {
    [Header("PuffDodgeController")]
    [SerializeField] private GameObject vapeItemPrefab;
    [SerializeField] private TrailRenderer sliceTrailRenderer;
    [Space]
    [SerializeField, Range(0f, 3f)] private float itemSpawnRate;
    [SerializeField, Range(1, 5)] private int itemSpawnBurstMin;
    [SerializeField, Range(1, 5)] private int itemSpawnBurstMax;
    [SerializeField, Range(0f, 1f)] private float itemSpawnBurstDelay;
    [SerializeField, Range(10, 50)] private int targetDestroyedItems;
    [SerializeField, Range(0f, 1f)] private float sliceTrailTime;
    [SerializeField, Range(0, 100)] private int sliceFrameDelay;
    [SerializeField] private LayerMask vapeItemLayer;

    private float itemSpawnTimer;
    private int sliceFrameCounter;
    private List<Vector2> slicePositions;

    public List<GameObject> VapeItems { get; private set; }

    public int DestroyedItems {
        get => _destroyedItems;
        set {
            _destroyedItems = Mathf.Clamp(value, 0, targetDestroyedItems);
            destroyedItemsLabel.text = $"{_destroyedItems} / {targetDestroyedItems} items destroyed!";

            if (_destroyedItems >= targetDestroyedItems) {
                WinGame( );
            }
        }
    }
    private int _destroyedItems;
    private Label destroyedItemsLabel;

    protected override void Awake( ) {
        base.Awake( );

        destroyedItemsLabel = ui.Q<Label>("DestroyedItemsLabel");
        VapeItems = new List<GameObject>( );
        slicePositions = new List<Vector2>( );
        DestroyedItems = 0;
    }

    protected override void Update( ) {
        base.Update( );

        if (!IsPlayingGame || DestroyedItems >= targetDestroyedItems) {
            return;
        }

        if (IsTouchingScreen) {
            sliceTrailRenderer.transform.position = LastTouchWorldPosition;

            // Enqueue the current touch position
            // Make sure the queue does not exceed the delay count
            slicePositions.Add(LastTouchWorldPosition);
            while (slicePositions.Count >= Mathf.FloorToInt((1 / Time.deltaTime) * sliceTrailTime / 2f)) {
                slicePositions.RemoveAt(0);
            }

            // This is to make sure there isn't a streak when the trail renderer's position gets set to a new location
            sliceFrameCounter++;
            if (sliceTrailRenderer.time == 0f && sliceFrameCounter >= sliceFrameDelay) {
                sliceTrailRenderer.time = sliceTrailTime;
            }

            // Check to see if one of the slice positions overlaps the collider for a vape item
            for (int i = slicePositions.Count - 1; i >= 0; i--) {
                RaycastHit2D[ ] hits = Physics2D.RaycastAll(slicePositions[i], Vector2.up, 0.05f, vapeItemLayer.value);
                for (int j = 0; j < hits.Length; j++) {
                    hits[j].transform.GetComponent<VapeItem>( ).Slice( );
                }
            }
        } else {
            // Reset slice trail renderer variables
            sliceTrailRenderer.time = 0f;
            sliceFrameCounter = 0;

            // Continue to clear the slice positions gradually
            if (slicePositions.Count > 0) {
                slicePositions.RemoveAt(0);
            }
        }

        // Spawn vape items periodically
        itemSpawnTimer += Time.deltaTime;
        if (itemSpawnTimer >= itemSpawnRate) {
            StartCoroutine(SpawnVapeItems( ));
            itemSpawnTimer -= itemSpawnRate;
        }
    }

    /// <summary>
    /// Spawn a burst of vape items that arc onto the screen
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnVapeItems( ) {
        int itemSpawnCount = Random.Range(itemSpawnBurstMin, itemSpawnBurstMax);
        for (int i = 0; i < itemSpawnCount; i++) {
            Vector2 spawnPosition = new Vector2(cameraHalfWidth * 1.5f, -cameraHalfHeight * 0.5f);
            if (Random.Range(0f, 1f) > 0.5f) {
                spawnPosition.x *= -1f;
            }
            Quaternion spawnRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            VapeItems.Add(Instantiate(vapeItemPrefab, spawnPosition, spawnRotation, objectContainer));

            yield return new WaitForSeconds(itemSpawnBurstDelay);
        }

        yield return null;
    }
}
