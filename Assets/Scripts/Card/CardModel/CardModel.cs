using System;

public enum SwipeDirection { Left, Right }

public class CardModel
{
    public CardSO Config { get; }
    public bool IsLocked { get; private set; }

    // Domain events
    public event Action<CardModel, SwipeDirection> Swiped;
    public event Action<CardModel> ResetRequested;

    public CardModel(CardSO config) => Config = config;

    public void Lock() => IsLocked = true;
    public void Unlock() => IsLocked = false;

    public void NotifySwiped(SwipeDirection dir) => Swiped?.Invoke(this, dir);
    public void RequestReset() => ResetRequested?.Invoke(this);
}