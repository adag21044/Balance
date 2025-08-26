using System;

public class ShopModel
{
    public bool isForeSightPurchased = false;

    public event Action OnForeSightPurchased;

    public void PurchaseForeSight()
    {
        if (!isForeSightPurchased)
        {
            isForeSightPurchased = true;
            OnForeSightPurchased?.Invoke();
        }
    }
}