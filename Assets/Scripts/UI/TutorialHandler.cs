using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class TutorialHandler : MonoBehaviour
{
        [SerializeField] private Camera tutorialCamera;
        public GameObject guideUI;
        public PlayableDirector cutSceneToPlay;
        private bool isTutorialCombatStarted;
        private float timer = 0.0f;
        
        private void Start()
        {
                guideUI.SetActive(true);
                GameManager.Instance.isRunningTutorial = true;
                InGameEvents.EnemySlayed += OnEndTutorial;
                //StartCoroutine(nameof(Timer));
        }

        private void OnDestroy()
        {
                InGameEvents.EnemySlayed -= OnEndTutorial;
        }

        private IEnumerator Timer()
        {
                while (true)
                {
                        timer += Time.deltaTime;
                        if (timer >= 5.0f)
                        {
                                break;
                        }
                        yield return null;
                }
                OnEnter();
        }
        
        public void OnEnter()
        {
                if (isTutorialCombatStarted) return;
                StopCoroutine(nameof(Timer));
                
                // Tutorial Setup
                UIManager.Instance.InGameCombatUI.GetComponent<Canvas>().worldCamera = tutorialCamera;
                AudioManager.Instance.PlayUIConfirmSound();
                isTutorialCombatStarted = true;
                guideUI.SetActive(false);
                UIManager.Instance.UsePlayerControl();
                
                // Tutorial Enemy Spawn
                var mantis = EnemyManager.Instance.SpawnEnemy((int)EEnemyType.VoidMantis, new Vector3(4.33f, -8.34f, 0f), true);
                mantis.transform.localScale = new Vector3(1.4f, 1.4f, 1);
                var mantisEnemyBase = mantis.GetComponent<EnemyBase>();
                mantisEnemyBase.SetMaxHealth(mantisEnemyBase.EnemyData.MaxHealth * 2);
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
                UIManager.Instance.InGameCombatUI.GetComponent<Canvas>().worldCamera = CameraManager.Instance.uiCamera;
                UIManager.Instance.HideAllInGameUI();
                PlayerController.Instance.gameObject.SetActive(false);
                GameManager.Instance.isRunningTutorial = false;
                cutSceneToPlay.Play(cutSceneToPlay.playableAsset, DirectorWrapMode.None);
        }
}