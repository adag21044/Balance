using System;

public class StatModel
{
    public static StatModel Instance { get; private set; } = new StatModel();

    public int HeartPercantage { get; set; }
    public int CareerPercantage { get; set; }
    public int HappinessPercantage { get; set; }

    public event Action<float> OnHeartChanged;
    public event Action<float> OnCareerChanged;
    public event Action<float> OnHappinessChanged;

    public StatModel()
    {
        HeartPercantage = 0;
        CareerPercantage = 0;
        HappinessPercantage = 0;
    }

    public void IncreaseHeart(int amount)
    {
        HeartPercantage += amount;
    }

    public void IncreaseCareer(int amount)
    {
        CareerPercantage += amount;
    }

    public void IncreaseHappiness(int amount)
    {
        HappinessPercantage += amount;
    }


    public void DecreaseHeart(int amount)
    {
        HeartPercantage -= amount;
        if (HeartPercantage < 0) HeartPercantage = 0; // Ensure it doesn't go below zero
    } 
    public void DecreaseCareer(int amount)
    {
        CareerPercantage -= amount;
        if (CareerPercantage < 0) CareerPercantage = 0; // Ensure it doesn't go below zero
    }

    public void DecreaseHappiness(int amount)
    {
        HappinessPercantage -= amount;
        if (HappinessPercantage < 0) HappinessPercantage = 0; // Ensure it doesn't go below zero
    }
}
