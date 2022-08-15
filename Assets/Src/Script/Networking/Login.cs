using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Login : MonoBehaviourPunCallbacks
{
    public Text btnConnectTxt;
    public Text tfUserName;

    private void Start()
    {
        btnConnectTxt.text = "Login";
    }

    public void OnClickConnect()
    {
        if (tfUserName.text.Length > 1)
        {
            PhotonNetwork.NickName = tfUserName.text;
            PhotonNetwork.ConnectUsingSettings();
            btnConnectTxt.text = "Connecting ...";
        }
    }

    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene("JoinRoomScene");
    }
}
