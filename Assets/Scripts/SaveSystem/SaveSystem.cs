using UnityEngine;
using System;
using System.Reflection;

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
        PlayerPrefs.SetFloat(HeartKey, model.HeartPercantage);
        PlayerPrefs.SetFloat(CareerKey, model.CareerPercantage);
        PlayerPrefs.SetFloat(HappinessKey, model.HappinessPercantage);
        PlayerPrefs.SetFloat(SociabilityKey, model.SociabilityPercantage);
        PlayerPrefs.SetInt(AgeKey, Mathf.FloorToInt(model.age));
        PlayerPrefs.Save();
    }

    // Load all stats into model
    public void LoadStats(StatModel model)
    {
        model.age = PlayerPrefs.GetInt(AgeKey, 18);
        // Default değer 0.5f olacak şekilde
        model.GetType().GetField("heartPercantage", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(model, PlayerPrefs.GetFloat(HeartKey, 0.5f));
        model.GetType().GetField("careerPercantage", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(model, PlayerPrefs.GetFloat(CareerKey, 0.5f));
        model.GetType().GetField("happinessPercantage", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(model, PlayerPrefs.GetFloat(HappinessKey, 0.5f));
        model.GetType().GetField("sociabilityPercantage", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(model, PlayerPrefs.GetFloat(SociabilityKey, 0.5f));
    }

    // Reset to defaults (called on fail)
    public void ResetStats(StatModel model)
    {
        model.age = 18;
        typeof(StatModel).GetField("heartPercantage", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(model, 0.5f);
        typeof(StatModel).GetField("careerPercantage", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(model, 0.5f);
        typeof(StatModel).GetField("happinessPercantage", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(model, 0.5f);
        typeof(StatModel).GetField("sociabilityPercantage", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(model, 0.5f);

        SaveStats(model);
    }

    public void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("[SaveSystem] All data deleted");
    }
}
