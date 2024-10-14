using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingPlayerIndicator : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Quaternion lookRotation = Camera.main.transform.rotation;
        transform.rotation = lookRotation;
    }
}
