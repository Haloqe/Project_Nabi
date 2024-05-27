using UnityEngine;

public abstract class BookPage : MonoBehaviour
{
    public abstract void Init();
    public abstract void OnBookOpen();
    public abstract void OnPageOpen();
    public abstract void OnNavigate(Vector2 value);
}