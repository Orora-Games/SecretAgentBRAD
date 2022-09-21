using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndController : MonoBehaviour
{
    public float anykeyTimer = 1f;
    public string loadScene;
    private void Start()
    {
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
        else if (anykeyTimer < 0f && Input.anyKey)
        {
            SceneManager.LoadScene( loadScene );
        }
        anykeyTimer -= Time.deltaTime;
    }
}
