//================================
//  Author: Peter Phillips, 2022
//  File:   ShieldCollision.cs
//================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldCollision : MonoBehaviour
{
    private string enemyLaserTag = "EnemyLaser";
    private string rockTag = "Rock";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == enemyLaserTag)
        {
            Debug.Log("Laser hit shield.");

            Destroy(collision.gameObject);
        }
        else if (collision.tag == rockTag)
        {
            Debug.Log("Rock hit shield.");

            Destroy(collision.gameObject);
        }
    }
}
