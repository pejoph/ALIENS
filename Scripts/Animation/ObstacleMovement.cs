//================================
//  Author: Peter Phillips, 2022
//  File:   ObstacleMovement.cs
//================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMovement : MonoBehaviour
{
    public float movementSpeed;

    private float timer = 0f;
    private float deathTime = 24f;

    private void Start()
    {
        if (movementSpeed > 0)
            deathTime /= movementSpeed;
    }

    void Update()
    {
        timer += Time.deltaTime;

        transform.position += Vector3.left * movementSpeed * Time.deltaTime;

        if (timer >= deathTime)
            Destroy(gameObject);
    }
}
