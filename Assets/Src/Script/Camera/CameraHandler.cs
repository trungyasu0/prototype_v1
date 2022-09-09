using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    public Transform targetTransform;
    // public Transform cameraTransform;
    // public Transform cameraPivotTransform;

    public static CameraHandler Instance;

    private Vector3 _velocity;

    public float followSpeed = 0.1f;
    public float smoothTimeMove = 0.2f;
    public Transform currentTarget;

    public Character _character;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        var enemy = FindEnemy();
        if (enemy) currentTarget = enemy.lockOnTransform;
    }

    private void Update()
    {
        if (targetTransform)
        {
            FollowCharacter(Time.fixedTime);
            LockOnTarget();
        }
    }

    private void FollowCharacter(float delta)
    {
        // Vector3 targetPosition = Vector3.Lerp(transform.position, targetTransform.position, delta/followSpeed);
        Vector3 smoothPos =
            Vector3.SmoothDamp(transform.position, targetTransform.position, ref _velocity, smoothTimeMove);

        transform.position = smoothPos;
    }

    private void LockOnTarget()
    {
        if (!currentTarget)
        {
            var enemy = FindEnemy();
            if (enemy) currentTarget = enemy.lockOnTransform;
        }
        else
        {
            var dir = currentTarget.position - transform.position;
            dir.Normalize();
            dir.y *= -1;
            dir.y = Math.Max(-0.2f, dir.y);

            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = targetRotation;
        }
    }

    private Character FindEnemy()
    {
        var colliders = Physics.OverlapSphere(transform.position, 50);
        foreach (var collider in colliders)
        {
            var character = collider.GetComponent<Character>();
            if (!character 
                // || character.photonView == null
               ) continue;
            if (character == _character) continue;
            return character;
        }

        return null;
    }
}