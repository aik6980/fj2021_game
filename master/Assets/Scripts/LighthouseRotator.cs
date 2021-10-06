using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LighthouseRotator : MonoBehaviour
{
    public float speed = 1.0f;

    void Update()
    {
        var starting_rotation = transform.localRotation.eulerAngles;
        transform.localRotation = Quaternion.Euler(starting_rotation.x, starting_rotation.y + speed * Time.deltaTime, starting_rotation.z);
    }
}
