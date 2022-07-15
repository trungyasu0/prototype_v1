using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    public Transform targetTransform;
    public Transform cameraTransform;
    public Transform cameraPivotTransform;

    public static CameraHandler Instance;

    private Vector3 _velocity;
    
    public float followSpeed = 0.1f;
    public float smoothTimeMove = 0.2f;
    public Transform currentTarget;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        var enemy = FindEnemy();
        currentTarget = enemy.lockOnTransform;
    }

    private void Update()
    {
        if (targetTransform)
        {
            FollowTarget(Time.fixedTime);
            LockOnTarget();
        }
    }

    private void FollowTarget(float delta)
    {
        // Vector3 targetPosition = Vector3.Lerp(transform.position, targetTransform.position, delta/followSpeed);
        Vector3 smoothPos = Vector3.SmoothDamp(transform.position, targetTransform.position, ref _velocity ,smoothTimeMove);
     
        transform.position = smoothPos;
    }

    private void LockOnTarget()
    {
        if (!currentTarget) return;
        var dir = currentTarget.position - transform.position;
        dir.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = targetRotation;
    }

    private Character FindEnemy()
    {
        var colliders = Physics.OverlapSphere(transform.position, 50);
        foreach (var collider in colliders)
        {
            var character = collider.GetComponent<Character>();
            if (character && character != PlayerController.Instance.GetMainPlayer())
            {
                Debug.Log("enemy: " + character);
                return character;
            }
        }
        return null;
    }
    
    
    
    
}