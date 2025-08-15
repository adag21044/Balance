using UnityEngine;

public class CardImpact : MonoBehaviour
{
    public void ApplyImpact(CardSO card)
    {
        StatModel.Instance.ApplyCard(card);
    }
}
