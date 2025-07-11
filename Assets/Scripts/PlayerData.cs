using System;
using System.Collections.Generic;
using UnityEditor;

// https://discussions.unity.com/t/write-save-and-read-load-data-to-and-from-json-file-on-android/785302/3

[Serializable]
public class PlayerData {
    public string RITchCode;
    public List<AppSessionData> AppSessionData;

    public PlayerData( ) {
        RITchCode = "";
        AppSessionData = new List<AppSessionData>( );
    }
}

[Serializable]
public class AppSessionData {
    public string AppVersion;
    public string SessionID;
    public string StartTimeUTC;
    public int Points;
    public float PlaytimeSeconds;

    public List<GameSessionData> GameSessionData;
    public List<CheckInSessionData> CheckInSessionData;

    public AppSessionData(string appVersion) {
        AppVersion = appVersion;
        SessionID = GUID.Generate( ).ToString( );
        StartTimeUTC = DateTime.UtcNow.ToString("o");
        Points = 0;
        PlaytimeSeconds = 0;

        GameSessionData = new List<GameSessionData>( );
        CheckInSessionData = new List<CheckInSessionData>( );
    }
}

[Serializable]
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

[Serializable]
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