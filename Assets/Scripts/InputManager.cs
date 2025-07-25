using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class InputManager : Singleton<InputManager> {
    [SerializeField] private PlayerInput playerInput;

    protected override void Awake( ) {
        base.Awake( );

        TouchSimulation.Enable( );
        playerInput.SwitchCurrentControlScheme(InputSystem.devices.First(d => d == Touchscreen.current));
    }
}
