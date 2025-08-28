using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject painelPause;
    [SerializeField] private GameObject painelOpcoes;

    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
   public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void OpenOpcoes()
    {
        painelPause.SetActive(false);
        painelOpcoes.SetActive(true);
    }

    public void CloseOpcoes()
    {
        painelOpcoes.SetActive(false);
        painelPause.SetActive(true);
    }

    public void ToMenuGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
