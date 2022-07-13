using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    private Transform _cameraObject;
    private Rigidbody _rigidbody;
    private void Start()
    {
        _cameraObject = Camera.main.transform;
        _rigidbody = GetComponent<Rigidbody>();
        

    }
}
