//================================
//  Author: Peter Phillips, 2022
//  File:   EnergyCollection.cs
//================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyCollection : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private string energyTag = "Energy";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == energyTag)
        {
            Debug.Log("Energy collected.");

            gameManager.RechargeBattery();

            Destroy(collision.gameObject);
        }
    }
}
