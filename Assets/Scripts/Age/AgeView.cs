using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AgeView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ageText;

    public void UpdateAgeDisplay(int age)
    {
        ageText.text = age.ToString();
    }
}