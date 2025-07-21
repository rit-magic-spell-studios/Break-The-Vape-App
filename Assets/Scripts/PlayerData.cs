using System;
using System.Collections.Generic;
using UnityEditor;

public delegate void ValueChangeEvent( );

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
    public List<GameSessionData> GameSessionData;
    public List<CheckInSessionData> CheckInSessionData;

    public int TotalPoints {
        get => _totalPoints;
        set {
            _totalPoints = value;
            OnTotalPointsChange?.Invoke( );
        }
    }
    private int _totalPoints;

    public float PlaytimeSeconds {
        get => _playtimeSeconds;
        set {
            _playtimeSeconds = value;
            OnPlaytimeSecondsChange?.Invoke( );
        }
    }
    private float _playtimeSeconds;

    public ValueChangeEvent OnTotalPointsChange;
    public ValueChangeEvent OnPlaytimeSecondsChange;

    public AppSessionData(string appVersion) {
        AppVersion = appVersion;
        //SessionID = GUID.Generate( ).ToString( );
        SessionID = "NA";
        StartTimeUTC = DateTime.UtcNow.ToString("o");
        TotalPoints = 0;
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

    public float PlaytimeSeconds {
        get => _playtimeSeconds;
        set {
            _playtimeSeconds = value;
            OnPlaytimeSecondsChange?.Invoke( );
        }
    }
    private float _playtimeSeconds;

    public ValueChangeEvent OnPlaytimeSecondsChange;

    public CheckInSessionData( ) {
        StartTimeUTC = DateTime.UtcNow.ToString("o");
        Intensity = -1;
        Triggers = new List<string>( );
        PlaytimeSeconds = 0;
    }
}

[Serializable]
public class GameSessionData {
    public string StartTimeUTC;
    public string Name;

    public int Points {
        get => _points;
        set {
            _points = value;
            OnPointsChange?.Invoke( );
        }
    }
    private int _points;

    public float PlaytimeSeconds {
        get => _playtimeSeconds;
        set {
            _playtimeSeconds = value;
            OnPlaytimeSecondsChange?.Invoke( );
        }
    }
    private float _playtimeSeconds;

    public ValueChangeEvent OnPointsChange;
    public ValueChangeEvent OnPlaytimeSecondsChange;

    public GameSessionData( ) {
        StartTimeUTC = DateTime.UtcNow.ToString("o");
        Name = "";
        Points = 0;
        PlaytimeSeconds = 0;
    }
}