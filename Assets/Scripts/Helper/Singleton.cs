using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
                _instance = Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
            return _instance;
        }
        private set => _instance = value;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError($"Multiple instances of singleton {typeof(T)} detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        _instance = this as T;
    }
}
