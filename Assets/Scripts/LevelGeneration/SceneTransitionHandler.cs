using System;
using UnityEngine;

public class SceneTransitionHandler : MonoBehaviour
{
        public ECutSceneType cutSceneType;
        private bool _isUnderTransition;

        public void Awake()
        {
                GameManager.Instance.activeCutScene = cutSceneType;
                CameraManager.Instance.gameObject.SetActive(false);
                if (cutSceneType == ECutSceneType.IntroPreTutorial)
                {
                        AudioManager.Instance.PlayIntroCutSceneBGM();
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
                                PlayerController.Instance.gameObject.SetActive(true);
                                AudioManager.Instance.StopBgm(2.5f);
                                GameManager.Instance.ContinueGame();
                                break;
                }
        }
}