using UnityEngine;

public abstract class BookPage : MonoBehaviour
{
    protected BookUIController BaseUI;

    public abstract void Init(BookUIController baseUI);
    public abstract void OnBookOpen();
    public abstract void OnPageOpen();
    public abstract void OnNavigate(Vector2 value);
    public abstract void OnSubmit();
}