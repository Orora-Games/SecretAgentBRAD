using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    /* Source: https://docs.unity3d.com/ScriptReference/Vector3.SmoothDamp.html */
    public Transform target;
    public float smoothTime = 0.3F;
    private Vector3 velocity = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPosition = target.position;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.position -= transform.forward * 2; /* Move the camera forwards to avoid clipping through the level */
    }
}

