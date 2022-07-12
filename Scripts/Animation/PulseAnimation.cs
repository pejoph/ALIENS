//================================
//  Author: Peter Phillips, 2022
//  File:   PulseAnimation.cs
//================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseAnimation : MonoBehaviour
{
    public float pulseRate;
    public float pulseScale;

    private float timer = 0f;
    private float deathTime = 10f;

    void Update()
    {
        timer += Time.deltaTime;

        float pulseSize = 1 + pulseScale * Mathf.Sin(timer * 2 * Mathf.PI * pulseRate);
        transform.localScale = new Vector3(pulseSize, pulseSize, 1);

        if (timer >= deathTime)
        {
            Debug.Log("System overheated.");

            GameObject.Find("GameManager").GetComponent<GameManager>().TakeDamage();
            Destroy(gameObject);
        }
    }
}
