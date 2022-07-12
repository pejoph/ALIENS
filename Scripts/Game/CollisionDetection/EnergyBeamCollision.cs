//================================
//  Author: Peter Phillips, 2022
//  File:   EnergyBeamCollision.cs
//================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBeamCollision : MonoBehaviour
{
    private string energyBeamTag = "EnergyBeam";
    private bool fast;

    private void FixedUpdate()
    {
        if (fast)
            GetComponent<ObstacleMovement>().movementSpeed = 4f;
        else
            GetComponent<ObstacleMovement>().movementSpeed = 1f;

        fast = false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == energyBeamTag)
        {
            Debug.Log("Energy inside beam.");
            fast = true;
        }
    }
}
