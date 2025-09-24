using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private const string AgeKey = "Age";
    private const string HeartKey = "Heart";
    private const string CareerKey = "Career";
    private const string HappinessKey = "Happiness";
    private const string SociabilityKey = "Sociability";

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
        PlayerPrefs.SetFloat(HeartKey, model.HeartPercantage);
        PlayerPrefs.SetFloat(CareerKey, model.CareerPercantage);
        PlayerPrefs.SetFloat(HappinessKey, model.HappinessPercantage);
        PlayerPrefs.SetFloat(SociabilityKey, model.SociabilityPercantage);

        PlayerPrefs.Save();
        Debug.Log("[SaveSystem] Saved all stats");
    }

    // Load all stats into model
    public void LoadStats(StatModel model)
    {
        model.age = PlayerPrefs.GetInt(AgeKey, 18);
        model.GetType().GetField("heartPercantage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(model, PlayerPrefs.GetFloat(HeartKey, 0.5f));
        model.GetType().GetField("careerPercantage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(model, PlayerPrefs.GetFloat(CareerKey, 0.5f));
        model.GetType().GetField("happinessPercantage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(model, PlayerPrefs.GetFloat(HappinessKey, 0.5f));
        model.GetType().GetField("sociabilityPercantage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(model, PlayerPrefs.GetFloat(SociabilityKey, 0.5f));

        Debug.Log("[SaveSystem] Loaded all stats");
    }

    // Reset to defaults (called on fail)
    public void ResetStats(StatModel model)
    {
        model.age = 18f;
        model.GetType().GetField("heartPercantage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(model, 0.5f);
        model.GetType().GetField("careerPercantage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(model, 0.5f);
        model.GetType().GetField("happinessPercantage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(model, 0.5f);
        model.GetType().GetField("sociabilityPercantage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(model, 0.5f);

        SaveStats(model); // hemen kaydet
        Debug.Log("[SaveSystem] Stats reset to defaults");
    }

    public void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("[SaveSystem] All data deleted");
    }
}
