using UnityEngine;

public interface ILanguageChangeHandler
{
    public void OnLanguageChanged();
}

public abstract class LanguageChangeHandlerBase : MonoBehaviour, ILanguageChangeHandler
{
    protected virtual void Awake()
    {
        GameEvents.languageChanged += OnLanguageChanged;
    }

    public abstract void OnLanguageChanged();
}