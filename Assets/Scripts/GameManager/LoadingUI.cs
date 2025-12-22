using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class LoadingUI : MonoBehaviour
{
    public static LoadingUI Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image fillImage;

    private Tween fillTween;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
        fillImage.fillAmount = 0f;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        fillImage.fillAmount = 0f;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void SetProgress(float value)
    {
        value = Mathf.Clamp01(value);

        fillTween?.Kill();
        fillTween = fillImage
            .DOFillAmount(value, 0.25f)
            .SetEase(Ease.OutQuad);
    }

    public void Hide()
    {
        fillTween?.Kill();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        gameObject.SetActive(false);
    }

    public async UniTask CompleteAndHide()
    {
        SetProgress(1f);
        await UniTask.Delay(300);
        Hide();
    }
}
