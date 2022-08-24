using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AttackerPack
{
    public float damage;
    public float poiseDamage;

    public AttackerPack(float damage, float poiseDamage)
    {
        this.damage = damage;
        this.poiseDamage = poiseDamage;
    }
}
