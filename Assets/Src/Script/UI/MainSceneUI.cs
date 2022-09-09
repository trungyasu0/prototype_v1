using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSceneUI : MonoBehaviour
{
    private PlayerController _controller;

    #region Singleton
    public static MainSceneUI Instance { get; private set; } // static singleton
    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }
    #endregion Singleton

    
    public void Init(PlayerController controller)
    {
        if(!controller) return;
        _controller = controller;
    }

    public void OnDef()
    {
        _controller.OnDefensive();
    }

    public void OnCancelDef()
    {
        _controller.OnCancelDefensive();
    }
}
