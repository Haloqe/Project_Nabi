using UnityEngine;
using UnityEngine.SceneManagement;

public class TEST_SceneLoader : MonoBehaviour
{
    public string nextSceneName;
    float elapsed = 0.0f;

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
