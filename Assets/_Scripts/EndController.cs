using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndController : MonoBehaviour
{
    public float anykeyTimer = 2f;

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
            SceneManager.LoadScene("PrototypeScene");
        }
        anykeyTimer -= Time.deltaTime;
    }
}
