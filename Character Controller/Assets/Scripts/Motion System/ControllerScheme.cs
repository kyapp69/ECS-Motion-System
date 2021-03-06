﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "InputData", menuName = "GameParts/InputField", order = 100)]
public class ControllerScheme : ScriptableObject, ButtonConfigs
{
    [SerializeField] KeyCode _jump;
    [SerializeField] KeyCode _lightAttack;
    [SerializeField] KeyCode _heavyAttack;
    [SerializeField] KeyCode _block;
    public KeyCode Jump { get { return _jump; } set { _jump = value; } }
    public KeyCode LightAttack { get { return _lightAttack; } set { _lightAttack = value; } }
    public KeyCode HeavyAttack { get { return _heavyAttack; } set { _heavyAttack = value; } }
    public KeyCode Block { get { return _block; } set { _block = value; } }
}



public enum PlatformOptions
{
    PC, XBOX, PS4, Switch
}
public interface ButtonConfigs
{
    KeyCode Jump { get; set; }
    KeyCode LightAttack { get; set; }
    KeyCode HeavyAttack { get; set; }
    KeyCode Block { get; set; }

}
