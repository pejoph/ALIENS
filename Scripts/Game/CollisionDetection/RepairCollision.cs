//================================
//  Author: Peter Phillips, 2022
//  File:   RepairCollision.cs
//================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairCollision : MonoBehaviour
{
    private string repairTag = "Repair";

    private float timer = 0f;
    private float repairTime = 2f;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == repairTag)
        {
            timer += Time.fixedDeltaTime;

            if (timer >= repairTime)
            {
                Debug.Log("System repaired.");
                Destroy(gameObject);
            }
        }
    }
}
