using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TESTSCRIPT : MonoBehaviour
{
    public string nextSceneName;
    float elapsed = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= 0.2f)
        {
            SceneManager.LoadScene(nextSceneName);
            elapsed = 0.0f;
        }
    }
}
