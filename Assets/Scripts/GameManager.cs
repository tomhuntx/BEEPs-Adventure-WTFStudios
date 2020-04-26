using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int targetFPS = 45;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Simple pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0)
                Time.timeScale = 1;
            else
                Time.timeScale = 0;
        }

        if (Application.targetFrameRate != targetFPS)
        {
            Application.targetFrameRate = targetFPS;
        }
    }
}
