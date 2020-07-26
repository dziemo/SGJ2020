using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float speed;

    void Update()
    {
        transform.Rotate(transform.up * speed * Time.deltaTime, Space.Self);
    }
}
