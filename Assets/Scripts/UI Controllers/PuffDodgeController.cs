using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PuffDodgeController : GameController {
    [Header("PuffDodgeController")]
    [SerializeField] private GameObject vapeItemPrefab;
    [SerializeField] private Camera mainCamera;
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
    [Space]
    [SerializeField] private int _destroyedItems;

    private float itemSpawnTimer;
    private Vector2 slicePosition;
    private Vector2 lastSlicePosition;
    private bool isSlicing;
    private int sliceFrameCounter;

    public List<GameObject> VapeItems { get; private set; }

    public int DestroyedItems {
        get => _destroyedItems;
        set {
            _destroyedItems = Mathf.Clamp(value, 0, targetDestroyedItems);
            destroyedItemsLabel.text = $"{_destroyedItems} / {targetDestroyedItems} items destroyed!";

            if (_destroyedItems >= targetDestroyedItems) {
                DelayAction(( ) => { UIControllerState = UIState.WIN; }, 2f);

                for (int i = 0; i < VapeItems.Count; i++) {
                    Destroy(VapeItems[i]);
                }
                VapeItems.Clear( );
            }
        }
    }
    private Label destroyedItemsLabel;

    private float cameraHalfWidth, cameraHalfHeight;

    protected override void Awake( ) {
        base.Awake( );

        destroyedItemsLabel = ui.Q<Label>("DestroyedItemsLabel");
        VapeItems = new List<GameObject>( );
        DestroyedItems = 0;

        cameraHalfHeight = Camera.main.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;

        isSlicing = false;
        lastSlicePosition = Vector2.positiveInfinity;
    }

    protected override void Update( ) {
        base.Update( );

        if (UIControllerState != UIState.GAME || DestroyedItems >= targetDestroyedItems) {
            return;
        }

        // Get inputs from either touch or mouse
        if (Input.touchCount == 1) {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved) {
                slicePosition = mainCamera.ScreenToWorldPoint(touch.position);
                isSlicing = true;
            }
        } else if (Input.GetMouseButton(0)) {
            slicePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            isSlicing = true;
        } else {
            isSlicing = false;
            sliceTrailRenderer.time = 0f;
            sliceFrameCounter = 0;
        }

        if (isSlicing) {
            sliceTrailRenderer.transform.position = slicePosition;
            sliceFrameCounter++;
            if (sliceTrailRenderer.time == 0f && sliceFrameCounter >= sliceFrameDelay) {
                sliceTrailRenderer.time = sliceTrailTime;
            }

            // Check to see if one of the slice positions overlaps the collider for a vape item
            // For now, just destroy the vape item and increment the number of destroyed items
            if (lastSlicePosition != slicePosition) {
                RaycastHit2D[ ] hits = Physics2D.RaycastAll(slicePosition, Vector2.up, 0.05f, vapeItemLayer.value);
                for (int i = 0; i < hits.Length; i++) {
                    GameObject hitObject = hits[i].transform.gameObject;

                    VapeItems.Remove(hitObject);
                    Destroy(hitObject);

                    DestroyedItems++;
                    GamePoints += 50;
                }
            }
        }
        lastSlicePosition = slicePosition;

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
            VapeItems.Add(Instantiate(vapeItemPrefab, spawnPosition, spawnRotation));

            yield return new WaitForSeconds(itemSpawnBurstDelay);
        }

        yield return null;
    }
}
