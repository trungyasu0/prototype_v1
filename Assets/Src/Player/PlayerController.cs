using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Vector2 _beganTouchPosition, _endTouchPosition, _currentTouchPosition;
    private float _beganTouchTime, _endTouchTime;
    private Touch _touch;

    private CharacterController _controller;
    private float _velocityX;
    private float _velocityZ;
    private float _velocity;
    private const float MaxVelocity = 1f;

    public float MiniTimeRoll = 0.5f;
    public float MiniTimeAttack = 0.2f;

    private enum State
    {
        Rolling,
        Attacking,
        Any
    }

    private State _playerState;
    private Animator _animator;
    private float _distanceToDes;
    private Vector2 _rollDir;

    private float _lastInputHorizontal;
    private float _lastInputVertical;

    public float turnSmoothTime = 0.1f;
    public float turnSmoothVelocity;
    public Transform cam;
    public Transform enemyTarget;
    public float speed = 2.0f;
    public float rollSpeed = 3f;
    public float acceleration = 50f;
    public float deceleration = 400;
    public float rollDistance = 2.5f;


    public enum AttackType
    {
        PunchLeft,
        PunchRight,
        HeavyAttack
    }

    private AttackType _attackType = AttackType.PunchLeft;


    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _playerState = State.Any;
    }

    private void LateUpdate()
    {
        HandleRotationCam();
    }

    private void HandleRotationCam()
    {
        Vector3 dir = enemyTarget.position - transform.position;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    private void Update()
    {
        CheckMoveOnSwipe();
        if (_playerState == State.Rolling) Roll();
    }


    private void Move(float horizontal, float vertical)
    {
        Debug.Log("dir " + horizontal + " " + vertical);
        AniMove(horizontal, vertical);
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        if (direction.magnitude >= 0.1f)
        {
            Vector3 moveDir = CalculatorDirMove(horizontal, vertical);
            if (_velocity < speed) _velocity += acceleration * Time.deltaTime;
            _controller.Move(moveDir * _velocity * Time.deltaTime);
        }
        else
        {
            if (_velocity > 0) _velocity -= deceleration * 10 * Time.deltaTime;
        }
    }

    private void AniMove(float horizontal, float vertical)
    {
        if (horizontal != 0)
        {
            if (Mathf.Abs(_velocityX) <= MaxVelocity || horizontal != _lastInputHorizontal)
            {
                _velocityX += acceleration / 5 * Time.deltaTime * horizontal;
            }
        }
        else
        {
            if (_velocityX > 0) _velocityX -= deceleration / 10 * Time.deltaTime;
            if (_velocityX < 0) _velocityX += deceleration / 10 * Time.deltaTime;
        }

        if (vertical != 0)
        {
            if (Mathf.Abs(_velocityZ) < MaxVelocity || vertical != _lastInputVertical)
                _velocityZ += acceleration * 5 * Time.deltaTime * vertical;
        }
        else
        {
            if (_velocityZ > 0) _velocityZ -= deceleration / 10 * Time.deltaTime;
            if (_velocityZ < 0) _velocityZ += deceleration / 10 * Time.deltaTime;
        }

        Debug.Log("vertical" + _velocityZ + " Horizontal " + _velocityX);
        _animator.SetFloat("Vertical", _velocityZ);
        _animator.SetFloat("Horizontal", _velocityX);

        _lastInputHorizontal = horizontal;
        _lastInputVertical = vertical;
    }

    private void OnRoll(float horizontal, float vertical)
    {
        _velocity = 0;
        _distanceToDes = rollDistance;
        AniRolling(horizontal, vertical);
        _rollDir = new Vector2(horizontal, vertical);
        _playerState = State.Rolling;
    }

    private void Roll()
    {
        float horizontal = _rollDir.x;
        float vertical = _rollDir.y;
        if (horizontal == 0 && vertical == 0) vertical = -1;
        Vector3 moveDir = CalculatorDirMove(horizontal, vertical);
        if (_velocity < rollSpeed) _velocity += acceleration * Time.deltaTime;
        float moveAmount = (moveDir * _velocity * Time.deltaTime).magnitude;

        if (_distanceToDes > 0)
        {
            _distanceToDes -= moveAmount;
            _controller.Move(moveDir * _velocity * Time.deltaTime);
        }
        else
        {
            if (_velocity > 0) _velocity = 0;

            _playerState = State.Any;
        }
    }

    private void AniRolling(float horizontal, float vertical)
    {
        if (vertical > 0) _animator.SetTrigger("RollForward");
        else if (vertical < 0) _animator.SetTrigger("RollBackward");
        else if (horizontal > 0) _animator.SetTrigger("RollRight");
        else if (horizontal < 0) _animator.SetTrigger("RollLeft");
        else if (horizontal == 0 && vertical == 0) _animator.SetTrigger("RollBackward");

        // StartCoroutine(OnCompleteRollAction());
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

    private void OnAttack()
    {
        if (_playerState == State.Any && _playerState != State.Attacking)
        {
            _playerState = State.Attacking;
            AniAttack();
        }
    }

    private void AniAttack()
    {
        if (_attackType == AttackType.PunchLeft)
        {
            _animator.SetTrigger("PunchLeft");
            _attackType = AttackType.PunchRight;
        }
        else if (_attackType == AttackType.PunchRight)
        {
            _animator.SetTrigger("PunchRight");
            _attackType = AttackType.PunchLeft;
        }

        _playerState = State.Any;
    }


    private void CheckMoveOnSwipe()
    {
        if (Input.touchCount == 0) return;
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

                var distanceFromBeganTouch = Vector2.Distance(_endTouchPosition, _beganTouchPosition);
                _animator.SetFloat("Vertical", 0);
                _animator.SetFloat("Horizontal", 0);

                var offsetTime = _endTouchTime - _beganTouchTime;
                Debug.Log("offset time " + offsetTime);

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
}