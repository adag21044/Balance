using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ProgressPanel : MonoBehaviour
{
    [SerializeField] private Image progressFill;      // 👈 IMAGE
    [SerializeField] private RunController runController;

    private void OnEnable()
    {
        Refresh(false); // panel açılınca ZIPLAMASIN
    }

    public void Refresh(bool animate = true)
    {
        if (!progressFill || !runController)
            return;

        float target = runController.CurrentProgress01;

        // 🔥 PANEL KAPALIYSA TWEEN YOK
        if (!gameObject.activeInHierarchy || !animate)
        {
            progressFill.fillAmount = target;
            return;
        }

        progressFill
            .DOKill(); // eski tween varsa öldür
        progressFill
            .DOFillAmount(target, 0.25f)
            .SetEase(Ease.OutCubic);
    }
}
