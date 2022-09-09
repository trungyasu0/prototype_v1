using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using Photon.Pun;

public class Character : MonoBehaviour
{
    public Transform lockOnTransform;
    public State state;
    public GameObject cameraHolder;

    [SerializeField]
    public StatConfig configStat;

    private float _health;
    private float _poise;
    private float _stamina;
    private float _defensiveResist;
    
    public enum State
    {
        None,
        Locomotion,
        GetHit,
        Rolling,
        LightAttacking,
        HeavyAttacking,
        HoldingForHeavyAttack,
        Defensive,
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
    private int _animDefensive;
    private int _animKnockBack;

    public PhotonView photonView;

    public State nextState;
    public float timeResetQueueAction;

    private void Awake()
    {
        photonView ??= GetComponent<PhotonView>();
        _playerController ??= GetComponent<PlayerController>();
        _animator ??= GetComponent<Animator>();
        _baseMeleeWeapon ??= GetComponentInChildren<BaseMeleeWeapon>();
        if (_baseMeleeWeapon) _baseMeleeWeapon.SetOwner(this);
        
        state = State.Locomotion;
        InitAnimation();

        _currentIndexLightAttack = 0;

        if (photonView == null || !photonView.IsMine)
        {
            _playerController.enabled = false;
            cameraHolder.SetActive(false);
            return;
        }

        _playerController.enabled = true;
        cameraHolder.SetActive(true);
    }

    private void Start()
    {
        _health = configStat.maxHeal;
        _poise = configStat.maxPoise;
        _stamina = configStat.maxStamina;
        _defensiveResist = configStat.defensiveResist;

        if (photonView != null && photonView.IsMine)
        {
            MainSceneUI.Instance.Init(_playerController);
        }
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
        _animDefensive = Animator.StringToHash("Defensive");
        _animKnockBack = Animator.StringToHash("KnockBack");
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

    public void AnimDefensive(bool active)
    {
        _animator.SetBool(_animDefensive, active);
    }

    public void AnimKnockBack(int type)
    {
        _animator.SetInteger(_animKnockBack, type);
    }

    private void OnGetHit()
    {
        if (state == State.Defensive)
        {
            switch (_poise < configStat.maxPoise / 2)
            {
                case true:
                    //heavy knock back
                    AnimKnockBack(2);
                    break;
                default:
                    //ez knock back
                    AnimKnockBack(1);
                    break;
            }
        }
        else
        {
            _animator.SetTrigger(_animGetHit);
        }

        state = State.GetHit;
    }

    private IEnumerator ResetQueueAction()
    {
        yield return new WaitForSeconds(timeResetQueueAction);
        nextState = State.None;
    }

    public void BeingAttack(AttackerPack attackerPack)
    {
        switch (state)
        {
            case State.Rolling:
            case State.Dead:
            case State.GetHit:
                break;

            case State.Defensive:
                FilterDamageByDefensive(attackerPack);
                _health -= attackerPack.damage;
                _poise -= attackerPack.poiseDamage;
                OnGetHit();

                break;
            default:
                _health -= attackerPack.damage;
                _poise -= attackerPack.poiseDamage;
                OnGetHit();

                break;
        }

        if (!(_health <= 0)) return;
        OnDead();
    }

    private void FilterDamageByDefensive(AttackerPack attackerPack)
    {
        attackerPack.damage *= configStat.defensiveResist / 100;
    }

    //function call by frame of anim action
    private void OnDead()
    {
        state = State.Dead;
        _animator.SetTrigger(_animDead);
    }

    private void GetHitDone()
    {
        

        state = State.Locomotion;
        _poise = configStat.maxPoise;
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

