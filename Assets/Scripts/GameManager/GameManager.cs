using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private CardController cardController;
    private AgeModel ageModel;
    [SerializeField] private AgeView ageView;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        ageModel = new AgeModel();
        ageModel.OnAgeChanged += ageView.UpdateAgeDisplay;
        ageModel.Aging(18);
    }
}