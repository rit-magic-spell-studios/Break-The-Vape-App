using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderText : MonoBehaviour {
	[SerializeField] private Slider slider;
	[SerializeField] private TextMeshProUGUI text;

	private void OnValidate ( ) {
		text = GetComponent<TextMeshProUGUI>( );

		UpdateText( );
	}

	private void Awake ( ) {
		OnValidate( );
	}

	private void Start ( ) {
		// When the slider value is changed, then update the text to show the value
		slider.onValueChanged.AddListener(delegate { UpdateText( ); });
	}

	/// <summary>
	/// Update the text to display the slider's value
	/// </summary>
	private void UpdateText ( ) {
		text.text = $"{slider.value}";
	}
}
