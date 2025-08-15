using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CraveSmashController : GameController {
    /// <summary>
    /// Called when the monster has been destroyed
    /// </summary>
    public void OnMonsterDestroyed( ) {
        SpawnConfettiParticles(transform.position);
        DelayAction(( ) => { DisplayScreen(winScreen); }, WIN_DELAY_SECONDS);
    }
}
