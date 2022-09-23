using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntelController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            Destroy(gameObject);
        }
    }
}
