using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardMovement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector3 initialPosition;
    private float distanceMoved;
    private bool swipeLeft;

    public void OnDrag(PointerEventData eventData)
    {
        transform.localPosition = new Vector2(transform.localPosition.x + eventData.delta.x,
                                              transform.localPosition.y);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        initialPosition = transform.localPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        distanceMoved = Mathf.Abs(transform.localPosition.x - initialPosition.x);

        if (distanceMoved < 0.4f * Screen.width)
        {
            transform.localPosition = initialPosition;
        }
        else
        {
            if (transform.localPosition.x > initialPosition.x)
            {
                swipeLeft = false;
            }
            else
            {
                swipeLeft = true;
            }

            StartCoroutine(MoveCard());
        }
    }

    private IEnumerator MoveCard()
    {
        float time = 0f;
        Image image = GetComponent<Image>();

        while (image.color.a > 0)
        {
            time += Time.deltaTime;

            float newX = swipeLeft
                ? Mathf.SmoothStep(transform.localPosition.x, transform.localPosition.x - Screen.width, 4 * time)
                : Mathf.SmoothStep(transform.localPosition.x, transform.localPosition.x + Screen.width, 4 * time);

            transform.localPosition = new Vector3(newX, transform.localPosition.y, 0);

            image.color = new Color(1, 1, 1, Mathf.SmoothStep(1, 0, 4 * time));

            yield return null;
        }

        Destroy(gameObject);
    }

}
