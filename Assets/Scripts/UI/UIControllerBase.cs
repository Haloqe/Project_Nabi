using UnityEngine;

public abstract class UIControllerBase : MonoBehaviour
{
        public abstract void OnNavigate(Vector2 value);
        public abstract void OnSubmit();
        public abstract void OnClose();
        public abstract void OnTab();
        public virtual void OnSettingsClosed() { }
}