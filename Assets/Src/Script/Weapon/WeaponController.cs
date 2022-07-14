using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public enum WeaponState
    {
        Enable,
        Disable
    }

    public WeaponState state;
    private Character _owner;

    private int _attackDamage;

    private void Awake()
    {
        state = WeaponState.Disable;
    }

    private void OnTriggerEnter(Collider other)
    {
        var character = other.GetComponent<Character>();
        if (character != null && character != _owner && state == WeaponState.Enable)
        {
            character.OnGetHit(_attackDamage);
        }
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