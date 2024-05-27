using UnityEngine;

public interface ILanguageChangeHandler
{
    public void UpdateText();
}

public abstract class LanguageChangeHandlerBase : MonoBehaviour, ILanguageChangeHandler
{
    protected virtual void Awake()
    {
        GameEvents.LanguageChanged += UpdateText;
    }

    public abstract void UpdateText();
}