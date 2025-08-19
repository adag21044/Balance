using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardMovement : MonoBehaviour,
                            IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private float swipeThreshold = 0.4f;
    [SerializeField] private float swipeDuration  = 0.4f;
    [SerializeField] private float returnDuration = 0.25f;
    [SerializeField] private float maxTiltAngle   = 12f;

    private Vector3 initialPos;
    private bool draggedLeft;
    private Image img;

    // 👉 Controller'a haber vermek için:
    public event Action<bool> Swiped; // bool: true = left, false = right

    private void Awake() => img = GetComponent<Image>();

    public void OnBeginDrag(PointerEventData _) => initialPos = transform.localPosition;

    public void OnDrag(PointerEventData data)
    {
        transform.localPosition += new Vector3(data.delta.x, 0, 0);
        float displacementX = transform.localPosition.x - initialPos.x;
        float normalized    = Mathf.Clamp(displacementX / (Screen.width * 0.5f), -1f, 1f);
        float zAngle        = -normalized * maxTiltAngle;
        transform.localRotation = Quaternion.Euler(0, 0, zAngle);
    }

    public void OnEndDrag(PointerEventData _)
    {
        float moved = Mathf.Abs(transform.localPosition.x - initialPos.x);

        if (moved < Screen.width * swipeThreshold)
        {
            transform.DOLocalMove(initialPos, returnDuration).SetEase(Ease.OutBack);
            transform.DOLocalRotate(Vector3.zero, returnDuration).SetEase(Ease.OutBack);
        }
        else
        {
            draggedLeft = transform.localPosition.x < initialPos.x;
            AnimateOutAndNotify();
        }
    }

    private void AnimateOutAndNotify()
    {
        float offscreenX = draggedLeft
            ? transform.localPosition.x - Screen.width
            : transform.localPosition.x + Screen.width;

        float targetAngle = draggedLeft ? +maxTiltAngle * 2 : -maxTiltAngle * 2;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOLocalMoveX(offscreenX, swipeDuration).SetEase(Ease.InQuad))
           .Join(transform.DOLocalRotate(new Vector3(0, 0, targetAngle), swipeDuration))
           .Join(img.DOFade(0f, swipeDuration))
           .OnComplete(() =>
           {
               // ❌ Destroy yok
               // 👉 Controller'a haber ver
               Swiped?.Invoke(draggedLeft);

               // Görseli resetle (Controller içerikten sonra ortalasın istersen bunu silebilirsin)
               transform.localRotation = Quaternion.identity;
               transform.localPosition = initialPos;
               img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
           });
    }
}
