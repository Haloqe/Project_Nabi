using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    public bool IsToBeDestroyed { get; protected set; }
	private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<T>();
            return _instance;
        }
    }
    
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("Duplicate " + typeof(T) + " detected. Destroying " + gameObject.name);
            Destroy(gameObject);
            IsToBeDestroyed = true;
        }
    }
}