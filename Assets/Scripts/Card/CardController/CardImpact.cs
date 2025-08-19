using UnityEngine;

public class CardImpact : MonoBehaviour
{
    // Call this with the decided direction from your controller
    public void ApplyImpact(CardSO card, SwipeDirection dir)
    {
        StatModel.Instance.ApplyCard(card, dir);
    }
}
