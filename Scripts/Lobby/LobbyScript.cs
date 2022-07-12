//========================================
//  Author: Peter Phillips                
//  File:   LobbyScript.cs                
//  Date Created:   06.10.2021            
//  Last Modified:  12.11.2021            
//  Brief:  Script that controls the UI in
//          the lobby when players join,
//          leave, and ready up.          
//========================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class LobbyScript : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMesh[] playerNames;
    [SerializeField] LobbySprite[] playerObjects;
    [SerializeField] GameObject[] readyLights;
    [SerializeField] private GameObject readyButton;
    [SerializeField] private StarController starController;
    [SerializeField] private JitterAnimation shipJitter;

    private int maxPlayers = 5;

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined lobby");
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    
        if (PhotonNetwork.IsMasterClient)
        {
            playerObjects[0].photonView.TransferOwnership(1);
            playerObjects[0].GetComponent<LobbySprite>().photonView.RPC("Initialise", RpcTarget.AllBuffered, 1);

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("isMaster");

                readyButton.SetActive(true);
            }

            for (int i = 1; i < playerObjects.Length; i++)
                playerObjects[i].photonView.TransferOwnership(-1);
        }
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        Debug.Log(player.NickName + " has joined the lobby with ActorNumber: " + player.ActorNumber);

        if (PhotonNetwork.IsMasterClient)
        {
            // Transfer ownership of a player object to the newly joined player.
            playerObjects[player.ActorNumber - 1].photonView.TransferOwnership(player.ActorNumber);
            // Initialise the player object.
            playerObjects[player.ActorNumber - 1].GetComponent<LobbySprite>().photonView.RPC("Initialise", RpcTarget.AllBuffered, player.ActorNumber);
            // Collect angular data and send it to the newly joined player so that the character positions and rotations are synced.
            Vector3[] tetherAngles = new Vector3[playerObjects.Length];
            Vector3[] playerAngles = new Vector3[playerObjects.Length];
            for (int i = 0; i < playerObjects.Length; i++)
            {
                tetherAngles[i] = playerObjects[i].tether.eulerAngles;
                playerAngles[i] = playerObjects[i].playerColour.eulerAngles;
            }
            photonView.RPC("ReceivePositionData", player, tetherAngles, playerAngles);
            // Stop master client from seizing ownership of all unowned networked objects.
            for (int i = 1; i < playerObjects.Length; i++)
                if (playerObjects[i].photonView.IsMine && i != player.ActorNumber - 1)
                    playerObjects[i].photonView.TransferOwnership(-1);
        }
    }

    // Called when a player leaves the room.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " disconnected.");
        // Make the player "disappear".
        playerObjects[otherPlayer.ActorNumber - 1].GetComponent<LobbySprite>().ResetTether();
        // Stop the master client from taking ownership.
        if (PhotonNetwork.IsMasterClient)
        {
            playerObjects[otherPlayer.ActorNumber - 1].photonView.TransferOwnership(-1);
            //PhotonNetwork.CloseConnection(otherPlayer);
        }
    }

    // Updates the lobby screen UI.
    [PunRPC]
    void UpdateLobbyUI()
    {
        // Set the player name texts.
        for (int i = 0; i < maxPlayers; i++)
        {
            if (PhotonNetwork.CurrentRoom.GetPlayer(i + 1) != null)
            {
                playerNames[i].text = PhotonNetwork.CurrentRoom.GetPlayer(i + 1).NickName;
            }
            else
            {
                playerNames[i].text = "...";
            }
        }
        Debug.Log("num of players: " + PhotonNetwork.PlayerList.Length);
    }

    [PunRPC]
    private void ReceivePositionData(Vector3[] tetherAngles, Vector3[] playerAngles)
    {
        for (int i = 0; i < playerObjects.Length; i++)
        {
            playerObjects[i].tether.eulerAngles = tetherAngles[i];
            playerObjects[i].playerColour.eulerAngles = playerObjects[i].playerOutline.eulerAngles = playerAngles[i];
        }
    }

    public int CheckReady()
    {
        int count = 0;

        foreach (GameObject go in readyLights)
            if (go.activeSelf)
                count++;

        return count;
    }

    [PunRPC]
    public void StartGame()
    {
        foreach (LobbySprite ls in playerObjects)
        {
            ls.SetNotInLobby();
        }

        PhotonNetwork.LoadLevel("Game");
    }

    public void OnPressedReadyButton()
    {
        if (PhotonNetwork.IsMasterClient)
            StartGame();
    }

    public void OnPressedBackButton()
    {
        //if (PhotonNetwork.IsConnected)
        //    PhotonNetwork.Disconnect();
        Destroy(GameObject.Find("NetworkManager"));
        SceneManager.LoadScene("Menu");
        if (PhotonNetwork.CurrentRoom != null)
            PhotonNetwork.LeaveRoom(false);
        //PhotonNetwork.CloseConnection(PhotonNetwork.LocalPlayer);
        //Destroy(GameObject.Find("PhotonMono"));
        //GameObject.Find("PhotonMono").GetComponent<PhotonHandler>().ApplyDontDestroyOnLoad = false;
    }

    public void UpdateIntensity()
    {
        starController.SetStarSpeed(5 + 5 * CheckReady());
        shipJitter.SetJitterSize(.025f + .025f * CheckReady());
    }
}
