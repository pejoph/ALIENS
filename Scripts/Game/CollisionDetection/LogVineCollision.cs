//================================
//  Author: Peter Phillips, 2022
//  File:   LogVineCollision.cs
//================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogVineCollision : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private string logTag = "Log";
    private string vineTag = "Vine";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == logTag)
        {
            Debug.Log("Log hit tracks.");

            gameManager.TakeDamage();
            Destroy(collision.gameObject);
        }

        else if (collision.tag == vineTag)
        {
            Debug.Log("Vine hit head.");

            gameManager.TakeDamage();
            Destroy(collision.gameObject);
        }
    }
}
