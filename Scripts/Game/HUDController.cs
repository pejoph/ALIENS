//================================
//  Author: Peter Phillips, 2022
//  File:   HUDController.cs
//================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Image playerIcon;
    [SerializeField] private RectTransform roleIcon;
    [SerializeField] private Sprite[] roleIcons;
    [SerializeField] private Image[] healthSprites;
    [SerializeField] private Sprite[] shieldNoShield;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject victoryScreen;

    private Coroutine pulseCo;
    private Color[] playerColours =
    {
        Color.white,
        Color.blue,
        Color.red,
        Color.green,
        Color.yellow,
        Color.cyan
    };

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
            return;
        // Set player icon to appropriate player colour.
        playerIcon.color = playerColours[PhotonNetwork.LocalPlayer.ActorNumber];
    }

    public void UpdateRoleIcon(int newRole)
    {
        roleIcon.GetComponent<Image>().sprite = roleIcons[newRole];
    
        if (pulseCo != null) StopCoroutine(pulseCo);
        pulseCo = StartCoroutine(PulseAnimation(roleIcon));
    }

    public void UpdateHealth(int newHealth)
    {
        for (int i = 0; i < healthSprites.Length; i++)
        {
            if (newHealth > i)
                healthSprites[i].sprite = shieldNoShield[0];
            else
                healthSprites[i].sprite = shieldNoShield[1];
        }
    }

    private IEnumerator PulseAnimation(RectTransform element)
    {
        float timer = 0f;
        float animationTime = .75f;

        while (timer < animationTime)
        {
            timer += Time.deltaTime;

            element.localScale = Vector3.one * (1 + .5f * Mathf.Sin(timer * Mathf.PI / animationTime));

            yield return null;
        }

        element.localScale = Vector3.one;
    }

    public void VictoryScreen()
    {
        victoryScreen.SetActive(true);
    }

    public void GameOverScreen()
    {
        gameOverScreen.SetActive(true);
    }
}
