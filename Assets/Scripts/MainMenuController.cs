using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour {
    private VisualElement ui;

    private void OnValidate( ) {
        ui = GetComponent<UIDocument>( ).rootVisualElement;
    }

    private void Awake( ) {
        OnValidate( );

        // Set up menu button functionality
        ui.Q<Button>("CraveSmashButton").clicked += ( ) => { GoToScene(1); };
        //ui.Q<Button>("PuffDodgeButtom").clicked += ( ) => { GoToScene(2); };
        //ui.Q<Button>("BlowItOffButton").clicked += ( ) => { GoToScene(3); };
        //ui.Q<Button>("NotSoTastyButton").clicked += ( ) => { GoToScene(4); };
        //ui.Q<Button>("MatchAndCatchButton").clicked += ( ) => { GoToScene(5); };

        ui.Q<Button>("QuitButton").clicked += ( ) => { 
            Application.Quit( );
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        };
    }

    /// <summary>
    /// Go to a specific scene based on the scene build index
    /// </summary>
    /// <param name="sceneIndex">The scene build index to go to</param>
    private void GoToScene(int sceneIndex) {
        SceneManager.LoadScene(sceneIndex);
    }
}
