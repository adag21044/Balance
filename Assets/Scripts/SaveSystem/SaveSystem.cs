using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private const string AgeKey = "Age";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Method(onlyPlayMode: true)]
    public void SaveNow()
    {
        if (StatModel.Instance != null)
        {
            SaveStats(StatModel.Instance);
            Debug.Log("[SaveSystem] Manual Save via button");
        }
    }

    [Method(onlyPlayMode: true)]
    public void LoadNow()
    {
        if (StatModel.Instance != null)
        {
            LoadStats(StatModel.Instance);
            Debug.Log("[SaveSystem] Manual Load via button");
        }
    }

    [Method(onlyPlayMode: true)]
    public void ResetNow()
    {
        if (StatModel.Instance != null)
        {
            ResetStats(StatModel.Instance);
            Debug.Log("[SaveSystem] Manual Reset via button");
        }
    }

    // Save all stats
    public void SaveStats(StatModel model)
    {
        PlayerPrefs.SetInt(AgeKey, Mathf.FloorToInt(model.age));

        PlayerPrefs.Save();
        Debug.Log("[SaveSystem] Saved all stats");
    }

    // Load all stats into model
    public void LoadStats(StatModel model)
    {
        model.age = PlayerPrefs.GetInt(AgeKey, 18);

        Debug.Log("[SaveSystem] Loaded all stats");
    }

    // Reset to defaults (called on fail)
    public void ResetStats(StatModel model)
    {
        model.age = 18;
        
        SaveStats(model); // hemen kaydet
        Debug.Log("[SaveSystem] Stats reset to defaults");
    }

    public void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("[SaveSystem] All data deleted");
    }
}
