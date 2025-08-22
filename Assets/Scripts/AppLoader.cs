using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppLoader : MonoBehaviour {
    [Header("App Loader")]
    [SerializeField] private GameObject dataManagerPrefab;
    [SerializeField] private GameObject backgroundBubbleManagerPrefab;
    [SerializeField] private GameObject soundManagerPrefab;

    private void Awake( ) {
        if (DataManager.Instance == null) {
            Instantiate(dataManagerPrefab);
        }

        if (BackgroundBubbleManager.Instance == null) {
            Instantiate(backgroundBubbleManagerPrefab);
        }

        if (SoundManager.Instance == null) {
            Instantiate(soundManagerPrefab);
        }

        // Set framerate of the application to the native refresh rate of the display
        QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = Mathf.CeilToInt((float) Screen.currentResolution.refreshRateRatio.value);
        Application.targetFrameRate = 60;
    }
}
