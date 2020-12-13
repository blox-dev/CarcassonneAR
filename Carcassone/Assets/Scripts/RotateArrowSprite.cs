using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateArrowSprite : MonoBehaviour
{
    public float speedFactor = -35f;

    void Update()
    {
        transform.Rotate(Vector3.forward * speedFactor * Time.deltaTime);
    }
}
