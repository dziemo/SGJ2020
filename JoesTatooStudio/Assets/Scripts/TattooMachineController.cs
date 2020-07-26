using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TattooMachineController : MonoBehaviour
{
    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        transform.position = (Vector3)mousePos + new Vector3(0, 0, -1);
    }
}
