//================================
//  Author: Peter Phillips, 2022
//  File:   EnemyController.cs
//================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    //[SerializeField] private Transform laserBeam;
    [SerializeField] private GameObject laserBeam;

    private Vector3 startPos;
    private float timer;
    private bool canShoot;
    private float shootTime;

    private Coroutine appearCo;
    private Coroutine shootCo;

    void Start()
    {
        startPos = transform.position;
        timer = 0f;
        canShoot = false;
        shootTime = 5f;

        Appear();
    }

    void Update()
    {
        if (!canShoot)
            return;

        timer += Time.deltaTime;

        if (timer >= shootTime)
        {
            Shoot();
            timer = 0f;
        }
    }

    private void Appear()
    {
        if (appearCo != null) StopCoroutine(appearCo);
        appearCo = StartCoroutine(AppearAnimation());
    }

    private IEnumerator AppearAnimation()
    {
        float animationTimer = 0f;

        while (animationTimer < 1f)
        {
            animationTimer += Time.deltaTime;
            
            transform.position = startPos + 5 * Vector3.right * Mathf.Cos(animationTimer / 2 * Mathf.PI);
            
            yield return null;
        }

        animationTimer = 1f;
        transform.position = startPos + Vector3.right * Mathf.Cos(animationTimer / 2 * Mathf.PI);
        canShoot = true;

        while (animationTimer < 9f)
        {
            animationTimer += Time.deltaTime;
            yield return null;
        }

        animationTimer = 1f;

        while (animationTimer > 0f)
        {
            animationTimer -= Time.deltaTime;

            transform.position = startPos + 5 * Vector3.right * Mathf.Cos(animationTimer / 2 * Mathf.PI);

            yield return null;
        }

        Destroy(gameObject);
    }

    private void Shoot()
    {
        if (laserBeam != null)
            Instantiate(laserBeam, transform.position, Quaternion.identity);
    }
}
