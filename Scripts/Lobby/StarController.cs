//==============================================//
//  Author: Peter Phillips                      //
//  File:   StarController.cs                   //
//  Date Created:   22.10.2021                  //
//  Last Modified:  12.11.2021                  //
//  Brief:  Script that controls the creation   //
//          and relative motion of stars in the //
//          background of the lobby.            //
//==============================================//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : MonoBehaviour
{
    [SerializeField] GameObject[] stars;        // array of star objects
    [SerializeField] int numberOfStars = 40;    // number of stars to spawn
    [SerializeField] float halfSpawnWidth = 16, halfSpawnHeight = 9;    // spawn area size
    [SerializeField] float starSpeedMultiplier = 5;     // speed multiplier

    private GameObject[] starsInScene;  // array to contain instantiated stars
    private float[] randomNumbers;      // array for random numbers associated with the instantiated stars
    private Color[] starColours;        // array of different star colours
    private float tan40 = 0.839f;       // close approximation of tan(40deg)

    void Start()
    {
        starsInScene = new GameObject[numberOfStars];
        randomNumbers = new float[numberOfStars];
        starColours = new Color[] { Color.blue / 2f, Color.white / 2f, Color.yellow / 2f, new Color(1, 0.5f, 0, 1)/*orange*/ / 2f, Color.red / 2f};

        for (int i = 0; i < numberOfStars; i++)
        {
            starsInScene[i] = Instantiate(stars[i % stars.Length], new Vector3(Random.Range(-halfSpawnWidth, halfSpawnWidth), Random.Range(-halfSpawnHeight, halfSpawnHeight), 0), Quaternion.Euler(0, 0, Random.Range(0, 360)), gameObject.transform);
            starsInScene[i].GetComponent<SpriteRenderer>().color = starColours[Random.Range(0, starColours.Length)];
            randomNumbers[i] = Random.Range(0.1f, 1.0f);
            starsInScene[i].transform.localScale = new Vector3(randomNumbers[i] / 2f, randomNumbers[i] / 2f, 1);
        }
    }

    void Update()
    {
        for (int i = 0; i < numberOfStars; i++)
        {
            starsInScene[i].transform.position += new Vector3(-1, -tan40, 0) * randomNumbers[i] * starSpeedMultiplier * Time.deltaTime;
            if (starsInScene[i].transform.position.x < -halfSpawnWidth)
            {
                Vector3 newPos = starsInScene[i].transform.position;
                newPos.x = halfSpawnWidth;
                starsInScene[i].transform.position = newPos;
            }
            if (starsInScene[i].transform.position.y < -halfSpawnHeight)
            {
                Vector3 newPos = starsInScene[i].transform.position;
                newPos.y = halfSpawnHeight;
                starsInScene[i].transform.position = newPos;
            }
        }
    }

    public void SetStarSpeed(float newSpeed)
    {
        starSpeedMultiplier = newSpeed;
    }
}
