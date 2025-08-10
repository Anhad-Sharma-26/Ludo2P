using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button playButton;
    public Button quitButton;

    void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }

    void QuitGame()
    {
        Application.Quit();
    }
}
