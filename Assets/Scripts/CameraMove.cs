using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    private float camera_sens = 0.15f;
    private Vector3 last_mouse_pos;

    void Start()
    {
        last_mouse_pos = new Vector3(255, 255, 255);
    }

    void Update()
    {
        last_mouse_pos = Input.mousePosition - last_mouse_pos;
        last_mouse_pos = new Vector3(-last_mouse_pos.y * camera_sens, last_mouse_pos.x * camera_sens, 0);
        last_mouse_pos = new Vector3(this.transform.eulerAngles.x + last_mouse_pos.x, this.transform.eulerAngles.y + last_mouse_pos.y, 0);
        this.transform.eulerAngles = last_mouse_pos;
        last_mouse_pos = Input.mousePosition;
    }
}
