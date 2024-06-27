using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class TutorialHandler : MonoBehaviour
{
        public GameObject guideUI;
        public PlayableDirector cutSceneToPlay;
        private bool isTutorialCombatStarted;

        private void Start()
        {
                guideUI.SetActive(true);
                GameManager.Instance.isRunningTutorial = true;
                InGameEvents.EnemySlayed += OnEndTutorial;
        }

        private void OnDestroy()
        {
                InGameEvents.EnemySlayed -= OnEndTutorial;
        }

        public void OnEnter()
        {
                if (isTutorialCombatStarted) return;
                AudioManager.Instance.PlayUIConfirmSound();
                isTutorialCombatStarted = true;
                var mantis = EnemyManager.Instance.SpawnEnemy((int)EEnemyType.VoidMantis, new Vector3(4.33f, -8.34f, 0f), true);
                mantis.transform.localScale = new Vector3(1.4f, 1.4f, 1);
                guideUI.SetActive(false);
                UIManager.Instance.UsePlayerControl();
        }
        
        private void OnEndTutorial(EnemyBase enemy)
        {
                PlayerController.Instance.Heal(PlayerController.Instance.playerDamageReceiver.MaxHealth);
                StartCoroutine(StartCutSceneDelayed());
        }
        
        private IEnumerator StartCutSceneDelayed()
        {
                yield return new WaitForSeconds(2.5f);
                UIManager.Instance.PlayerIAMap.Disable();
                UIManager.Instance.UIIAMap.Enable();
                UIManager.Instance.HideAllInGameUI();
                PlayerController.Instance.gameObject.SetActive(false);
                GameManager.Instance.isRunningTutorial = false;
                cutSceneToPlay.Play(cutSceneToPlay.playableAsset, DirectorWrapMode.None);
        }
}