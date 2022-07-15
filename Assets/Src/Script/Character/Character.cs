using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public Transform lockOnTransform;
    public float health = 100;
    private Animator _animator;

    public int id;
    public State state;

    public enum State
    {
        GetHit,
        Rolling,
        Attacking,
        Locomotion,
        Dead
    }

    private WeaponController _weapon;

    private void Awake()
    {
        state = State.Locomotion;
        _animator ??= GetComponent<Animator>();
        _weapon ??= GetComponentInChildren<WeaponController>();

        if (_weapon) _weapon.SetOwner(this);
    }

    public void OnGetHit(int attackDamage)
    {
        if (state != State.GetHit)
        {
            health -= attackDamage;
            _animator.SetTrigger("GetHit");
            state = State.GetHit;
        }

       
    }

    private void OnDead()
    {
        state = State.Dead;
        _animator.SetTrigger("Dead");
    }

    private void GetHitDone()
    {
        if (health <= 0)
        {
            OnDead();
            return;
        }
       
        state = State.Locomotion;
    }

    private void EnableAttackHitBox()
    {
        _weapon.OnEnableHitBox();
    }
    private void DisableAttackHitBox()
    {
        _weapon.OnDisableHitBox();
    }
    private void OnAttackFinish()
    {
        state = State.Locomotion;
        Debug.Log("attack finish");
    }
}