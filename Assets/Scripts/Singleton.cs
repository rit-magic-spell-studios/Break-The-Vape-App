using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T> {
    private static T _instance;

    public static T Instance {
        get {
            if (_instance == null) {
                _instance = FindFirstObjectByType<T>( );
            }

            return _instance;
        }
        set => _instance = value;
    }

    protected virtual void Awake( ) {
        // Make sure only one instance of the singleton is in the game at one time
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this as T;
        }

        // Have the singleton persist between scenes
        DontDestroyOnLoad(gameObject);
    }
}