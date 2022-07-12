//========================================
//  Author: Peter Phillips             
//  File:   JitterAnimation.cs         
//  Date Created:   22.10.2021         
//  Last Modified:  12.11.2021         
//  Brief:  Script that controls simple
//          jittery animations.        
//========================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JitterAnimation : MonoBehaviour
{
    [SerializeField] public float jittersPerSecond;    // controls the speed of the jitter
    [SerializeField] public float jitterSize;          // controls the size of the jitter

    private float timer;
    private float timeBetweenJitters;

    void Start()
    {
        // If jitters per second is 0 or less, log an error message and back out.
        if (jittersPerSecond <= 0)
        {
            Debug.Log("Jitters per second must be above 0.");
            return;
        }

        timeBetweenJitters = 1 / jittersPerSecond;
    }

    void Update()
    {
        timer += Time.deltaTime;    // iterate the timer

        if (timer >= timeBetweenJitters)   // check if we should jitter
        {
            transform.localPosition = new Vector3(Random.Range(-jitterSize, jitterSize), Random.Range(-jitterSize, jitterSize), 0);  // move the x and y co-ordinates of the transform to a random location within the jitter size

            timer = 0;  // reset the timer
        }
    }

    public void SetJitterSize(float newSize)
    {
        jitterSize = newSize;
    }
}
