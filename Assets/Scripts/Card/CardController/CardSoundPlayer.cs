using UnityEngine;

public class CardSoundPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip swipeClip;

    public void PlaySwipeSound()
    {
        if (audioSource != null && swipeClip != null)
        {
            audioSource.PlayOneShot(swipeClip);
        }
    }
}