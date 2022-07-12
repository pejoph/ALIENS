//==============================================//
//  Author: Peter Phillips                      //
//  File:   SpriteSwapAnimation.cs              //
//  Date Created:   22.10.2021                  //
//  Last Modified:  22.10.2021                  //
//  Brief:  Script that controls simple         //
//          2D spritesheet-style animations.    //
//==============================================//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSwapAnimation : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;          // array of sprites to cycle through
    [SerializeField] float cycleTimeInSeconds;  // time it takes to loop through a full cycle of the sprite array in seconds

    private SpriteRenderer sr;
    private float timer;
    private float timeBetweenSwap;
    private int iterator;

    void Start()
    {
        // If there are no sprites in the sprite array, log an error message and back out.
        if (sprites.Length < 1)
        {
            Debug.Log("No sprites found in array.");
            return;
        }

        sr = GetComponent<SpriteRenderer>();
        timeBetweenSwap = cycleTimeInSeconds / sprites.Length;  // time between each sprite swap in seconds
        iterator = 0;
    }

    void Update()
    {
        if (sprites.Length < 1)
            return;

        timer += Time.deltaTime;    // iterate the timer

        if (timer >= timeBetweenSwap)   // check if we should swap the sprite
        {
            iterator = (iterator + 1) % sprites.Length; // iterate the iterator but loop it between 0 and # of sprites
            
            sr.sprite = sprites[iterator];  // swap out the current sprite with ther next one

            timer = 0;  // reset the timer
        }
    }
}
