//================================
//  Author: Peter Phillips, 2021
//  File:   SpinAnimation.cs
//================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAnimation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    [SerializeField] bool clockwise;

    void Update()
    {
        Vector3 tempAngles = transform.localEulerAngles;
        tempAngles.z += rotationSpeed * Time.deltaTime * 360f * ((clockwise) ? -1 : 1);
        transform.localEulerAngles = tempAngles;
    }
}
