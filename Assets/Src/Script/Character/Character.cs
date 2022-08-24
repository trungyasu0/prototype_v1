using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using Photon.Pun;

public class Character : MonoBehaviour
{
    public Transform lockOnTransform;
    public float health = 100;
    public int id;
    public State state;
    public GameObject cameraHolder;
    
    public enum State
    {
        None,
        Locomotion,
        GetHit,
        Rolling,
        LightAttacking,
        HeavyAttacking,
        HoldingForHeavyAttack,
        Dead
    }

    private BaseMeleeWeapon _baseMeleeWeapon;
    private Animator _animator;
    private PlayerController _playerController;

    private List<int> _moveSetLightAttack;
    private int _currentIndexLightAttack;

    private int _animSpeed;
    private int _animRoll;
    private int _animDead;
    private int _animGetHit;
    private int _animHoldHeavyAttack;
    private int _animHeavyAttack;

    public CharacterStat characterStat;
    public PhotonView photonView;


    public State nextState;
    public float timeResetQueueAction;
    
    private void Awake()
    {
        state = State.Locomotion;
        _animator ??= GetComponent<Animator>();
        _playerController ??= GetComponent<PlayerController>();
        _baseMeleeWeapon ??= GetComponentInChildren<BaseMeleeWeapon>();
        photonView ??= GetComponent<PhotonView>();

        if (!photonView.IsMine)
        {
            _playerController.enabled = false;
            cameraHolder.SetActive(false);
        }
        else
        {
            _playerController.enabled = true;
            cameraHolder.SetActive(true);
        }
        
        
        _currentIndexLightAttack = 0;
        if (_baseMeleeWeapon) _baseMeleeWeapon.SetOwner(this);
        InitAnimation();
        
        characterStat.Init();
    }
    
    private void InitAnimation()
    {
        _moveSetLightAttack = new List<int>
        {
            Animator.StringToHash("LightAttack1"),
            Animator.StringToHash("LightAttack2"),
            Animator.StringToHash("LightAttack3")
        };

        _animSpeed = Animator.StringToHash("Speed");
        _animRoll = Animator.StringToHash("RollForward");
        _animDead = Animator.StringToHash("Dead");
        _animGetHit = Animator.StringToHash("GetHit");
        _animHoldHeavyAttack = Animator.StringToHash("Hold");
        _animHeavyAttack = Animator.StringToHash("HeavyAttack1");
    }
    public void AnimRoll()
    {
        StartCoroutine(ResetQueueAction());
        _animator.SetTrigger(_animRoll);
    }
    public void AnimMoveSetLightAttack()
    {
        StartCoroutine(ResetQueueAction());
        _animator.SetTrigger(_moveSetLightAttack[_currentIndexLightAttack]);
        if (_currentIndexLightAttack < _moveSetLightAttack.Count - 1)
            _currentIndexLightAttack++;
        else _currentIndexLightAttack = 0;
    }

    public void AnimHoldHeavyAttack()
    {
        state = State.HoldingForHeavyAttack;
        _animator.SetBool(_animHoldHeavyAttack, true);
    }
    public void AnimHeavyAttack()
    {
        _animator.SetBool(_animHoldHeavyAttack, false);
        _animator.SetTrigger(_animHeavyAttack);
        state = State.HeavyAttacking;
    }
    public void AnimMove(float speed)
    {
        _animator.SetFloat(_animSpeed, speed);
    }
    public void SetHoldAnim(bool active)
    {
        _animator.SetBool(_animHoldHeavyAttack, active);
    }

    public void OnGetHit()
    {
        _animator.SetTrigger(_animGetHit);
        state = State.GetHit;
        
    }

    private IEnumerator ResetQueueAction()
    {
        yield return new WaitForSeconds(timeResetQueueAction);
        nextState = State.None;
    }

    public void BeingAttack(AttackerPack attackerPack)
    {
        if(state == State.Rolling) return;
        characterStat.heal -= attackerPack.damage;
        characterStat.poise -= attackerPack.poiseDamage;

        if (characterStat.poise <= 0)
        {
            OnGetHit();
        }
        
    }
    
    //function call by frame of anim action
    private void OnDead()
    {
        state = State.Dead;
        _animator.SetTrigger(_animDead);
    }
    private void GetHitDone()
    {
        if (characterStat.heal <= 0)
        {
            OnDead();
            return;
        }

        state = State.Locomotion;
        characterStat.RefillPoise();
    }
    private void EnableAttackHitBox()
    {
        _baseMeleeWeapon.OnEnableHitBox();
    }
    private void DisableAttackHitBox()
    {
        _baseMeleeWeapon.OnDisableHitBox();
    }
    private void OnAttackFinish()
    {        
        state = State.Locomotion;
        OnEndState();
    }
    private void OnRollFinish()
    {
        OnEndState();
    }
    private void OnEndState()
    {
        switch (nextState)
        {
            case State.None:
                break;
            case State.LightAttacking:
                _playerController.OnLightAttack();
                break;
            case State.Rolling:
                _playerController.OnNextRoll();
                break;
        }

        nextState = State.None;
    }



}

public class CharacterStat
{
    public float heal;
    public float poise;
    public float stamina;

    public float maxHeal;
    public float maxPoise;
    public float maxStamina;

    public void RefillPoise()
    {
        poise = maxPoise;
    }

    public void Init()
    {
        heal = maxHeal;
        poise = maxPoise;
        stamina = maxStamina;
    }
}