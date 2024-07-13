using UnityEngine;

public class CreditsUI : MonoBehaviour
{
        public MainMenuUIController BaseUIController { get; set; }
        public bool IsClosing { get; set; }
        
        public void OnCancel()
        {
                OnEndCredits();
        }

        private void OnEndCredits()
        {
                if (IsClosing) return;
                IsClosing = true;
                UIManager.Instance.isRunningEndingCredits = false;
                if (BaseUIController != null)
                {
                        BaseUIController.OnCreditsClosed();
                }
                else
                {
                        UIManager.Instance.UIIAMap.Enable();
                        Destroy(gameObject);
                }
        }
}