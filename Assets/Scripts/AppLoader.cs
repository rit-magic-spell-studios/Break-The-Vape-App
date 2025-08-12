using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppLoader : MonoBehaviour {
    [Header("App Loader")]
    [SerializeField] private GameObject dataManagerPrefab;
    [SerializeField] private GameObject backgroundBubbleManagerPrefab;

    private void Awake( ) {
        if (DataManager.Instance == null) {
            Instantiate(dataManagerPrefab);
        }

        if (BackgroundBubbleManager.Instance == null) {
            Instantiate(backgroundBubbleManagerPrefab);
        }
    }
}
