using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;
    public List<Transform> listSpawnPosition;

    private Room _currentRoom;


    private void Start()
    {
        _currentRoom = PhotonNetwork.CurrentRoom;
        var numPlayer = _currentRoom.Players.Count;
        PhotonNetwork.Instantiate(playerPrefab.name, listSpawnPosition[numPlayer - 1].position, Quaternion.identity);
        
    }
}

