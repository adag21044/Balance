using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource ambientAudioSource;

    [SerializeField] private AudioClip heartFailSoundClip;
    [SerializeField] private AudioClip careerFailSoundClip;
    [SerializeField] private AudioClip happinessFailSoundClip;
    [SerializeField] private AudioClip sociabilityFailSoundClip;

    [Header("UI")]
    [SerializeField] private GameObject soundDisabledIcon;

    // Single source of truth
    private bool isSoundOn = true;

    private void Start()
    {
        ApplySoundState();
    }

    #region Public API

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        ApplySoundState();

        Debug.Log("[SoundManager] Sound toggled. IsSoundOn: " + isSoundOn);
    }

    public void SetSound(bool enabled)
    {
        isSoundOn = enabled;
        ApplySoundState();
    }

    #endregion

    #region Internal

    private void ApplySoundState()
    {
        if (ambientAudioSource != null)
        {
            ambientAudioSource.mute = !isSoundOn;
        }

        if (soundDisabledIcon != null)
        {
            soundDisabledIcon.SetActive(!isSoundOn);
        }
    }

    private void PlayFailSound(AudioClip clip)
    {
        if (!isSoundOn) return;

        if (ambientAudioSource != null && clip != null)
        {
            ambientAudioSource.PlayOneShot(clip);
        }
    }

    #endregion

    #region Fail Sounds

    public void PlayHeartFailSound()
    {
        PlayFailSound(heartFailSoundClip);
    }

    public void PlayCareerFailSound()
    {
        PlayFailSound(careerFailSoundClip);
    }

    public void PlayHappinessFailSound()
    {
        PlayFailSound(happinessFailSoundClip);
    }

    public void PlaySociabilityFailSound()
    {
        PlayFailSound(sociabilityFailSoundClip);
    }

    #endregion
}
