using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

// https://discussions.unity.com/t/write-save-and-read-load-data-to-and-from-json-file-on-android/785302/3

[CreateAssetMenu(fileName = "PlayerData", menuName = "PlayerData")]
public class PlayerData : ScriptableObject {
    public string RITchCode;
    public List<AppSessionData> AppSessionData;

    /// <summary>
    /// The current app session loaded should be the last app session data entry in the list
    /// </summary>
    public AppSessionData CurrentAppSession => AppSessionData[^1];

    private void OnEnable( ) {
        // When the app starts, set default values
        RITchCode = "";

        LoadNewRITchCode("NONE00");
    }

    /// <summary>
    /// Load a new RITch code. This saves the current data and loads the new RITch code's data
    /// </summary>
    /// <param name="newRITchCode">The new RITch code to log into</param>
    public void LoadNewRITchCode(string newRITchCode) {
        // Do not try to log into the same RITch code
        if (RITchCode == newRITchCode) {
            return;
        }

        // Save all currently loaded data to the current RITch code file
        // // If there is no app session data (i.e. the app just started), then skip this step
        // // Set the final playtime since the play session for the current RITch code is finished
        // // This should overwrite the entire file because the data should have already been loaded in when the RITch code was set

        RITchCode = newRITchCode;

        // Load JSON associated with the new RITch code, overwriting the currently loaded data
        // // Check for file with the RITch code. If none exist, create a new one
        AppSessionData = new List<AppSessionData>( );

        // Restart the play session stats (current time, total points, etc)
        AppSessionData.Add(new AppSessionData( ));

        Debug.Log("HERE");
    }
}

[System.Serializable]
public class AppSessionData {
    public string AppVersion;
    public string SessionID;
    public string StartTimeUTC;
    public int Points;
    public float PlaytimeSeconds;

    public List<GameSessionData> GameSessionData;
    public List<CheckInSessionData> CheckInSessionData;

    public AppSessionData( ) {
        AppVersion = Application.version;
        SessionID = GUID.Generate( ).ToString( );
        StartTimeUTC = DateTime.UtcNow.ToString("o");
        Points = 0;
        PlaytimeSeconds = 0;

        GameSessionData = new List<GameSessionData>( );
        CheckInSessionData = new List<CheckInSessionData>( );
    }
}

[System.Serializable]
public class CheckInSessionData {
    public string StartTimeUTC;
    public int Intensity;
    public List<string> Triggers;

    public CheckInSessionData( ) {
        StartTimeUTC = DateTime.UtcNow.ToString("o");
        Intensity = 0;
        Triggers = new List<string>( );
    }
}

[System.Serializable]
public class GameSessionData {
    public string StartTimeUTC;
    public string Name;
    public int Points;
    public float PlaytimeSeconds;

    public GameSessionData( ) {
        StartTimeUTC = DateTime.UtcNow.ToString("o");
        Name = "";
        Points = 0;
        PlaytimeSeconds = 0;
    }
}