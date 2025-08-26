using System;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    private ShopModel shopModel;
    private ShopView shopView;

    private void Start()
    {
        shopModel = new ShopModel();
        shopModel.OnForeSightPurchased += BuyForeSight;
    }

    public void BuyForeSight()
    {

    }
}