using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorText : MonoBehaviour {
	[SerializeField, Range(0f, 5f)] private float duration = 2f;

	private float timer = 0f;

	private void Update ( ) {
		// After a certain duration, destroy this indicator text
		timer += Time.deltaTime;
		if (timer >= duration) {
			Destroy(gameObject);
		}
	}
}
