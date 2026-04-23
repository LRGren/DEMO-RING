using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility_DestoryAfterTime : MonoBehaviour
{
    [SerializeField] private float timeUntilDestroy = 1f;

    private void Awake()
    {
        Destroy(gameObject, timeUntilDestroy);
    }
}
