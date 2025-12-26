using UnityEngine;
using System.Reflection;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private const string HighestAgeKey = "HighestAge";

    private const int DefaultAge = 18;
    private const float DefaultStat = 0.5f;

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

    // CALLED ONLY ON FAIL
    public void ResetStatsOnFail(StatModel model)
    {
        int currentAge = Mathf.FloorToInt(model.age);
        int highestAge = PlayerPrefs.GetInt(HighestAgeKey, DefaultAge);

        if (currentAge > highestAge)
        {
            PlayerPrefs.SetInt(HighestAgeKey, currentAge);
            PlayerPrefs.Save();
            Debug.Log($"[SaveSystem] HighestAge updated → {currentAge}");
        }

        // RESET RUN (NOT SAVED)
        model.age = DefaultAge;
        SetAllStats(model, DefaultStat);
    }

    public int GetHighestAge()
    {
        return PlayerPrefs.GetInt(HighestAgeKey, DefaultAge);
    }

    private void SetAllStats(StatModel model, float value)
    {
        typeof(StatModel).GetField("heartPercantage",
            BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(model, value);

        typeof(StatModel).GetField("careerPercantage",
            BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(model, value);

        typeof(StatModel).GetField("happinessPercantage",
            BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(model, value);

        typeof(StatModel).GetField("sociabilityPercantage",
            BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(model, value);
    }

    public void DeleteAll()
    {
        PlayerPrefs.DeleteKey(HighestAgeKey);
        Debug.Log("[SaveSystem] HighestAge deleted");
    }

    public bool TryUpdateHighestAge(int currentAge)
    {
        int highestAge = PlayerPrefs.GetInt(HighestAgeKey, DefaultAge);

        if (currentAge > highestAge)
        {
            PlayerPrefs.SetInt(HighestAgeKey, currentAge);
            PlayerPrefs.Save();
            Debug.Log($"[SaveSystem] New HighestAge reached: {currentAge}");
            return true; // IMPORTANT
        }

        return false;
    }
}
