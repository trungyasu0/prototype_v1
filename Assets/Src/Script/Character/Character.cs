using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public Transform lockOnTransform;
    public float health;
    public float damage;
    private Animator _animator;

    public int id;
    public State state;

    public enum State
    {
        GetHit,
        Rolling,
        Attacking,
        Any
    }

    private WeaponController _weapon;
    private void Awake()
    {
        state = State.Any;
        _animator ??= GetComponent<Animator>();
        _weapon ??= GetComponentInChildren<WeaponController>();

        if (_weapon) _weapon.SetOwner(this);
    }

    public void OnGetHit(int attackDamage)
    {
        // if (state != State.GetHit)
        // {
        _animator.SetTrigger("GetHit");
        health -= attackDamage;
        // state = State.GetHit;
        // }
        if(health <= 0) OnDead();
    }
    private void OnDead()
    {
        _animator.SetTrigger("OnDead");
    }

    private void GetHitDone()
    {
        // state = State.Any;
    }
    private void EnableAttackHitBox()
    {
        _weapon.OnEnableHitBox();
    }
    private void DisableAttackHitBox()
    {
        _weapon.OnDisableHitBox();
    }
}