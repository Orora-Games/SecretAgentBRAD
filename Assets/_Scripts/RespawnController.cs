using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnController : MonoBehaviour
{
    public GameObject spawnObject; // This is the object we are spawning.

    // Start is called before the first frame update
    void Start()
    {
        /* Instantiate spawnObject */
        GameObject spawnedGameObject = Instantiate(spawnObject, new Vector3( transform.position.x, transform.position.y, transform.position.z ), transform.rotation,transform);
        /* Making sure our spawned Object is a child of the same level as the spawner. */
        spawnedGameObject.transform.parent = spawnedGameObject.transform.parent.parent;
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
