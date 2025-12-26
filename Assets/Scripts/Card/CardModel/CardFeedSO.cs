// CardFeedSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Feed")]
public class CardFeedSO : ScriptableObject
{
    public enum Mode { Loop, Shuffle, Once }

    [Tooltip("Which cards to show, in order.")]
    public CardSO[] items;

    [Tooltip("Loop: repeat; Shuffle: random order; Once: exhaust then stop")]
    public Mode mode = Mode.Loop;
}
