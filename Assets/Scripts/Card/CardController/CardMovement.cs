using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

public class CardMovement : MonoBehaviour,
    IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private float swipeThreshold = 0.4f;
    [SerializeField] private float swipeDuration = 0.4f;
    [SerializeField] private float returnDuration = 0.25f;
    [SerializeField] private float maxTiltAngle = 12f;

    private Vector3 initialPos;
    private Image img;

    private float screenWidth;
    private float halfScreenWidth;

    // 🔒 Swipe lock
    private CancellationTokenSource swipeCts;
    private bool swipeLocked;

    public event Action<bool> Swiped;

    private void Awake()
    {
        img = GetComponent<Image>();
        screenWidth = Screen.width;
        halfScreenWidth = screenWidth * 0.5f;
    }

    public void OnBeginDrag(PointerEventData _)
    {
        if (swipeLocked) return;

        initialPos = transform.localPosition;

        transform.DOKill();
        img.DOKill();
    }

    public void OnDrag(PointerEventData data)
    {
        if (swipeLocked) return;

        transform.localPosition += new Vector3(data.delta.x, 0, 0);

        float displacementX = transform.localPosition.x - initialPos.x;
        float normalized = Mathf.Clamp(displacementX / halfScreenWidth, -1f, 1f);
        float zAngle = -normalized * maxTiltAngle;

        transform.localRotation = Quaternion.Euler(0, 0, zAngle);
    }

    public void OnEndDrag(PointerEventData _)
    {
        if (swipeLocked) return;

        float moved = Mathf.Abs(transform.localPosition.x - initialPos.x);

        if (moved < screenWidth * swipeThreshold)
        {
            transform.DOLocalMove(initialPos, returnDuration).SetEase(Ease.OutBack);
            transform.DOLocalRotate(Vector3.zero, returnDuration).SetEase(Ease.OutBack);
        }
        else
        {
            bool draggedLeft = transform.localPosition.x < initialPos.x;
            PlaySwipeAsync(draggedLeft).Forget();
        }
    }

    private async UniTaskVoid PlaySwipeAsync(bool draggedLeft)
    {
        swipeLocked = true;

        swipeCts?.Cancel();
        swipeCts = new CancellationTokenSource();

        float offscreenX = draggedLeft
            ? transform.localPosition.x - screenWidth
            : transform.localPosition.x + screenWidth;

        float targetAngle = draggedLeft ? maxTiltAngle * 2f : -maxTiltAngle * 2f;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOLocalMoveX(offscreenX, swipeDuration).SetEase(Ease.InQuad))
        .Join(transform.DOLocalRotate(new Vector3(0, 0, targetAngle), swipeDuration))
        .Join(img.DOFade(0f, swipeDuration));

        try
        {
            // 🔴 ÖNEMLİ: AsUniTask()
            await seq.AsyncWaitForCompletion().AsUniTask();
        }
        catch (OperationCanceledException)
        {
            // ignore
        }

        // reset
        transform.localPosition = initialPos;
        transform.localRotation = Quaternion.identity;
        img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);

        swipeLocked = false;
        Swiped?.Invoke(draggedLeft);
    }
}
