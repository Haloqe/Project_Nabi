using UnityEngine;

public class LogoUI : MonoBehaviour
{
        public void OnLogoAnimationEnd()
        {
                GameManager.Instance.OnMainMenuInitialLoad();
        }
}