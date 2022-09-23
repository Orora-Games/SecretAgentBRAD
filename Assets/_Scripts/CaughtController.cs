using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CaughtController : MonoBehaviour
{
    public string endSceneToLoad;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /* Get the list of visible targets from FieldOfView component ..*/
        List<Transform> visibleTargets = GetComponent<FieldOfView>().visibleTargets;

        /* .. if this Entity (Enemy) sees Player (target) ... */
        if (visibleTargets.Count > 0)
        {
            /* We're trusting the FOV- to know when it sees a player*/
            SceneManager.LoadScene(endSceneToLoad);
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.transform.tag == "Player")
    //    {
    //        SceneManager.LoadScene( endSceneToLoad );
    //    }
    //}
}
