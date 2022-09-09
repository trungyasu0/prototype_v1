using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Photon.Pun;
using Photon.Realtime;
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
    private Vector2 _nextRoll;


    public float turnSmoothTime = 0.1f;
    public float turnSmoothVelocity;
    public Transform cam;
    public float speed = 2.0f;
    public float rollSpeed = 3f;
    public float acceleration = 50f;
    public float deceleration = 400;
    public float rollDistance = 2.5f;

    public float maxTimeHeavyAttackHold = 0.5f;

    private BoxCollider _aoEAttacking;
    private Character _character;

    public float miniDistanceTouch = 10;
    public float miniTimeHoldAttack = 0.5f;

    private bool _rotateToEnemy;
    private bool _isMoving;

    private CameraHandler _cameraHandler;


    private void Awake()
    {
        _controller ??= GetComponent<CharacterController>();
        _animator ??= GetComponent<Animator>();
        _aoEAttacking ??= GetComponent<BoxCollider>();
        _character ??= GetComponent<Character>();

        _cameraHandler = _character.cameraHolder.GetComponent<CameraHandler>();

        _rotateToEnemy = false;
        Instance = this;
    }

    private void Start()
    {
    }

    private void Update()
    {
        OnSwipe();
        if (_character.state == Character.State.Rolling) Roll();

        if (_rotateToEnemy)
        {
            var enemyTrans = _cameraHandler.currentTarget;
            if (!enemyTrans) return;
            RotateToEnemy(enemyTrans);
        }
    }

    private void FaceTheEnemy()
    {
        _rotateToEnemy = true;
        StartCoroutine(IEFaceTheEnemy());
    }

    private IEnumerator IEFaceTheEnemy()
    {
        yield return new WaitForSeconds(turnSmoothTime + 0.1f);
        _rotateToEnemy = false;
    }

    private void RotateToEnemy(Transform enemyTrans)
    {
        var dir = enemyTrans.position - transform.position;
        dir.y = 0;
        if (dir.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity,
                turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    private void Move(float horizontal = 0f, float vertical = 0f)
    {
        if (_character.state != Character.State.Locomotion
            && _character.state != Character.State.HoldingForHeavyAttack)
            return;
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        if (direction.magnitude >= 0.1f)
        {
            Vector3 moveDir = CalculatorDirMove(new Vector2(horizontal, vertical));
            if (_velocity < speed) _velocity += acceleration * Time.deltaTime;
            _controller.Move(moveDir * _velocity * Time.deltaTime);
        }
        else
        {
            if (_velocity > 0) _velocity -= deceleration * Time.deltaTime;
        }

        _character.AnimMove(_velocity);
    }

    private void OnRoll(Vector2 dir)
    {
        if (dir.x == 0 && dir.y == 0) return;
        if (_character.state == Character.State.Locomotion)
        {
            _character.state = Character.State.Rolling;
            _velocity = 0;
            _character.AnimRoll();
            _rollDir = dir;
            _distanceToDes = rollDistance;
        }
        else
        {
            _character.nextState = Character.State.Rolling;
            _nextRoll = dir;
        }
    }

    private void Roll()
    {
        Vector3 moveDir = CalculatorDirMove(_rollDir);
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

    public void OnNextRoll()
    {
        OnRoll(_nextRoll);
    }

    private Vector3 CalculatorDirMove(Vector2 dir)
    {
        Vector3 direction = new Vector3(dir.x, 0f, dir.y).normalized;
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

    public void OnLightAttack()
    {
        if (_character.state == Character.State.Locomotion && _character.state != Character.State.LightAttacking)
        {
            FaceTheEnemy();

            _character.state = Character.State.LightAttacking;
            _character.AnimMoveSetLightAttack();
        }
        else
        {
            _character.nextState = Character.State.LightAttacking;
        }
    }

    private void OnHeavyAttack()
    {
        if (_character.state != Character.State.HoldingForHeavyAttack) return;
        FaceTheEnemy();
        _character.AnimHeavyAttack();
    }

    private IEnumerator IEHeavyAttack()
    {
        yield return new WaitForSeconds(maxTimeHeavyAttackHold);
        OnHeavyAttack();
    }

    private void OnHold()
    {
        if (_character.state != Character.State.Locomotion) return;
        _character.AnimHoldHeavyAttack();
        StartCoroutine(IEHeavyAttack());
    }

    public void OnDefensive()
    {
        if (_character.state != Character.State.Locomotion) return;
        _character.state = Character.State.Defensive;
        _rotateToEnemy = true;
        _character.AnimDefensive(true);
    }

    public void OnCancelDefensive()
    {
        _character.state = Character.State.Locomotion;
        _rotateToEnemy = false;
        _character.AnimDefensive(false);
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

        float offsetTime;
        switch (_touch.phase)
        {
            case TouchPhase.Began:
                _beganTouchTime = Time.time;
                _beganTouchPosition = _touch.position;
                break;
            case TouchPhase.Moved:
                _currentTouchPosition = _touch.position;

                if (_character.state != Character.State.HoldingForHeavyAttack)
                {
                    HandleMoveByTouch();
                    _isMoving = true;
                }

                break;

            case TouchPhase.Stationary:
                _currentTouchPosition = _touch.position;
                offsetTime = Time.time - _beganTouchTime;
                if (_isMoving) HandleMoveByTouch();
                else
                {
                    if (_character.state != Character.State.HoldingForHeavyAttack && offsetTime > MiniTimeAttack)
                    {
                        OnHold();
                    }
                }


                break;
            case TouchPhase.Ended:
                _endTouchTime = Time.time;
                _endTouchPosition = _touch.position;
                _velocity = 0;
                _isMoving = false;

                var distanceFromBeganTouch = Vector2.Distance(_endTouchPosition, _beganTouchPosition);
                offsetTime = _endTouchTime - _beganTouchTime;
                //detect light attack
                if (offsetTime < MiniTimeAttack && distanceFromBeganTouch < miniDistanceTouch)
                {
                    OnLightAttack();
                }
                //detect roll
                else if (offsetTime < MiniTimeRoll)
                {
                    var dirRoll = GetDirOfTouchAction(_beganTouchPosition, _endTouchPosition);
                    OnRoll(dirRoll);
                }

                //detect heavy attack
                if (_character.state == Character.State.HoldingForHeavyAttack && offsetTime > miniTimeHoldAttack)
                {
                    OnHeavyAttack();
                }

                break;
        }
    }

    private static Vector2 GetDirOfTouchAction(Vector2 startPos, Vector2 desPos)
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

    private void HandleMoveByTouch()
    {
        var dir = GetDirOfTouchAction(_beganTouchPosition, _currentTouchPosition);
        Move(dir.x, dir.y);
    }
}