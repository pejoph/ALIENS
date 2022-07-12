//==================================
//  Author: Peter Phillips, 2022
//  File:   DestroyEnemyCollision.cs
//==================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEnemyCollision : MonoBehaviour
{
    private string laserTag = "Laser";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == laserTag)
        {
            Debug.Log("Laser hit enemy.");
            Destroy(gameObject);
        }
    }
}
