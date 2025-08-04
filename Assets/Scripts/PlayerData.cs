using System;
using System.Collections.Generic;

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
	public float PlaytimeSeconds;
	public int TotalPoints;
	public List<GameSessionData> GameSessionData;
    public List<CheckInSessionData> CheckInSessionData;

    public ValueChangeEvent OnTotalPointsChange;
    public ValueChangeEvent OnPlaytimeSecondsChange;

    public AppSessionData(string appVersion) {
        AppVersion = appVersion;
        SessionID = Guid.NewGuid( ).ToString();
        StartTimeUTC = DateTime.UtcNow.ToString("o");
        SetTotalPoints(0);
        SetPlaytimeSeconds(0);

        GameSessionData = new List<GameSessionData>( );
        CheckInSessionData = new List<CheckInSessionData>( );
    }

    /// <summary>
    /// Set the total points of this app session
    /// </summary>
    /// <param name="totalPoints">The current total points value</param>
    public void SetTotalPoints(int totalPoints)
    {
		TotalPoints = totalPoints;
		OnTotalPointsChange?.Invoke( );
	}

    /// <summary>
    /// Add a certain number of points to the total points value
    /// </summary>
    /// <param name="points">The number of points to add</param>
    public void AddToTotalPoints(int points)
    {
        TotalPoints += points;
        OnTotalPointsChange?.Invoke( );
    }

    /// <summary>
    /// Set the playtime seconds of this app session
    /// </summary>
    /// <param name="playtimeSeconds">The current playtime seconds value</param>
    public void SetPlaytimeSeconds (float playtimeSeconds)
    {
		PlaytimeSeconds = playtimeSeconds;
		OnPlaytimeSecondsChange?.Invoke( );
	}

    /// <summary>
    /// Add a certain number of seconds to the total playtime seconds value
    /// </summary>
    /// <param name="seconds">The number of seconds to add</param>
	public void AddToPlaytimeSeconds (float seconds)
	{
		PlaytimeSeconds += seconds;
		OnPlaytimeSecondsChange?.Invoke( );
	}
}

[Serializable]
public class CheckInSessionData {
    public string SessionID;
    public string StartTimeUTC;
	public float PlaytimeSeconds;
	public int Intensity;
	public List<string> Triggers;

	public ValueChangeEvent OnPlaytimeSecondsChange;

    public CheckInSessionData( ) {
        SessionID = Guid.NewGuid( ).ToString( );
		StartTimeUTC = DateTime.UtcNow.ToString("o");
        Intensity = -1;
        Triggers = new List<string>( );
        SetPlaytimeSeconds(0);
    }

	/// <summary>
	/// Set the playtime seconds of this check in session
	/// </summary>
	/// <param name="playtimeSeconds">The current playtime seconds value</param>
	public void SetPlaytimeSeconds (float playtimeSeconds)
	{
		PlaytimeSeconds = playtimeSeconds;
		OnPlaytimeSecondsChange?.Invoke( );
	}

	/// <summary>
	/// Add a certain number of seconds to the total playtime seconds value
	/// </summary>
	/// <param name="seconds">The number of seconds to add</param>
	public void AddToPlaytimeSeconds (float seconds)
	{
		PlaytimeSeconds += seconds;
		OnPlaytimeSecondsChange?.Invoke( );
	}
}

[Serializable]
public class GameSessionData {
    public string SessionID;
    public string Name;
	public string StartTimeUTC;
	public float PlaytimeSeconds;
	public int Points;

    public ValueChangeEvent OnPointsChange;
    public ValueChangeEvent OnPlaytimeSecondsChange;

    public GameSessionData( ) {
        SessionID = Guid.NewGuid().ToString();
        StartTimeUTC = DateTime.UtcNow.ToString("o");
        Name = "";
		SetPoints(0);
        SetPlaytimeSeconds(0);
	}

	/// <summary>
	/// Set the total points of this game session
	/// </summary>
	/// <param name="points">The current points value</param>
	public void SetPoints (int points)
	{
		Points = points;
		OnPointsChange?.Invoke( );
	}

	/// <summary>
	/// Add a certain number of points to the points value
	/// </summary>
	/// <param name="points">The number of points to add</param>
	public void AddToTotalPoints (int points)
	{
		Points += points;
		OnPointsChange?.Invoke( );
	}

	/// <summary>
	/// Set the playtime seconds of this game session
	/// </summary>
	/// <param name="playtimeSeconds">The current playtime seconds value</param>
	public void SetPlaytimeSeconds (float playtimeSeconds)
	{
		PlaytimeSeconds = playtimeSeconds;
		OnPlaytimeSecondsChange?.Invoke( );
	}

	/// <summary>
	/// Add a certain number of seconds to the total playtime seconds value
	/// </summary>
	/// <param name="seconds">The number of seconds to add</param>
	public void AddToPlaytimeSeconds (float seconds)
	{
		PlaytimeSeconds += seconds;
		OnPlaytimeSecondsChange?.Invoke( );
	}
}