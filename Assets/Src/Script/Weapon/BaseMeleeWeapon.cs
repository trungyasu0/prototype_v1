using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BaseMeleeWeapon : MonoBehaviour
{
    public enum WeaponState
    {
        Enable,
        Disable
    }
    public WeaponState state;
    
    private Character _owner;

    public float lightAttackDamage;
    public float heavyAttackDamage;

    public float heavyPoiseDamage;
    public float lightPoiseDamage;


    private void Awake()
    {
        state = WeaponState.Disable;

    }
    private void OnTriggerEnter(Collider other)
    {
        var character = other.GetComponent<Character>();
        if (character == null || character == _owner || state != WeaponState.Enable) return;
        var attackerPack = _owner.state switch
        {
            Character.State.HeavyAttacking => new AttackerPack(heavyAttackDamage, heavyPoiseDamage),
            Character.State.LightAttacking => new AttackerPack(heavyAttackDamage, lightPoiseDamage),
            _ => new AttackerPack()
        };

        character.BeingAttack(attackerPack);
    }
    public void OnEnableHitBox()
    {
        state = WeaponState.Enable;
    }
    public void OnDisableHitBox()
    {
        state = WeaponState.Disable;
    }
    public void SetOwner(Character owner)
    {
        _owner = owner;
    }
}