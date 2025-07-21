using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JSONManager : Singleton<JSONManager> {
    [HideInInspector] public PlayerData PlayerData;
    [SerializeField] private bool doDataSaving;

    public static string RITchCode { get => Instance.PlayerData.RITchCode; private set => Instance.PlayerData.RITchCode = value; }
    public static List<AppSessionData> AppSessionData { get => Instance.PlayerData.AppSessionData; private set => Instance.PlayerData.AppSessionData = value; }
   
    /// <summary>
    /// Whether or not at least one check in session has been completed
    /// </summary>
    public static bool HasCompletedCheckIn => (ActiveAppSession.CheckInSessionData.Count > 0);

    /// <summary>
    /// The currently active app session (the last app session data in the session list)
    /// </summary>
    public static AppSessionData ActiveAppSession {
        get {
            if (AppSessionData.Count == 0) {
                return null;
            }

            return AppSessionData[^1];
        }
    }

    /// <summary>
    /// The currently active game session (the last game session data in the session list)
    /// </summary>
    public static GameSessionData ActiveGameSession {
        get {
            if (ActiveAppSession.GameSessionData.Count == 0) {
                return null;
            }

            return ActiveAppSession.GameSessionData[^1];
        }
    }

    /// <summary>
    /// The currently active check in session (the last check in session data in the session list)
    /// </summary>
    public static CheckInSessionData ActiveCheckInSession {
        get {
            if (ActiveAppSession.CheckInSessionData.Count == 0) {
                return null;
            }

            return ActiveAppSession.CheckInSessionData[^1];
        }
    }

    /// <summary>
    /// The data path to where all json files will be saving
    /// </summary>
    private string DataPath {
        get {
            if (Directory.Exists(Application.persistentDataPath)) {
                return Path.Combine(Application.persistentDataPath, $"{PlayerData.RITchCode}.json");
            }

            return Path.Combine(Application.streamingAssetsPath, $"{PlayerData.RITchCode}.json");
        }
    }

    protected override void Awake( ) {
        base.Awake( );

        // When the app starts, set default values
        PlayerData = new PlayerData( );

        // Load the default RITch code at the start of the game
        LoadNewRITchCode("NONE00");
    }

    private void OnDisable( ) {
        SavePlayerData( );
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
        SavePlayerData( );

        // Set the new RITch code
        RITchCode = newRITchCode;

        // Load JSON associated with the new RITch code, overwriting the currently loaded data
        LoadPlayerData( );

        // Restart the play session stats (current time, total points, etc)
        AppSessionData.Add(new AppSessionData(Application.version));
    }

    /// <summary>
    /// Save the current player data to its RITch code file
    /// </summary>
    public void SavePlayerData( ) {
        // Do not try to save to the file if there is no RITch code
        if (RITchCode == "" || !doDataSaving) {
            return;
        }

        // Check to see if there is a file at the data path
        CheckForFile(DataPath);

        // This should overwrite the entire file because the data should have already been loaded in when the RITch code was set
        string json = JsonUtility.ToJson(PlayerData);
        using StreamWriter streamWriter = new StreamWriter(DataPath);
        streamWriter.Write(json);

        Debug.Log($"Saved data to {DataPath}: {json}");
    }

    /// <summary>
    /// Load player data and overwrite the currently loaded player data
    /// </summary>
    private void LoadPlayerData( ) {
        if (!doDataSaving) {
            return;
        }

        // Check to see if there is a file at the data path
        CheckForFile(DataPath);

        // Read the json file
        using StreamReader streamReader = new StreamReader(DataPath);
        string json = streamReader.ReadToEnd( );

        // Only load the data if the json file has stuff in it
        if (json.Length > 0) {
            PlayerData tempPlayerData = JsonUtility.FromJson<PlayerData>(json);
            AppSessionData = tempPlayerData.AppSessionData;
            Debug.Log($"Loaded data from {DataPath}: {json}");
        } else {
            AppSessionData.Clear( );
        }
    }

    /// <summary>
    /// Check if the file at the specified file path exists. If it does not, a new file will be created
    /// </summary>
    /// <param name="filePath">The file path to check</param>
    /// <returns>Whether or not the file exists. If false, a new file was created automatically</returns>
    private bool CheckForFile(string filePath) {
        if (File.Exists(filePath)) {
            return true;
        }

        File.Create(filePath).Close( );
        return false;
    }
}
