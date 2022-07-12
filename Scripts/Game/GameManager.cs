//========================================
//  Author: Peter Phillips             
//  File:   GameManager.cs         
//  Date Created:   09.11.2021         
//  Last Modified:  10.11.2021         
//  Brief:  Script that controls the game
//          scene.
//========================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPun
{
    [HideInInspector] public bool controlsEnabled;

    [SerializeField] private GameObject[] obstacles;
    [SerializeField] private GameObject[] logVine;
    [SerializeField] private GameObject energy;
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private Transform[] overheatPositions;
    [SerializeField] private GameObject[] playerControls;
    [SerializeField] private RobotController robotController;
    [SerializeField] private GameObject[] tutorialObjects; // 0-Hand, 1-Log, 2-Vine, 3-Overheat, 4-Enemy, 5-Rock, 6-Laser, 7-Energy
    [SerializeField] private HUDController hud;
    [SerializeField] private GameObject[] batteryChargeSprites;

    private float timer;
    private bool iterateTime;
    private int energyIterator;
    private int energyMaxIterations;
    private Player[] players;
    private float spawnInterval;
    private int lastObstacle;
    private int lastEnergy;
    private int lastPlayer;
    private float changePlacesTimer;
    private float changePlacesDuration;
    private int currentStation;
    private int roundTracker;
    private float batteryCharge;
    private float fullCharge;
    private float batteryDrain;
    private int currentHealth;
    private int maxHealth;
    private Color[] playerColours =
    {
        Color.white,
        Color.blue,
        Color.red,
        Color.green,
        Color.yellow,
        Color.cyan
    };

    private Dictionary<int, string> obstaclesIndex = new Dictionary<int, string>
    {
        { 0, "Log/Vine"},
        { 1, "Overheat"},
        { 2, "Enemy"},
        { 3, "Rock"},
        { 4, "Energy"}
    };

    void Start()
    {
        players = PhotonNetwork.PlayerList;
        timer = 0f;
        energyIterator = 3;
        energyMaxIterations = 5;
        spawnInterval = 3f;
        lastObstacle = -1;
        lastEnergy = -1;
        lastPlayer = -1;
        changePlacesTimer = 0f;
        roundTracker = 0;
        changePlacesDuration = Random.Range(20f, 40f);
        iterateTime = true;
        controlsEnabled = true;
        fullCharge = 120f;
        batteryCharge = fullCharge;
        batteryDrain = 5f;
        maxHealth = 5;
        currentHealth = maxHealth;

        if (!PhotonNetwork.IsConnected)
            return;
        // Set background colour to appropriate player colour.
        Vector3 tempCol;
        Color.RGBToHSV(playerColours[PhotonNetwork.LocalPlayer.ActorNumber], out tempCol.x, out tempCol.y, out tempCol.z);
        tempCol.y = .4f;
        background.color = Color.HSVToRGB(tempCol.x, tempCol.y, tempCol.z);
        // Set current station.
        currentStation = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        // Loop through controls and activate only the correct one.
        ChangeControls(currentStation);
        // Stop anything from spawning during initial tutorial.
        ResumeTime(true);
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient || !iterateTime)
            return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            int randPlayer = Random.Range(0, PhotonNetwork.PlayerList.Length);
            // Stop same player from being selected twice in a row.
            if (randPlayer == lastPlayer)
                randPlayer = (randPlayer + 1) % PhotonNetwork.PlayerList.Length;
            lastPlayer = randPlayer;

            int randObstacle = Random.Range(0, PhotonNetwork.PlayerList.Length);
            // Stop same obstacle from being spawned twice in a row.
            if (randObstacle == lastObstacle)
                randObstacle = (randObstacle + 1) % PhotonNetwork.PlayerList.Length;
            lastObstacle = randObstacle;
            randObstacle = (randObstacle + roundTracker) % obstacles.Length;
           
            photonView.RPC("ReceiveMessage", players[randPlayer]);
            photonView.RPC("SpawnObstacle", players[randPlayer], randObstacle);

            Debug.Log("Sending a message to " + players[randPlayer].NickName);

            timer -= spawnInterval;
            energyIterator++;
        }

        // Deplete energy provided someone is manning energy.
        if (CheckIfSomeoneIsManningEnergy())
        {
            batteryCharge -= Time.deltaTime * batteryDrain;

            if (batteryCharge <= 0)
            {
                Debug.Log("Unstable backup battery kicks in.");

                TakeDamage();
                batteryCharge = fullCharge;
            }

            photonView.RPC("UpdateBatterySprites", RpcTarget.All, batteryCharge);
        }

        // Spawn some energy as long as someone is manning energy.
        if (energyIterator >= energyMaxIterations && CheckIfSomeoneIsManningEnergy())
        {
            int randPlayer = Random.Range(0, PhotonNetwork.PlayerList.Length);
            // Stop same player from being selected twice in a row.
            if (randPlayer == lastEnergy)
                randPlayer = (randPlayer + 1) % PhotonNetwork.PlayerList.Length; 
            lastEnergy = randPlayer;

            photonView.RPC("SpawnObstacle", players[randPlayer], -1);

            energyIterator = 0;
        }

        changePlacesTimer += Time.deltaTime;

        if (changePlacesTimer >= changePlacesDuration)
        {
            // Suspend the spawning of obstacles.
            ResumeTime(true);

            // After 12 seconds, change places.
            StartCoroutine(WaitThenChangePlaces(12));
        }
    }

    public void TakeDamage()
    {
        currentHealth--;

        photonView.RPC("UpdateHealthOnHUD", RpcTarget.AllBuffered, currentHealth);

        if (PhotonNetwork.IsMasterClient && currentHealth <= 0)
        {
            Debug.Log("Game Over.");

            GameOver();
        }
    }

    [PunRPC]
    private void UpdateHealthOnHUD(int newHealth)
    {
        hud.UpdateHealth(newHealth);
    }

    [PunRPC]
    private void UpdateBatterySprites(float currentCharge)
    {
        for (int i = 0; i < batteryChargeSprites.Length; i++)
        {
            if (currentCharge >= i * 20f)
                batteryChargeSprites[i].SetActive(true);
            else
                batteryChargeSprites[i].SetActive(false);
        }
    }

    public void RechargeBattery()
    {
        photonView.RPC("RechargeBatteryRPC", RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RechargeBatteryRPC()
    {
        batteryCharge = fullCharge;
    }

    private bool CheckIfSomeoneIsManningEnergy()
    {
        int minRole = currentStation;
        int maxRole = currentStation + PhotonNetwork.PlayerList.Length - 1;
        int energyRole = 4;

        if (minRole <= energyRole && energyRole <= maxRole)
            return true;
        else
            return false;
    }

    private IEnumerator WaitThenChangePlaces(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        roundTracker++;

        if (roundTracker == 5)
        {
            Debug.Log("Victory!");
            
            EndGame();
            
            yield break;
        }

        photonView.RPC("ChangePlaces", RpcTarget.All);

        spawnInterval -= .5f;
        changePlacesTimer = 0f;
        batteryCharge = fullCharge;
        energyIterator = 3;
        currentHealth = Mathf.Clamp(currentHealth + 1, 0, maxHealth);
        photonView.RPC("UpdateHealthOnHUD", RpcTarget.AllBuffered, currentHealth);
        changePlacesDuration = Random.Range(20f, 40f);
    }

    private void GameOver()
    {
        ResumeTime(true);

        photonView.RPC("GameOverPUN", RpcTarget.AllBuffered);
        EnableControls(false);
    }

    [PunRPC]
    private void GameOverPUN()
    {
        hud.GameOverScreen();
    }

    private void EndGame()
    {
        ResumeTime(true);

        photonView.RPC("EndGamePUN", RpcTarget.AllBuffered);
        EnableControls(false);
    }

    [PunRPC]
    private void EndGamePUN()
    {
        hud.VictoryScreen();
    }

    [PunRPC]
    private void ChangePlaces()
    {
        robotController.ChangePlaces();

        currentStation = (currentStation + 1) % playerControls.Length;

        ChangeControls(currentStation);
    }

    private void ResumeTimeInvokeable()
    {
        photonView.RPC("ResumeTimePUN", RpcTarget.AllBuffered, false);
        EnableControls(true);
    }

    private void ResumeTime(bool opposite = false)
    {
        photonView.RPC("ResumeTimePUN", RpcTarget.AllBuffered, opposite);
    }

    private void EnableControls(bool enable = true)
    {
        photonView.RPC("EnableControlsPUN", RpcTarget.AllBuffered, enable);
    }

    [PunRPC]
    private void EnableControlsPUN(bool enable = true)
    {
        if (enable)
            controlsEnabled = true;
        else
            controlsEnabled = false;    
    }

    [PunRPC]
    private void ResumeTimePUN(bool opposite)
    {
        if (!opposite)
            iterateTime = true;
        else
            iterateTime = false;
    }

    private void ChangeControls(int index)
    {
        // Loop through controls and activate only the correct one.
        for (int i = 0; i < playerControls.Length; i++)
        {
            if (i == index)
                playerControls[i].SetActive(true);
            else
                playerControls[i].SetActive(false);
        }

        TutorialAnimation(index);

        hud.UpdateRoleIcon(index);
    }

    private void TutorialAnimation(int index)
    {
        EnableControls(false);

        switch (index)
        {
            case 0: // Movement.
                StartCoroutine(MovementTutorial()); // Total time: 15s.
                break;

            case 1: // Repairs.
                StartCoroutine(RepairsTutorial());  // Total time: 16s.
                break;

            case 2: // Weapons.
                StartCoroutine(WeaponsTutorial());  // Total time: 14.5s.
                break;

            case 3: // Shields.
                StartCoroutine(ShieldsTutorial());  // Total time: 15s.
                break;

            case 4: // Energy.
                StartCoroutine(EnergyTutorial());   // Total time: 15.5s
                break;
        }

        if (!PhotonNetwork.IsMasterClient)
            return;
        // Resume the spawning of enemies after 16 seconds (the longest tutorial).
        Invoke("ResumeTimeInvokeable", 16f);
    }

    private IEnumerator MovementTutorial()
    {
        float tutorialTimer = 0f;

        GameObject hand = Instantiate(tutorialObjects[0], Vector3.zero, Quaternion.identity);
        GameObject vine = Instantiate(tutorialObjects[2], new Vector3(10f, 3.9f, 0f), Quaternion.identity);

        // Fade objects in.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;
       
            hand.GetComponent<SpriteRenderer>().color = new Vector4(1, 1, 1, tutorialTimer);
            vine.GetComponent<SpriteRenderer>().color = new Vector4(1, 1, 1, tutorialTimer);
            
            yield return null;
        }

        hand.GetComponent<SpriteRenderer>().color = Color.white;
        vine.GetComponent<SpriteRenderer>().color = Color.white;
        tutorialTimer -= 1f;

        // Move hand to head.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += new Vector3(-3, 2.3f, 0) * Time.deltaTime;

            yield return null;
        }

        GameObject log = Instantiate(tutorialObjects[1], new Vector3(12f, -3.5f, 0f), Quaternion.identity);
        hand.transform.position = new Vector3(-3, 2.3f, 0);
        tutorialTimer -= 1f;

        // Activate duck control.
        robotController.DuckPUN(true);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Move hand to Tracks.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += new Vector3(0, -5f, 0) * Time.deltaTime;

            yield return null;
        }

        hand.transform.position = new Vector3(-3, -2.7f, 0);
        tutorialTimer -= 1f;

        // Activate hover control.
        robotController.HoverPUN(true);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Move hand to head.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += new Vector3(0, 5f, 0) * Time.deltaTime;

            yield return null;
        }

        hand.transform.position = new Vector3(-3, 2.3f, 0);
        tutorialTimer -= 1f;

        // Activate duck control.
        robotController.DuckPUN(true);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Move hand to Tracks.
        while (tutorialTimer < 2f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += new Vector3(0, -5f, 0) * Time.deltaTime / 2;

            yield return null;
        }

        hand.transform.position = new Vector3(-3, -2.7f, 0);
        tutorialTimer -= 2f;

        // Activate hover control.
        robotController.HoverPUN(true);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;
       
        // Wait a few seconds.
        while (tutorialTimer < 4f)
        {
            tutorialTimer += Time.deltaTime;

            yield return null;
        }

        tutorialTimer -= 4f;

        // Fade objects out.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            if (hand != null) hand.GetComponent<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));
            if (log != null)  log.GetComponent<SpriteRenderer>().color  = new Vector4(1, 1, 1, (1 - tutorialTimer));
            if (vine != null) vine.GetComponent<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        if (hand != null) Destroy(hand);
        if (log != null) Destroy(log);
        if (vine != null) Destroy(vine);
    }

    private IEnumerator RepairsTutorial()
    {
        float tutorialTimer = 0f;

        GameObject hand = Instantiate(tutorialObjects[0], Vector3.zero, Quaternion.identity);

        // Fade hand in.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.GetComponent<SpriteRenderer>().color = new Vector4(1, 1, 1, tutorialTimer);

            yield return null;
        }

        hand.GetComponent<SpriteRenderer>().color = Color.white;
        tutorialTimer -= 1f;

        // Move hand to room 1.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += new Vector3(-4.88f, 1.83f, 0) * Time.deltaTime;

            yield return null;
        }

        hand.transform.position = new Vector3(-4.88f, 1.83f, 0);
        tutorialTimer -= 1f;

        // Activate repair control.
        robotController.RepairPUN(1);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;
        
        // Move hand to room 3.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += (new Vector3(-4.05f, 0f, 0) - new Vector3(-4.88f, 1.83f, 0)) * Time.deltaTime / .5f;

            yield return null;
        }

        hand.transform.position = new Vector3(-4.05f, 0f, 0);
        tutorialTimer -= .5f;

        // Activate repair control.
        robotController.RepairPUN(3);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Move hand to room 5.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += (new Vector3(-4.05f, -1.54f, 0) - new Vector3(-4.05f, 0f, 0)) * Time.deltaTime / .5f;

            yield return null;
        }

        hand.transform.position = new Vector3(-4.05f, -1.54f, 0);
        tutorialTimer -= .5f;

        // Activate repair control.
        robotController.RepairPUN(5);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Move hand back to centre.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += (Vector3.zero - new Vector3(-4.05f, -1.54f, 0)) * Time.deltaTime / .5f;

            yield return null;
        }

        tutorialTimer -= .5f;
        
        // Wait a while.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            yield return null;
        }

        tutorialTimer -= .5f;
        hand.transform.position = Vector3.zero;
        // Instantiate overheat in room 1.
        GameObject overheat = Instantiate(tutorialObjects[3], new Vector3(-5.28f, 2.41f, 0f), Quaternion.identity);

        // Move hand to room 1.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += new Vector3(-4.88f, 1.83f, 0) * Time.deltaTime;

            yield return null;
        }

        hand.transform.position = new Vector3(-4.88f, 1.83f, 0);
        tutorialTimer -= 1f;

        // Activate repair control.
        robotController.RepairPUN(1);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Wait a while.
        while (tutorialTimer < 1.5f)
        {
            tutorialTimer += Time.deltaTime;

            yield return null;
        }

        tutorialTimer -= 1.5f;

        // Destroy overheat.
        if (overheat != null) Destroy(overheat);

        // Wait a second.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            yield return null;
        }

        tutorialTimer -= 1f;

        // Instantiate overheat in room 4.
        overheat = Instantiate(tutorialObjects[3], new Vector3(-5.79f, -0.89f, 0f), Quaternion.identity);

        // Move hand to room 4.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += (new Vector3(-5.58f, -1.61f, 0) - new Vector3(-4.88f, 1.83f, 0)) * Time.deltaTime;

            yield return null;
        }

        hand.transform.position = new Vector3(-5.58f, -1.61f, 0);
        tutorialTimer -= 1f;

        // Activate repair control.
        robotController.RepairPUN(4);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;
       
        // Wait a while.
        while (tutorialTimer < 1.5f)
        {
            tutorialTimer += Time.deltaTime;

            yield return null;
        }

        tutorialTimer -= 1.5f;

        // Destroy overheat.
        if (overheat != null) Destroy(overheat);

        // Fade objects out.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            if (hand != null) hand.GetComponent<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        if (hand != null) Destroy(hand);
    }

    private IEnumerator WeaponsTutorial()
    {
        float tutorialTimer = 0f;

        GameObject hand = Instantiate(tutorialObjects[0], Vector3.zero, Quaternion.identity);

        // Fade hand in.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.GetComponent<SpriteRenderer>().color = new Vector4(1, 1, 1, tutorialTimer);

            yield return null;
        }

        hand.GetComponent<SpriteRenderer>().color = Color.white;
        tutorialTimer -= 1f;

        // Move hand to gun position 1.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += new Vector3(-.6f, 1.3f, 0) * Time.deltaTime / .5f;

            yield return null;
        }

        hand.transform.position = new Vector3(-.6f, 1.3f, 0);
        tutorialTimer -= .5f;
        
        // Activate weapon control.
        robotController.AimPUN(24);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Move hand to gun position 2.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += (new Vector3(-.3f, -.4f, 0) - new Vector3(-.6f, 1.3f, 0)) * Time.deltaTime / .5f;

            yield return null;
        }

        hand.transform.position = new Vector3(-.3f, -.4f, 0);
        tutorialTimer -= .5f;

        // Activate weapon control.
        robotController.AimPUN(-7);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Move hand to gun position 3.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += (new Vector3(-.9f, -2.1f, 0) - new Vector3(-.3f, -.4f, 0)) * Time.deltaTime / .5f;

            yield return null;
        }

        hand.transform.position = new Vector3(-.9f, -2.1f, 0);
        tutorialTimer -= .5f;

        // Activate weapon control.
        robotController.AimPUN(-40);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Spawn enemy.
        GameObject enemy = Instantiate(tutorialObjects[4], new Vector3(7f, 1.8f, 0f), Quaternion.identity);

        // Move hand to centre.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position -= new Vector3(-.9f, -2.1f, 0) * Time.deltaTime / .5f;

            yield return null;
        }

        tutorialTimer -= .5f;
        hand.transform.position = Vector3.zero;

        // Wait a while.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            yield return null;
        }

        tutorialTimer -= 1f;

        // Move hand to gun position 1.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += new Vector3(-.6f, 1.3f, 0) * Time.deltaTime / .5f;

            yield return null;
        }

        hand.transform.position = new Vector3(-.6f, 1.3f, 0);
        tutorialTimer -= .5f;

        // Spawn enemy.
        GameObject enemy2 = Instantiate(tutorialObjects[4], new Vector3(7f, -1.8f, 0f), Quaternion.identity);

        // Activate weapon control.
        robotController.AimPUN(24);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Move hand to gun position 3.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += (new Vector3(-.9f, -2.1f, 0) - new Vector3(-.6f, 1.3f, 0)) * Time.deltaTime / .5f;

            yield return null;
        }

        tutorialTimer -= .5f;
        hand.transform.position = new Vector3(-.9f, -2.1f, 0);

        // Activate weapon control.
        robotController.AimPUN(-40);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Spawn enemy.
        GameObject enemy3 = Instantiate(tutorialObjects[4], new Vector3(7f, 0f, 0f), Quaternion.identity);
        
        // Wait a while.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            yield return null;
        }

        tutorialTimer -= 1f;

        // Move hand to gun position 2.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += (new Vector3(-.3f, -.4f, 0) - new Vector3(-.9f, -2.1f, 0)) * Time.deltaTime / .5f;

            yield return null;
        }

        tutorialTimer -= .5f;
        hand.transform.position = new Vector3(-.3f, -.4f, 0);

        // Activate weapon control.
        robotController.AimPUN(-7);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Wait a while.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            yield return null;
        }

        tutorialTimer -= 1f;

        // Fade objects out.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            if (hand != null) hand.GetComponent<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        if (hand != null) Destroy(hand);
    }

    private IEnumerator ShieldsTutorial()
    {
        float tutorialTimer = 0f;

        GameObject hand = Instantiate(tutorialObjects[0], Vector3.zero, Quaternion.identity);

        // Fade objects in.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.GetComponent<SpriteRenderer>().color = new Vector4(1, 1, 1, tutorialTimer);

            yield return null;
        }

        hand.GetComponent<SpriteRenderer>().color = Color.white;
        tutorialTimer -= 1f;

        // Move hand to shield position 1.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += new Vector3(-2.5f, 1f, 0) * Time.deltaTime / .5f;

            yield return null;
        }

        hand.transform.position = new Vector3(-2.5f, 1f, 0);
        tutorialTimer -= .5f;

        // Activate shield control.
        robotController.ShieldPUN(1.5f);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Move hand to shield position 2.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += (new Vector3(-2.5f, -.5f, 0) - new Vector3(-2.5f, 1f, 0)) * Time.deltaTime / .5f;

            yield return null;
        }

        hand.transform.position = new Vector3(-2.5f, -.5f, 0);
        tutorialTimer -= .5f;
        
        // Spawn rock.
        GameObject rock = Instantiate(tutorialObjects[5], new Vector3(12f, 1.5f, 0f), Quaternion.identity);

        // Activate shield control.
        robotController.ShieldPUN(0f);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;
        
        // Move hand to shield position 3.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += (new Vector3(-2.5f, -2f, 0) - new Vector3(-2.5f, -.5f, 0)) * Time.deltaTime / .5f;

            yield return null;
        }

        hand.transform.position = new Vector3(-2.5f, -2f, 0);
        tutorialTimer -= .5f;
        
        // Spawn rock.
        GameObject rock2 = Instantiate(tutorialObjects[5], new Vector3(12f, -1.5f, 0f), Quaternion.identity);

        // Activate shield control.
        robotController.ShieldPUN(-1.5f);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Move hand to shield position 1.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += (new Vector3(-2.5f, 1f, 0) - new Vector3(-2.5f, -2f, 0)) * Time.deltaTime / .5f;

            yield return null;
        }

        hand.transform.position = new Vector3(-2.5f, 1f, 0);
        tutorialTimer -= .5f;

        // Activate shield control.
        robotController.ShieldPUN(1.5f);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Spawn enemy.
        GameObject enemy = Instantiate(tutorialObjects[4], new Vector3(7f, 0f, 0f), Quaternion.identity);

        // Wait a while.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            yield return null;
        }

        tutorialTimer -= 1f;

        // Move hand to shield position 3.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += (new Vector3(-2.5f, -2f, 0) - new Vector3(-2.5f, 1f, 0)) * Time.deltaTime / .5f;

            yield return null;
        }

        hand.transform.position = new Vector3(-2.5f, -2f, 0);
        tutorialTimer -= .5f;
        
        // Activate shield control.
        robotController.ShieldPUN(-1.5f);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Spawn enemy laser.
        GameObject laser = Instantiate(tutorialObjects[6], new Vector3(7f, 0f, 0f), Quaternion.identity);

        // Wait a while.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            yield return null;
        }

        tutorialTimer -= 1f;

        // Move hand to shield position 2.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += (new Vector3(-2.5f, -.5f, 0) - new Vector3(-2.5f, -2f, 0)) * Time.deltaTime / .5f;

            yield return null;
        }

        hand.transform.position = new Vector3(-2.5f, -.5f, 0);
        tutorialTimer -= .5f;

        // Activate shield control.
        robotController.ShieldPUN(0f);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Wait a while.
        while (tutorialTimer < 2f)
        {
            tutorialTimer += Time.deltaTime;

            yield return null;
        }

        tutorialTimer -= 2f;

        // Fade objects out.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            if (hand != null) hand.GetComponent<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));
            if (enemy != null) enemy.GetComponent<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        if (hand != null) Destroy(hand);
        if (enemy != null) Destroy(enemy);
    }

    private IEnumerator EnergyTutorial()
    {
        float tutorialTimer = 0f;

        GameObject hand = Instantiate(tutorialObjects[0], Vector3.zero, Quaternion.identity);

        // Fade hand in.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.GetComponent<SpriteRenderer>().color = new Vector4(1, 1, 1, tutorialTimer);

            yield return null;
        }

        hand.GetComponent<SpriteRenderer>().color = Color.white;
        tutorialTimer -= 1f;

        // Move hand to beam position 1.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += new Vector3(-2.5f, 1.5f, 0) * Time.deltaTime / .5f;

            yield return null;
        }

        hand.transform.position = new Vector3(-2.5f, 1.5f, 0);
        tutorialTimer -= .5f;

        // Activate beam control.
        robotController.BeamPUN(2f);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Move hand to beam position 2.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += (new Vector3(-2.5f, -.5f, 0) - new Vector3(-2.5f, 1.5f, 0)) * Time.deltaTime / .5f;

            yield return null;
        }

        hand.transform.position = new Vector3(-2.5f, -.5f, 0);
        tutorialTimer -= .5f;

        // Activate beam control.
        robotController.BeamPUN(0f);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Spawn energy.
        GameObject tutEnergy = Instantiate(tutorialObjects[7], new Vector3(12f, 2f, 0f), Quaternion.identity);

        // Move hand to beam position 3.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += (new Vector3(-2.5f, -2.5f, 0) - new Vector3(-2.5f, -.5f, 0)) * Time.deltaTime / .5f;

            yield return null;
        }

        hand.transform.position = new Vector3(-2.5f, -2.5f, 0);
        tutorialTimer -= .5f;

        // Activate beam control.
        robotController.BeamPUN(-2f);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Spawn energy.
        GameObject tutEnergy2 = Instantiate(tutorialObjects[7], new Vector3(12f, 0f, 0f), Quaternion.identity);
        
        // Wait a while.
        while (tutorialTimer < 2f)
        {
            tutorialTimer += Time.deltaTime;

            yield return null;
        }

        tutorialTimer -= 2f;

        // Move hand to beam position 1.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += (new Vector3(-2.5f, 1.5f, 0f) - new Vector3(-2.5f, -2.5f, 0f)) * Time.deltaTime / .5f;

            yield return null;
        }

        hand.transform.position = new Vector3(-2.5f, 1.5f, 0);
        tutorialTimer -= .5f;

        // Activate beam control.
        robotController.BeamPUN(2f);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Wait a while.
        while (tutorialTimer < 2f)
        {
            tutorialTimer += Time.deltaTime;

            yield return null;
        }

        tutorialTimer -= 2f;

        // Move hand to beam position 2.
        while (tutorialTimer < .5f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.position += (new Vector3(-2.5f, -.5f, 0f) - new Vector3(-2.5f, 1.5f, 0f)) * Time.deltaTime / .5f;

            yield return null;
        }

        hand.transform.position = new Vector3(-2.5f, -.5f, 0);
        tutorialTimer -= .5f;

        // Activate beam control.
        robotController.BeamPUN(0f);
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            hand.transform.GetChild(0).localScale = Vector3.one * tutorialTimer * 2;
            hand.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        tutorialTimer -= 1f;

        // Wait a while.
        while (tutorialTimer < 2f)
        {
            tutorialTimer += Time.deltaTime;

            yield return null;
        }

        tutorialTimer -= 2f;

        // Fade objects out.
        while (tutorialTimer < 1f)
        {
            tutorialTimer += Time.deltaTime;

            if (hand != null) hand.GetComponent<SpriteRenderer>().color = new Vector4(1, 1, 1, (1 - tutorialTimer));

            yield return null;
        }

        if (hand != null) Destroy(hand);
    }

    [PunRPC]
    private void ReceiveMessage()
    {
        Debug.Log("I received a message!");
    }

    [PunRPC]
    private void SpawnObstacle(int index)
    {
        GameObject go;
        int randIndex = 0;

        if (index == -1)    // Energy,
        {
            Debug.Log("Spawning: Energy");
            // Spawn energy.
            go = Instantiate(energy, Vector3.right * 30, Quaternion.identity);
        }
        else if (index == 0)    // Log/Vine.
        {
            Debug.Log("Spawning: " + obstaclesIndex[index]);
            randIndex = Random.Range(0, 2);
            // Spawn Log/Vine randomly.
            go = Instantiate(logVine[randIndex], Vector3.right * 30, Quaternion.identity);
        }
        else if (index == 4)    // Fake energy.
        {
            // Do nothing, set go = null to appease the code.
            go = null;
        }
        else
        {
            Debug.Log("Spawning: " + obstaclesIndex[index]);
            // Spawn indexed obstacle.
            go = Instantiate(obstacles[index], Vector3.right * 30, Quaternion.identity);
        }
       
        // Set colour to appropriate player colour, except in the case of the overheat obstacle, or the fake energy.
        if (index != 1 && index != 4)
        {
            Vector3 tempCol;
            Color.RGBToHSV(playerColours[PhotonNetwork.LocalPlayer.ActorNumber], out tempCol.x, out tempCol.y, out tempCol.z);
            tempCol.y = .5f;

            if (go != null)
                go.GetComponent<SpriteRenderer>().color = Color.HSVToRGB(tempCol.x, tempCol.y, tempCol.z);
        }
        
        // Set position based on obstacle.
        float yPos;
        switch (index)
        {
            case -1: // Energy.
                yPos = Random.Range(-1, 2) * 2f;
                go.transform.position = new Vector3(12f, yPos, 0f); 
                break;

            case 0: // Log/Vine.
                if (randIndex == 0) // Log.
                    go.transform.position = new Vector3(12f, -3.5f, 0f);
                else   // Vine.
                    go.transform.position = new Vector3(12f, 3.95f, 0f);
                break;

            case 1: // Overheat.
                go.transform.position = overheatPositions[currentStation].position;
                break;

            case 2: // Enemy.
                yPos = Random.Range(-1, 2) * 1.65f + .15f;
                go.transform.position = new Vector3(7f, yPos, 0f);
                break;

            case 3: // Rock.
                yPos = Random.Range(-1, 2) * 1.5f;
                go.transform.localEulerAngles = new Vector3(0f, 0f, Random.Range(-180, 180));
                go.transform.position = new Vector3(12f, yPos, 0f);
                break;

            case 4: // Fake energy.
                // Do nothing.
                break;

            default:
                Debug.Log("Unexpected obstacle index");
                break;
        }
    }
}
