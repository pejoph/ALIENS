//================================
//  Author: Peter Phillips, 2022
//  File:   ScrollingBG.cs
//================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingBG : MonoBehaviour
{
    [SerializeField] private Transform[] ground;
    [SerializeField] private Transform[] atmosphere1;
    [SerializeField] private Transform[] atmosphere2;

    [SerializeField] private float groundScrollSpeed;
    [SerializeField] private float atmosphere1ScrollSpeed;
    [SerializeField] private float atmosphere2ScrollSpeed;

    [SerializeField] private float atmosphere1BounceHeight;
    [SerializeField] private float atmosphere2BounceHeight;

    private float timer;

    private float groundWidth;
    private float groundStartY;

    private float atmosphere1Width;
    private float atmosphere1StartY;
    private float atmosphere2Width;
    private float atmosphere2StartY;

    void Start()
    {
        timer = 0f;

        groundWidth = 20.48f;
        groundStartY = ground[0].position.y;

        atmosphere1Width = 25f;
        atmosphere1StartY = atmosphere1[0].position.y;
        atmosphere2Width = 25f;
        atmosphere2StartY = atmosphere2[0].position.y;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Scroll the ground sprites.
        Scroll(ground, groundWidth, groundScrollSpeed, groundStartY);

        // Scroll the atmosphere sprites.
        Scroll(atmosphere1, atmosphere1Width, atmosphere1ScrollSpeed, atmosphere1StartY, atmosphere1BounceHeight);
        Scroll(atmosphere2, atmosphere2Width, atmosphere2ScrollSpeed, atmosphere2StartY, atmosphere2BounceHeight, .25f);
    }

    private void Scroll(Transform[] sprites, float spriteWidth, float scrollSpeed = 1f, float startingYPos = 0f, float bounceHeight = 0f, float offset = 0f)
    {
        foreach (Transform t in sprites)
        {
            // Scroll sprite.
            t.position += Vector3.left * Time.deltaTime * scrollSpeed;
            // Reset position once it scrolls off-screen.
            if (t.position.x <= -1.5f * spriteWidth)
                t.position += Vector3.right * spriteWidth * 3;

            Vector3 tempPos = t.position;
            tempPos.y = startingYPos + bounceHeight * Mathf.Sin(timer + offset * 2 * Mathf.PI);
            t.position = tempPos;
        }
    }
}
