//================================
//  Author: Peter Phillips, 2022
//  File:   StarController.cs
//================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RobotController : MonoBehaviourPun
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Transform[] head;
    [SerializeField] private Transform arm;
    [SerializeField] private Transform gun;
    [SerializeField] private Transform tracks;
    [SerializeField] private Transform[] jets;
    [SerializeField] private Transform[] repairArm;
    [SerializeField] private ArmTransforms[] repairArmPositions;
    [SerializeField] private GameObject shield;
    [SerializeField] private Transform beam;
    [SerializeField] private Transform laser;
    [SerializeField] private SpriteRenderer[] players;

    private float duckProgress;     // .6f;
    private float hoverProgress;    // 1.7f , 1.3f
    private float aimProgress;      // 20f, -7f, -38f
    private float shieldProgress;   // .8f;
    private float beamProgress;     // 2.5f;
    private float changeProgress;     // .9f;
    private float duckMax = .6f;     
    private float tracksMax = 1.7f;
    private float jetsMax = 1.3f;
    private float shieldMax = .8f;
    private float beamMax = 2.5f;
    private float changeMax = .9f;
    private Coroutine duckCo;
    private Coroutine hoverCo;
    private Coroutine aimCo;
    private Coroutine shootCo;
    private Coroutine repairCo;
    private Coroutine shieldCo;
    private Coroutine beamCo;
    private Coroutine changeCo;
    private float weaponTimer;
    private float weaponCooldown = 1f;

    [System.Serializable]
    public class ArmTransforms
    {
        public Transform[] transforms;
    }

    void Start()
    {
        duckProgress = 0f;
        hoverProgress = 0f;
        aimProgress = -7f;
        shieldProgress = 0f;
        beamProgress = 0f;
        changeProgress = .9f;
        weaponTimer = weaponCooldown;
    }

    private void Update()
    {
        weaponTimer += Time.deltaTime;
    }

    [PunRPC]
    public void DuckPUN(bool down)
    {
        if (down)
        {
            if (hoverCo != null) StopCoroutine(hoverCo);
            hoverCo = StartCoroutine(HoverAnimation(false));
        }

        if (duckCo != null) StopCoroutine(duckCo);
        duckCo = StartCoroutine(DuckAnimation(down));
    }

    public void Duck(bool down)
    {
        if (!gameManager.controlsEnabled)
            return;

        if (PhotonNetwork.IsConnected)
            photonView.RPC("DuckPUN", RpcTarget.All, down);
        else
            DuckPUN(down);
    }

    [PunRPC]
    public void HoverPUN(bool on)
    {
        if (on)
        {
            if (duckCo != null) StopCoroutine(duckCo);
            duckCo = StartCoroutine(DuckAnimation(false));
        }

        if (hoverCo != null) StopCoroutine(hoverCo);
        hoverCo = StartCoroutine(HoverAnimation(on));
    }

    public void Hover(bool on)
    {
        if (!gameManager.controlsEnabled)
            return;
        
        if (PhotonNetwork.IsConnected)
            photonView.RPC("HoverPUN", RpcTarget.All, on);
        else
            HoverPUN(on);
    }

    [PunRPC]
    public void AimPUN(int angle)
    {
        if (aimCo != null) StopCoroutine(aimCo);
        aimCo = StartCoroutine(AimAnimation(angle));
    }

    public void Aim(int angle)
    {
        if (weaponTimer < weaponCooldown || !gameManager.controlsEnabled)
            return;

        weaponTimer = 0f;

        if (PhotonNetwork.IsConnected)
            photonView.RPC("AimPUN", RpcTarget.All, angle);
        else
            AimPUN(angle);
    }

    public void Shoot()
    {
        if (shootCo != null) StopCoroutine(shootCo);
        shootCo = StartCoroutine(ShootAnimation());
    }
    
    [PunRPC]
    public void RepairPUN(int room)
    {
        if (repairCo != null) StopCoroutine(repairCo);
        repairCo = StartCoroutine(RepairAnimation(room));
    }

    public void Repair(int room)
    {
        if (!gameManager.controlsEnabled)
            return;

        if (PhotonNetwork.IsConnected)
            photonView.RPC("RepairPUN", RpcTarget.All, room);
        else
            RepairPUN(room);
    }

    [PunRPC]
    public void ShieldPUN(float yPos)
    {
        if (shieldCo != null) StopCoroutine(shieldCo);
        shieldCo = StartCoroutine(ShieldAnimation(yPos));
    }

    public void Shield(float yPos)
    {
        if (!gameManager.controlsEnabled)
            return;

        if (PhotonNetwork.IsConnected)
            photonView.RPC("ShieldPUN", RpcTarget.All, yPos);
        else
            ShieldPUN(yPos);
    }

    [PunRPC]
    public void BeamPUN(float yPos)
    {
        if (beamCo != null) StopCoroutine(beamCo);
        beamCo = StartCoroutine(BeamAnimation(yPos));
    }

    public void Beam(float yPos)
    {
        if (!gameManager.controlsEnabled)
            return;

        if (PhotonNetwork.IsConnected)
            photonView.RPC("BeamPUN", RpcTarget.All, yPos);
        else
            BeamPUN(yPos);
    }

    public void ChangePlaces()
    {
        if (changeCo != null) StopCoroutine(changeCo);
        changeCo = StartCoroutine(ChangePlacesAnimation());
    }

    private IEnumerator DuckAnimation(bool down)
    {
        float animationTime = .25f;
    
        while ((down) ? duckProgress < duckMax : duckProgress > 0)
        {
            duckProgress += Time.deltaTime * duckMax / animationTime * ((down) ? 1 : -1);

            foreach (Transform t in head)
                t.localPosition = new Vector3(t.localPosition.x, -duckProgress, t.localPosition.z);

            yield return null;
        }

        foreach (Transform t in head)
            t.localPosition = new Vector3(t.localPosition.x, (down) ? -duckMax : 0, t.localPosition.z);

        if (down)
        {
            yield return new WaitForSeconds(5);
            DuckPUN(false);
        }
    }

    private IEnumerator HoverAnimation(bool on)
    {
        float animationTime = .35f;

        while ((on) ? hoverProgress < tracksMax : hoverProgress > 0)
        {
            hoverProgress += Time.deltaTime * tracksMax / animationTime * ((on) ? 1 : -1);

            tracks.localPosition = new Vector3(tracks.localPosition.x, hoverProgress, tracks.localPosition.z);

            foreach (Transform t in jets)
                t.localPosition = new Vector3(t.localPosition.x, -hoverProgress * jetsMax / tracksMax, t.localPosition.z);

            yield return null;
        }

        tracks.localPosition = new Vector3(tracks.localPosition.x, (on) ? tracksMax : 0, tracks.localPosition.z);

        foreach (Transform t in jets)
            t.localPosition = new Vector3(t.localPosition.x, (on) ? -jetsMax : 0, t.localPosition.z);

        if (on)
        {
            yield return new WaitForSeconds(5);
            HoverPUN(false);
        }
    }

    private IEnumerator AimAnimation(float angle)
    {
        float increment = 180 * Time.deltaTime * ((aimProgress < angle) ? 1 : -1);

        while (Mathf.Abs(angle - aimProgress) > Mathf.Abs(increment))
        {
            aimProgress += increment;
            arm.localEulerAngles = new Vector3(arm.localRotation.x, arm.localRotation.y, aimProgress);
            gun.right = Vector3.right;

            yield return null;
        }

        aimProgress = angle;
        gun.right = Vector3.right;

        Shoot();
    }

    private IEnumerator ShootAnimation()
    {
        float laserSpeed = 15f;
        float timer = 0f;
        float animationTime = 1f;
        Vector3 tempPos = laser.transform.localPosition;
        tempPos.x = 0f;
        laser.transform.localPosition = tempPos;

        while (timer < animationTime)
        {
            timer += Time.deltaTime;

            tempPos.x += Time.deltaTime * laserSpeed / animationTime;
            laser.transform.localPosition = tempPos;

            yield return null;
        }

        tempPos.x = 0f;
        laser.transform.localPosition = tempPos;
    }

    private IEnumerator RepairAnimation(int room)
    {
        float timer = 0f;
        float animationTime = .5f;
        float repairTime = 2f;

        Vector3 startingPos1 = repairArm[0].localEulerAngles;
        Vector3 startingPos2 = repairArm[1].localEulerAngles;
        Vector3[] startingPos = {startingPos1, startingPos2};

        while (timer <= animationTime)
        {
            timer += Time.deltaTime;

            for (int i = 0; i < repairArm.Length; i++)
                repairArm[i].localEulerAngles += (repairArmPositions[room].transforms[i].localEulerAngles - startingPos[i]) * Time.deltaTime / animationTime;

            yield return null;
        }
        while (timer <= animationTime + repairTime)
        {
            timer += Time.deltaTime;

            yield return null;
        }

        startingPos = new Vector3[2];
        startingPos[0] = repairArmPositions[room].transforms[0].localEulerAngles;
        startingPos[1] = repairArmPositions[room].transforms[1].localEulerAngles;

        while (timer <= 2 * animationTime + repairTime)
        {
            timer += Time.deltaTime;

            for (int i = 0; i < repairArm.Length; i++)
                repairArm[i].localEulerAngles += (repairArmPositions[0].transforms[i].localEulerAngles - startingPos[i]) * Time.deltaTime / animationTime;

            yield return null;
        }

        for (int i = 0; i < repairArm.Length; i++)
            repairArm[i].localEulerAngles = repairArmPositions[0].transforms[i].localEulerAngles;
    }

    private IEnumerator ShieldAnimation(float yPos)
    {
        float animationTime = .25f;
        Color tempCol = shield.GetComponent<SpriteRenderer>().color;
        Vector3 tempPos = shield.transform.localPosition;

        // Make sure collider is on.
        shield.GetComponent<BoxCollider2D>().enabled = true;

        // If clicking on current position, don't remove the shield.
        if (yPos != tempPos.y)
        {
            // Reduce the alpha of the shield to 0 over the animation time.
            while (shieldProgress > 0)
            {
                shieldProgress -= Time.deltaTime * shieldMax / animationTime;

                tempCol.a = shieldProgress;
                shield.GetComponent<SpriteRenderer>().color = tempCol;

                yield return null;
            }

            // Set shield alpha = 0;
            tempCol.a = 0;
            shield.GetComponent<SpriteRenderer>().color = tempCol;
            // Set yPos of shield = yPos parameter;
            tempPos.y = yPos;
            shield.transform.localPosition = tempPos;
        }

        // Increase the alpha of the shield to max value over the animation time.
        while (shieldProgress < shieldMax)
        {
            shieldProgress += Time.deltaTime * shieldMax / animationTime;

            tempCol.a = shieldProgress;
            shield.GetComponent<SpriteRenderer>().color = tempCol;

            yield return null;
        }

        // Set shield alpha = max;
        tempCol.a = shieldMax;
        shield.GetComponent<SpriteRenderer>().color = tempCol;

        // After 5 seconds, remove the shield.
        yield return new WaitForSeconds(5);
        // Reduce the alpha of the shield to 0 over the animation time.
        while (shieldProgress > 0)
        {
            shieldProgress -= Time.deltaTime * shieldMax / animationTime;

            tempCol.a = shieldProgress;
            shield.GetComponent<SpriteRenderer>().color = tempCol;

            yield return null;
        }

        // Set shield alpha = 0;
        tempCol.a = 0;
        shield.GetComponent<SpriteRenderer>().color = tempCol;
        // Remove collider to stop shield still working.
        shield.GetComponent<BoxCollider2D>().enabled = false;
    }

    private IEnumerator BeamAnimation(float yPos)
    {
        float animationTime = .5f;
        Vector3 tempPos = beam.transform.localPosition;
        Vector3 tempScale = beam.transform.localScale;

        // If clicking on current position, don't remove the beam.
        if (yPos != tempPos.y)
        {
            // Reduce the y-scale of the beam to 0 over the animation time.
            while (beamProgress > 0)
            {
                beamProgress -= Time.deltaTime * beamMax / animationTime;
    
                tempScale.y = beamProgress;
                beam.transform.localScale = tempScale;
    
                yield return null;
            }
    
            // Set beam y-scale = 0;
            tempScale.y = 0;
            beam.transform.localScale = tempScale;
            // Set yPos of beam = yPos parameter;
            tempPos.y = yPos;
            beam.transform.localPosition = tempPos;
        }

        // Increase the y-scale of the beam to max value over the animation time.
        while (beamProgress < beamMax)
        {
            beamProgress += Time.deltaTime * beamMax / animationTime;

            tempScale.y = beamProgress;
            beam.transform.localScale = tempScale;

            yield return null;
        }

        // Set beam y-scale = max;
        tempScale.y = beamMax;
        beam.transform.localScale = tempScale;

        // After 5 seconds, remove the beam.
        yield return new WaitForSeconds(5);
        // Reduce the y-scale of the beam to 0 over the animation time.
        while (beamProgress > 0)
        {
            beamProgress -= Time.deltaTime * beamMax / animationTime;

            tempScale.y = beamProgress;
            beam.transform.localScale = tempScale;

            yield return null;
        }

        // Set beam y-scale = 0;
        tempScale.y = 0;
        beam.transform.localScale = tempScale;
        // Set yPos of beam = yPos parameter;
        tempPos.y = yPos;
        beam.transform.localPosition = tempPos;
    }

    private IEnumerator ChangePlacesAnimation()
    {
        float animationTime = .5f;

        while (changeProgress >= 0)
        {
            changeProgress -= Time.deltaTime * changeMax / animationTime;

            foreach (SpriteRenderer sr in players)
            {
                sr.size = new Vector2(sr.size.x, -changeProgress);
            }

            yield return null;
        }

        changeProgress = 0f;
        for (int i = 1; i < players.Length; i++)
        {
            players[i].size = new Vector2(players[i].size.x, 0);

            // Cycle through player colours.
            if (i % 2 == 0)
                continue;
            Color playerColour = players[i].color;
            if (playerColour == Color.blue)
                players[i].color = Color.cyan;
            else if (playerColour == Color.red)
                players[i].color = Color.blue;
            else if (playerColour == Color.green)
                players[i].color = Color.red;
            else if (playerColour == Color.yellow)
                players[i].color = Color.green;
            else if (playerColour == Color.cyan)
                players[i].color = Color.yellow;
        }

        while (changeProgress <= changeMax)
        {
            changeProgress += Time.deltaTime * changeMax / animationTime;

            foreach (SpriteRenderer sr in players)
            {
                sr.size = new Vector2(sr.size.x, -changeProgress);
            }

            yield return null;
        }
        
        changeProgress = changeMax;
        foreach (SpriteRenderer sr in players)
        {
            sr.size = new Vector2(sr.size.x, -changeMax);
        }
    }
}
