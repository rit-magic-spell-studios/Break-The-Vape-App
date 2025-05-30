using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CraveSmashManager : MonoBehaviour {
	[SerializeField] private GameObject formUIContainer;
	[SerializeField] private GameObject gameUIContainer;
	[SerializeField] private GameObject endUIContainer;
	[SerializeField] private CraveMonster craveMonster;
	[SerializeField] private Slider craveSlider;

	/// <summary>
	/// The current game state
	/// </summary>
	public CraveSmashGameState CraveSmashGameState {
		get => _craveSmashGameState;
		set {
			_craveSmashGameState = value;

			// Update the UI based on the current game state
			formUIContainer.SetActive(_craveSmashGameState == CraveSmashGameState.FORM);
			gameUIContainer.SetActive(_craveSmashGameState == CraveSmashGameState.GAME);
			endUIContainer.SetActive(_craveSmashGameState == CraveSmashGameState.END);

			switch (_craveSmashGameState) {
				case CraveSmashGameState.GAME:
					// Set the size of the monster
					craveMonster.MonsterSize = (((craveSlider.value - 1) / 9f) * craveMonster.MaxMonsterSize) + craveMonster.MinMonsterSize;

					// Make sure the health is at maximum when starting the game
					craveMonster.HealthPercentage = 1;

					break;
			}
		}
	}
	private CraveSmashGameState _craveSmashGameState;

	private void Start ( ) {
		// At the start of the game, make sure the player does the form first
		CraveSmashGameState = CraveSmashGameState.FORM;
	}

	/// <summary>
	/// Set the game state based on an index
	/// </summary>
	/// <param name="craveSmashGameStateIndex">The index of the new game state to set</param>
	public void SetCraveSmashGameState (int craveSmashGameStateIndex) {
		CraveSmashGameState = (CraveSmashGameState) craveSmashGameStateIndex;
	}
}
