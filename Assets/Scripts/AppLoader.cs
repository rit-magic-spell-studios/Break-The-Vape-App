using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppLoader : MonoBehaviour {
    [Header("App Loader")]
    [SerializeField] private GameObject dataManagerPrefab;

    private void Awake( ) {
        if (DataManager.Instance == null) {
            Instantiate(dataManagerPrefab);
        }
    }
}
