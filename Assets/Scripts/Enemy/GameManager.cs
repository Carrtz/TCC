using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private float finalGameTime = 0f;

    private void Awake()
    {
        // Implementação do Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayerWins(float time)
    {
        // Salvar o tempo final
        finalGameTime = time;
        Debug.Log("Tempo final salvo: " + FormatTime(finalGameTime));

        // Carregar a cena de vitória
        SceneManager.LoadScene("Win");
    }

    public float GetFinalTime()
    {
        return finalGameTime;
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = (int)timeInSeconds / 60;
        int seconds = (int)timeInSeconds % 60;
        int milliseconds = (int)(timeInSeconds * 100) % 100;
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }
}