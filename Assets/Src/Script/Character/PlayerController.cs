using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    private Vector2 _beganTouchPosition, _endTouchPosition, _currentTouchPosition;
    private float _beganTouchTime, _endTouchTime;
    private Touch _touch;

    private CharacterController _controller;
    private float _velocity;
    private const float MaxVelocity = 1f;

    public float MiniTimeRoll = 0.5f;
    public float MiniTimeAttack = 0.2f;

    private Animator _animator;
    private float _distanceToDes;
    private Vector2 _rollDir;
    

    public float turnSmoothTime = 0.1f;
    public float turnSmoothVelocity;
    public Transform cam;
    public float speed = 2.0f;
    public float rollSpeed = 3f;
    public float acceleration = 50f;
    public float deceleration = 400;
    public float rollDistance = 2.5f;

    private BoxCollider _aoEAttacking;

    private Character _character;
    
    public enum AttackType
    {
        PunchLeft,
        PunchRight,
        HeavyAttack
    }

    private AttackType _attackType = AttackType.PunchLeft;


    private void Awake()
    {
        _controller ??= GetComponent<CharacterController>();
        _animator ??= GetComponent<Animator>();
        _aoEAttacking ??= GetComponent<BoxCollider>();
        _character ??= GetComponent<Character>();
        
        Instance = this;
    }

    private void Update()
    {
        OnSwipe();
        if (_character.state == Character.State.Rolling) Roll();
    }

    
    #region Movement

    private void Move(float horizontal = 0f, float vertical = 0f)
    {
        if(_character.state != Character.State.Locomotion) return;
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        if (direction.magnitude >= 0.1f)
        {
            Vector3 moveDir = CalculatorDirMove(horizontal, vertical);
            if (_velocity < speed) _velocity += acceleration * Time.deltaTime;
            _controller.Move(moveDir * _velocity * Time.deltaTime);
        }
        else
        {
            if (_velocity > 0) _velocity -= deceleration * Time.deltaTime;
        }
        AniMove();
    }

    private void AniMove()
    {
        _animator.SetFloat("Speed", _velocity);
    }

    private void OnRoll(float horizontal, float vertical)
    {
        if(horizontal == 0 && vertical == 0) return;
        if (_character.state == Character.State.Locomotion)
        {
            _character.state = Character.State.Rolling;
            _velocity = 0;
            AniRolling();
            _rollDir = new Vector2(horizontal, vertical);
            _distanceToDes = rollDistance;
        }
    }

    private void Roll()
    {
        float horizontal = _rollDir.x;
        float vertical = _rollDir.y;
        Vector3 moveDir = CalculatorDirMove(horizontal, vertical);
        if (_velocity < rollSpeed)
        {
            _velocity += acceleration * Time.deltaTime;
        }
        float moveAmount = (moveDir * _velocity * Time.deltaTime).magnitude;
        if (_distanceToDes > 0)
        {
            _distanceToDes -= moveAmount;
            _controller.Move(moveDir * _velocity * Time.deltaTime);
        }
        else
        {
            if (_velocity > 0) _velocity = 0;
            _character.state = Character.State.Locomotion;
        }
    }

    private void AniRolling()
    {
        _animator.SetTrigger("RollForward");
    }

    private Vector3 CalculatorDirMove(float horizontal, float vertical)
    {
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity,
                turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            return moveDir.normalized;
        }

        return new Vector3(0, 0, 0);
    }

    #endregion


    private void OnAttack()
    {
        if (_character.state == Character.State.Locomotion && _character.state != Character.State.Attacking)
        {
            _character.state = Character.State.Attacking;

            var enemy = CameraHandler.Instance.currentTarget;
            var dir = enemy.position - transform.position;
            dir.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = targetRotation;
            AniAttack();

        }
    }

    private void AniAttack()
    {
        if (_attackType == AttackType.PunchLeft)
        {
            _animator.SetTrigger("Attack1");
            _attackType = AttackType.PunchRight;
        }
        else if (_attackType == AttackType.PunchRight)
        {
            _animator.SetTrigger("Attack2");
            _attackType = AttackType.PunchLeft;
        }

    }
    
    private void OnSwipe()
    {
        if (Input.touchCount == 0)
        {
            if (_character.state == Character.State.Locomotion) Move();
            return;
        }
        if (Input.touchCount > 0)
        {
            _touch = Input.GetTouch(0);
        }
        switch (_touch.phase)
        {
            case TouchPhase.Began:
                _beganTouchTime = Time.time;
                _beganTouchPosition = _touch.position;
                break;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                _currentTouchPosition = _touch.position;
                var dir = GetDirOfTouchAction(_beganTouchPosition, _currentTouchPosition);
                Move(dir.x, dir.y);
                break;
            case TouchPhase.Ended:
                _endTouchTime = Time.time;
                _endTouchPosition = _touch.position;
                _velocity = 0;
                var distanceFromBeganTouch = Vector2.Distance(_endTouchPosition, _beganTouchPosition);
                var offsetTime = _endTouchTime - _beganTouchTime;
                if (offsetTime < MiniTimeAttack && distanceFromBeganTouch < 20)
                {
                    OnAttack();
                }
                else if (offsetTime < MiniTimeRoll)
                {
                    var dirRoll = GetDirOfTouchAction(_beganTouchPosition, _endTouchPosition);
                    OnRoll(dirRoll.x, dirRoll.y);
                }
                break;
        }
    }
    private Vector2 GetDirOfTouchAction(Vector2 startPos, Vector2 desPos)
    {
        float vertical;
        float horizontal;
        if (desPos.x - startPos.x > 50) vertical = 1;
        else if (desPos.x - startPos.x < -50) vertical = -1;
        else vertical = 0;

        if (desPos.y - startPos.y > 100) horizontal = 1;
        else if (desPos.y - startPos.y < -100) horizontal = -1;
        else horizontal = 0;


        return new Vector2(vertical, horizontal);
    }

    public Character GetMainPlayer()
    {
        return _character;
    }
}