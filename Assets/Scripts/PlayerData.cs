using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "PlayerData")]
public class PlayerData : ScriptableObject {
    /// <summary>
    /// The total amount of points that the player has for the current playsession
    /// </summary>
    public int TotalPoints;

    /// <summary>
    /// Whether or not the player has completed the initial check in for the app
    /// </summary>
    public bool HasCompletedInitialCheckIn;

    /// <summary>
    /// Whether or not the player has vaped recently (from check in form)
    /// </summary>
    public bool HasVapedRecently;

    /// <summary>
    /// The causes for the player's craving
    /// </summary>
    public List<string> CravingCauses;

    /// <summary>
    /// The player's current craving level at the start of the playsession
    /// </summary>
    public int InitialCravingLevel;

    /// <summary>
    /// The player's current craving level at the end of the playsession
    /// </summary>
    public int FinalCravingLevel;

    private void OnEnable( ) {
        TotalPoints = 0;
        HasCompletedInitialCheckIn = false;
        HasVapedRecently = false;
        CravingCauses = new List<string>();
        InitialCravingLevel = 0;
        FinalCravingLevel = 0;
    }
}
