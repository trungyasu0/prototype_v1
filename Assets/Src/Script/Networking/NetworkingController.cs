using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkingController : MonoBehaviourPunCallbacks
{
    public InputField tfCreate;
    public InputField tfJoin;
    
    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(tfCreate.text);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(tfJoin.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("MainGameScene");
    }
}
