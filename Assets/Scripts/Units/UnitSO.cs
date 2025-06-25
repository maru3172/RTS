﻿using UnityEngine;

[CreateAssetMenu (fileName = "Unit", menuName = "Units/Unit")]
public class UnitSO : ScriptableObject
{
    [field: SerializeField] public int Health { get; private set; } = 100;
}