using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HomeUIManager : MonoBehaviour
{
    public void OnPlayClicked()
    {
        SceneManager.LoadScene("Game");
    }

    public void OnPlayFor10Clicked()
    {
        SceneManager.LoadScene("Game");
    }
}
