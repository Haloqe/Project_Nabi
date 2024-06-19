using UnityEngine;

public class SceneTransitionHandler : MonoBehaviour
{
        public ECutSceneType cutSceneType;

        public void Awake()
        {
                UIManager.Instance.UIIAMap.Disable();
                CameraManager.Instance.gameObject.SetActive(false);
        }
        
        public void StartSceneTransition()
        {
                switch (cutSceneType)
                {
                        case ECutSceneType.IntroPreTutorial:
                                GameManager.Instance.LoadTutorial();
                                break;
                        
                        case ECutSceneType.IntroPostTutorial:
                                GameManager.Instance.isRunningTutorial = false;
                                GameManager.Instance.ContinueGame();
                                break;
                }
        }
}