//========================================
//  Author: Peter Phillips                
//  File:   LobbySprite.cs                
//  Date Created:   13.10.2021            
//  Last Modified:  10.11.2021            
//  Brief:  Script that controls the
//          movement of the player sprites
//          in the lobby.
//========================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LobbySprite : MonoBehaviourPun
{
    public Player photonPlayer;     // photon player class

    [SerializeField] public Transform tether;
    [SerializeField] Transform anchorPoint;
    [SerializeField] Transform playerSprite;
    [SerializeField] public Transform playerColour, playerOutline;
    [SerializeField] TextMesh playerName;
    [SerializeField] GameObject readyPlayer, greenLight;
    [SerializeField] private LobbyScript lobbyScript;   // a reference to the lobbyu manager script.
    [SerializeField] private GameObject readyButton;

    private int direction;
    private float speedModififer;
    private int spinDirection;
    private float spinModififer;
    private Coroutine moveTetherOut, moveTetherIn;
    private float tetherLength;
    private bool hasReceivedValues;
    private bool inLobby;

    private void Start()
    {
        inLobby = true;
    }

    void Update()
    {
        if (hasReceivedValues)
        {
            playerSprite.position = anchorPoint.position;

            tether.eulerAngles += Vector3.forward * speedModififer * direction * Time.deltaTime;

            playerColour.eulerAngles = playerOutline.eulerAngles += Vector3.forward * spinModififer * spinDirection * Time.deltaTime;
        }

        if (!photonView.IsMine)
            return;
        
        if (Input.GetMouseButtonDown(0) && inLobby)
        {
            photonView.RPC("MoveTetherIn", RpcTarget.All);
        }
        if (Input.GetMouseButtonUp(0) && inLobby)
        {
            photonView.RPC("MoveTetherOut", RpcTarget.All);
        }
    }

    public void SetNotInLobby()
    {
        inLobby = false;
    }

    [PunRPC]
    void Initialise(int actorNumber)
    {
        playerName.text = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber).NickName;

        if (PhotonNetwork.IsMasterClient)
        {
            SetTetherValues();
            photonView.RPC("GetTetherValues", RpcTarget.OthersBuffered, tetherLength, direction, speedModififer, spinDirection, spinModififer, playerColour.localScale);
            photonView.RPC("MoveTetherOut", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void SetTetherValues()
    {
        tetherLength = Random.Range(3.5f, 4.5f);
        direction = (Random.Range(0f, 1f) < .5f) ? -1 : 1;
        speedModififer = Random.Range(10f, 40f);
        spinDirection = (Random.Range(0f, 1f) < .5f) ? -1 : 1;
        spinModififer = Random.Range(10f, 40f);
        Vector3 flipped = new Vector3(-1, 1, 1);
        playerColour.localScale = playerOutline.localScale = (Random.Range(0f, 1f) < .5f) ? flipped : Vector3.one;
        hasReceivedValues = true;
    }

    [PunRPC]
    private void GetTetherValues(float tLength, int dir, float speedMod, int spinDir, float spinMod, Vector3 localScale)
    {
        tetherLength = tLength;
        direction = dir;
        speedModififer = speedMod;
        spinDirection = spinDir;
        spinModififer = speedMod;
        Vector3 flipped = new Vector3(-1, 1, 1);
        playerColour.localScale = playerOutline.localScale = localScale;
        hasReceivedValues = true;
    }

    [PunRPC]
    private void MoveTetherOut()
    {
        if (moveTetherIn != null) StopCoroutine(moveTetherIn);
        moveTetherOut = StartCoroutine(MoveTetherOutCo());
    }

    [PunRPC]
    private void MoveTetherIn()
    {
        if (moveTetherOut != null) StopCoroutine(moveTetherOut);
        moveTetherIn = StartCoroutine(MoveTetherInCo());
    }
    
    // Coroutine that extends a player's tether.
    public IEnumerator MoveTetherOutCo()
    {
        readyPlayer.SetActive(false);    // set player as unready
        greenLight.SetActive(false);

        lobbyScript.UpdateIntensity();

        Vector3 targetPos = new Vector3(0.1f, tetherLength, 1);

        while (tether.localScale.y < targetPos.y)
        {
            tether.localScale += new Vector3(0, 6f, 0) * Time.deltaTime;
            yield return null;
        }

        tether.localScale = targetPos;  // stop the scale from exceeding the target
    }

    // Coroutine that retracts a player's tether.
    public IEnumerator MoveTetherInCo()
    {
        Vector3 targetPos = new Vector3(0.1f, 0, 1);

        while (tether.localScale.y > targetPos.y)
        {
            tether.localScale -= new Vector3(0, 6f, 0) * Time.deltaTime;
            yield return null;
        }

        tether.localScale = targetPos;  // stop the scale from going past target

        readyPlayer.SetActive(true);    // set player as ready
        greenLight.SetActive(true);

        lobbyScript.UpdateIntensity();

        if (PhotonNetwork.IsMasterClient && lobbyScript.CheckReady() == PhotonNetwork.PlayerList.Length)
            lobbyScript.StartGame();
    }

    // Instantly set tether length to 0 so player appears to disappear.
    public void ResetTether()
    {
        if (moveTetherIn != null) StopCoroutine(moveTetherIn);
        if (moveTetherOut != null) StopCoroutine(moveTetherOut);
        readyPlayer.SetActive(false);    // set player as unready
        greenLight.SetActive(false);
        tether.localScale = new Vector3(0.1f, 0, 1);

        lobbyScript.UpdateIntensity();
    }
}


