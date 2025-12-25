using System.Collections.Generic;

public class ProgressTracker
{
    private readonly HashSet<string> seenCardIds = new();

    /// <summary>
    /// Returns true if this card is seen for the first time in this run
    /// </summary>
    public bool TryRegister(CardSO card)
    {
        return seenCardIds.Add(card.Id);
    }

    public void Reset()
    {
        seenCardIds.Clear();
    }
}
