//==============================================//
//  Author: Peter Phillips                      //
//  File:   NetworkManager.cs                   //
//  Date Created:   06.10.2021                  //
//  Last Modified:  24.10.2021                  //
//  Brief:  Script that sets up a network       //
//          bewteen the players and the server  //
//          and defines some network messages   //
//==============================================//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPun
{
    public static NetworkManager instance;

    private int maxPlayers = 5;

    void Awake()
    {
        // Set the instance to this script.
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Connect to the master server.
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Joins a random room or creates a new room.
    public void CreateOrJoinRoom()
    {
        // If there is an available room, join it.
        if (PhotonNetwork.CountOfRooms > 0)
            PhotonNetwork.JoinRandomRoom();
        // Otherwise, create a new room.
        else
        {
            // Set the max players to 5.
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = (byte)maxPlayers;

            PhotonNetwork.CreateRoom(null, options);
        }
    }

    // Changes the scene using Photon's system.
    [PunRPC]
    public void ChangeScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }
}