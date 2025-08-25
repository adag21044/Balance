using UnityEngine;

public class EndCard : MonoBehaviour
{
    public CardSO[] gameEndSOs;
    
    public CardSO SetEndCard(CardSO endCardSO)
    {
        var cardView = GetComponent<CardView>();
        if (cardView != null && endCardSO != null)
        {
            cardView.SetContent(endCardSO);
            cardView.CaptureInitial();
        }

        return endCardSO;
    }
}