using System;
using System.Collections;
using System.Collections.Generic;
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

    private WeaponController _weapon;
    private Animator _animator;

    private List<int> _moveSetLightAttack;
    private int _currentIndexLightAttack;

    private int _animSpeed;
    private int _animRoll;
    private int _animDead;
    private int _animGetHit;
    private int _holdHeavyAttack;
    private int _heavyAttack;
    
    
    public PhotonView photonView;

    
    private PlayerController _playerController;

    public State nextState;

    public float timeResetQueueAction;
    
    private void Awake()
    {
        state = State.Locomotion;
        _animator ??= GetComponent<Animator>();
        _playerController ??= GetComponent<PlayerController>();
        _weapon ??= GetComponentInChildren<WeaponController>();
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
        if (_weapon) _weapon.SetOwner(this);
        InitAnimation();
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
        _holdHeavyAttack = Animator.StringToHash("Hold");
        _heavyAttack = Animator.StringToHash("HeavyAttack1");
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
        _animator.SetBool(_holdHeavyAttack, true);
    }

    public void AnimHeavyAttack()
    {
        _animator.SetBool(_holdHeavyAttack, false);
        _animator.SetTrigger(_heavyAttack);
        state = State.HeavyAttacking;
    }
    
    public void AnimMove(float speed)
    {
        _animator.SetFloat(_animSpeed, speed);
    }

    public void SetHoldAnim(bool active)
    {
        _animator.SetBool(_holdHeavyAttack, active);
    }

    public void OnGetHit(int attackDamage)
    {
        if (state != State.GetHit)
        {
            health -= attackDamage;
            _animator.SetTrigger(_animGetHit);
            state = State.GetHit;
        }
    }

    private void OnDead()
    {
        state = State.Dead;
        _animator.SetTrigger(_animDead);
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

    private IEnumerator ResetQueueAction()
    {
        yield return new WaitForSeconds(timeResetQueueAction);
        nextState = State.None;
    }


}