//==============================================//
//  Author: Peter Phillips                      //
//  File:   MenuScript.cs                       //
//  Date Created:   06.10.2021                  //
//  Last Modified:  06.10.2021                  //
//  Brief:  Script that controls UI interaction //
//          in the main menu                    //
//==============================================//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class MenuScript : MonoBehaviourPunCallbacks
{
    [SerializeField] private InputField playerNameInput;

    // Called when we press the Play button.
    public void OnPressPlay()
    {
        PhotonNetwork.NickName = (playerNameInput.text != "") ? playerNameInput.text : "Player";
        Debug.Log("Pressed Play");
        NetworkManager.instance.CreateOrJoinRoom();
        SceneManager.LoadScene("Lobby");
    }
}
