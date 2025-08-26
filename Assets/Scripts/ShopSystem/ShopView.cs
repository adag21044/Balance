using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopView : MonoBehaviour
{
    [SerializeField] private Image foreSightIcon;

    public void UpdateForeSightText(TextMeshProUGUI foreSightText)
    {
        // Convert text to integer
        int currentValue = int.Parse(foreSightText.text);

        // Increment value
        currentValue += 1;

        // Update text back
        foreSightText.text = currentValue.ToString();
    }
}