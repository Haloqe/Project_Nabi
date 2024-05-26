using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPC_Queen : Interactor
{
    private UIManager _uiManager;

    private void Awake()
    {
        _uiManager = UIManager.Instance;
        StartCoroutine(BounceCoroutine());
    }
    
    protected override void OnInteract(InputAction.CallbackContext obj)
    {
        _uiManager.OpenMetaUpgradeUI();
    }

    private IEnumerator BounceCoroutine()
    {
        Vector3 lowestPos = transform.position;
        Vector3 highestPos = transform.position + new Vector3(0, 2f, 0);
        
        while (true)
        {
            for (float time = 0; time < 3 * 2; time += Time.deltaTime)
            {
                float progress = Mathf.PingPong(time, 3) / 3;
                transform.position = Vector3.Lerp(lowestPos, highestPos, progress);
                yield return null;
            }
        }
    }
}
