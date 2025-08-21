using System;

public class AgeModel
{
    public int Age { get; private set; }

    public event Action<int> OnAgeChanged;

    public void Aging(int amount)
    {
        Age += amount;
        OnAgeChanged?.Invoke(Age);
    }
}