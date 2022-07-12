//================================
//  Author: Peter Phillips, 2022
//  File:   RobotBodyCollision.cs
//================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotBodyCollision : MonoBehaviour
{
    private string enemyLaserTag = "EnemyLaser";
    private string rockTag = "Rock";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == enemyLaserTag)
        {
            Debug.Log("Laser hit robot.");

            GameObject.Find("GameManager").GetComponent<GameManager>().TakeDamage();
            Destroy(collision.gameObject);
        }
        else if (collision.tag == rockTag)
        {
            Debug.Log("Rock hit robot.");

            GameObject.Find("GameManager").GetComponent<GameManager>().TakeDamage();
            Destroy(collision.gameObject);
        }
    }
}
