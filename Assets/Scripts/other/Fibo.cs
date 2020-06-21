using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fibo : MonoBehaviour
{
    [SerializeField] private GameObject fiboPrefab = null;
    [SerializeField] private int size = 6;

    private void Start()
    {
        int[] f = new int[size];
        f[0] = 0;
        f[1] = 1;
        GameObject originFib = Instantiate(fiboPrefab, transform);

        for (int i = 2; i < size; i++)
        {
            GameObject fib = Instantiate(fiboPrefab,
                transform.position + Quaternion.Euler(0f, 0f, 90f * ((i - 1) % 4)) * ((f[i - 1] + f[i - 2]) / 2 * originFib.transform.up),
                Quaternion.Euler(0f, 0f, 90f * ((i - 1) % 4)), transform);
            fib.transform.localScale = (f[i - 1] + f[i - 2]) * Vector3.one;
            f[i] = f[i - 1] + f[i - 2];
        }
    }
}