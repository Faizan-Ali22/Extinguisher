using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScript : MonoBehaviour
{
    public float loadTime;
    // Start is called before the first frame update
    void Start()
    {
        loadTime = 8.0f;
    }

    // Update is called once per frame
    void Update()
    {
        loadTime -= Time.deltaTime;
        if (loadTime <= 0f)
        {
            SceneManager.LoadScene(1);
        }
    }

    
}
