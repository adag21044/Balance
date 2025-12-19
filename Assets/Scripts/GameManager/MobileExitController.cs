using UnityEngine;

public class MobileExitController : MonoBehaviour
{
    [SerializeField] private GameObject exitPanel;

    private bool isPanelOpen;

    private void Awake()
    {
        exitPanel.SetActive(false);
    }

    private void Update()
    {
        // Android Back Button
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackButton();
        }
    }

    private void HandleBackButton()
    {
        if (isPanelOpen)
            CloseExitPanel();
        else
            OpenExitPanel();
    }

    public void OpenExitPanel()
    {
        isPanelOpen = true;
        exitPanel.SetActive(true);
        Time.timeScale = 0f; // pause
    }

    public void CloseExitPanel()
    {
        isPanelOpen = false;
        exitPanel.SetActive(false);
        Time.timeScale = 1f; // resume
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;

        Debug.Log("[Mobile Exit Controller] Exiting game...");

#if UNITY_ANDROID && !UNITY_EDITOR
        Application.Quit();
#else
        Debug.Log("ExitGame called");
#endif
    }
}
