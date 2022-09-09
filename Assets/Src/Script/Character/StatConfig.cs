using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CharacterStat", menuName = "Data/Character/StatConfigCell")]
public class StatConfig : ScriptableObject
{
    public float maxHeal;
    public float maxPoise;
    public float maxStamina;
    public float defensiveResist;
}
