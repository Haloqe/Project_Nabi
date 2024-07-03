using System;
using UnityEngine;

public class SceneTransitionHandler : MonoBehaviour
{
        public ECutSceneType cutSceneType;
        private bool _isUnderTransition;

        public void Awake()
        {
                GameManager.Instance.isRunningCutScene = true;
                GameManager.Instance.activeCutScene = cutSceneType;
                CameraManager.Instance.gameObject.SetActive(false);
                switch (cutSceneType)
                {
                        case ECutSceneType.IntroPreTutorial:
                                AudioManager.Instance.PlayIntroCutSceneBGM();
                                break;
                        case ECutSceneType.Ending:
                                AudioManager.Instance.PlayEndingCutSceneBGM();
                                break;
                        default:
                                break;
                }
        }

        public void StartSceneTransition()
        {
                if (_isUnderTransition) return;
                _isUnderTransition = true;
                
                switch (cutSceneType)
                {
                        case ECutSceneType.IntroPreTutorial:
                                GameManager.Instance.LoadTutorial();
                                break;
                        
                        case ECutSceneType.IntroPostTutorial:
                                GameManager.Instance.isRunningCutScene = false;
                                PlayerController.Instance.gameObject.SetActive(true);
                                AudioManager.Instance.StopBgm(2.5f);
                                GameManager.Instance.ContinueGame();
                                break;
                        
                        case ECutSceneType.Ending:
                                GameManager.Instance.isRunningCutScene = false;
                                UIManager.Instance.StartCoroutine(UIManager.Instance.GameEndCoroutine());
                                break;
                }
        }
}