using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip heartFailSoundClip;
    public AudioClip careerFailSoundClip;
    public AudioClip happinessFailSoundClip;
    public AudioClip sociabilityFailSoundClip;

    public void PlayHeartFailSound()
    {
        if (audioSource != null && heartFailSoundClip != null)
        {
            audioSource.PlayOneShot(heartFailSoundClip);
        }
    }

    public void PlayCareerFailSound()
    {
        if (audioSource != null && careerFailSoundClip != null)
        {
            audioSource.PlayOneShot(careerFailSoundClip);
        }
    }

    public void PlayHappinessFailSound()
    {
        if (audioSource != null && happinessFailSoundClip != null)
        {
            audioSource.PlayOneShot(happinessFailSoundClip);
        }
    }

    public void PlaySociabilityFailSound()
    {
        if (audioSource != null && sociabilityFailSoundClip != null)
        {
            audioSource.PlayOneShot(sociabilityFailSoundClip);
        }
    }
}