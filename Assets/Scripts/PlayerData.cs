using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void ValueChangeEvent( );

[Serializable]
public class UserData {
    public string Age;
    public string Environment;
    public int DaysVapedDuringPastWeek;

    public UserData( ) {
        Age = "NA";
        Environment = "NA";
        DaysVapedDuringPastWeek = -1;
    }
}

[Serializable]
public class AppSessionData : SessionData {
    public int TotalPointsEarned;

    public ValueChangeEvent OnTotalPointsEarnedChange;

    public int TotalPointsEarnedValue {
        get => TotalPointsEarned;
        set {
            TotalPointsEarned = value;
            OnTotalPointsEarnedChange?.Invoke( );
        }
    }

    public AppSessionData(string ritchCode) : base(ritchCode) {
        TotalPointsEarned = 0;
    }

    public override void ClearAllDelegates( ) {
        base.ClearAllDelegates( );
        OnTotalPointsEarnedChange = null;
    }

    public override void InvokeAllDelegates( ) {
        base.InvokeAllDelegates( );
        OnTotalPointsEarnedChange?.Invoke( );
    }

    public override string ToString( ) {
        return "AppSession";
    }
}

[Serializable]
public class CheckInSessionData : SessionData {
    public int CravingIntensity;
    public List<string> CravingTriggers;

    public CheckInSessionData(string ritchCode) : base(ritchCode) {
        CravingIntensity = -1;
        CravingTriggers = new List<string>( );
    }

    public override string ToString( ) {
        return "CheckInSession";
    }
}

[Serializable]
public class GameSessionData : SessionData {
    public string GameName;
    public int PointsEarned;
    public int TotalPointsEarned;
    public int CravingIntensity;

    private readonly int initialTotalPoints;

    public ValueChangeEvent OnPointsEarnedChange;

    public int PointsEarnedValue {
        get => PointsEarned;
        set {
            PointsEarned = value;
            TotalPointsEarned = initialTotalPoints + PointsEarned;
            OnPointsEarnedChange?.Invoke( );
        }
    }

    public GameSessionData(string gameName, string ritchCode, int initialTotalPoints) : base(ritchCode) {
        GameName = gameName;
        PointsEarned = 0;
        this.initialTotalPoints = initialTotalPoints;
        TotalPointsEarned = initialTotalPoints;
        CravingIntensity = -1;
    }

    public override void ClearAllDelegates( ) {
        base.ClearAllDelegates( );
        OnPointsEarnedChange = null;
    }

    public override void InvokeAllDelegates( ) {
        base.InvokeAllDelegates( );
        OnPointsEarnedChange?.Invoke( );
    }

    public override string ToString( ) {
        return "GameSession";
    }
}

[Serializable]
public class SessionData {
    public UserData UserData;
    public string RITchCode;
    public string AppVersion;
    public string SessionID;
    public string StartTimeUTC;
    public float TotalTimeSeconds;

    public ValueChangeEvent OnTotalTimeSecondsChange;

    public float TotalTimeSecondsValue {
        get => TotalTimeSeconds;
        set {
            TotalTimeSeconds = value;
            OnTotalTimeSecondsChange?.Invoke( );
        }
    }

    public SessionData(string ritchCode) {
        UserData = null;
        RITchCode = ritchCode;
        AppVersion = Application.version;
        SessionID = Guid.NewGuid( ).ToString( );
        StartTimeUTC = DateTime.UtcNow.ToString("o");
        TotalTimeSeconds = 0;
    }

    public virtual void ClearAllDelegates( ) {
        OnTotalTimeSecondsChange = null;
    }

    public virtual void InvokeAllDelegates( ) {
        OnTotalTimeSecondsChange?.Invoke( );
    }

    public override string ToString( ) {
        return "Session";
    }
}