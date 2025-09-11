using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TimerWin : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TMP_Text finalTimeText;

    private string winSceneName = "Win";

  
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Se não estivermos mais na cena de vitória, destruir este objeto
        if (scene.name != winSceneName)
        {
            Destroy(gameObject);
        }
        else
        {
            // Se estamos na cena de vitória, encontrar a UI e mostrar o tempo
            FindFinalTimeUI();
            DisplayFinalTime();
        }
    }

    void Start()
    {
        // Configurar a UI
        FindFinalTimeUI();
        DisplayFinalTime();
    }

    private void FindFinalTimeUI()
    {
        // Procurar o texto do tempo final na cena
        if (finalTimeText == null)
        {
            finalTimeText = GameObject.Find("FinalTimeText")?.GetComponent<TMP_Text>();
        }

        // Se não encontrou, tentar encontrar por tag
        if (finalTimeText == null)
        {
            GameObject finalTimeObj = GameObject.FindGameObjectWithTag("FinalTimeUI");
            if (finalTimeObj != null) finalTimeText = finalTimeObj.GetComponent<TMP_Text>();
        }
    }

    private void DisplayFinalTime()
    {
        if (finalTimeText != null && GameManager.Instance != null)
        {
            float finalTime = GameManager.Instance.GetFinalTime();
            finalTimeText.text = "Tempo Final: " + FormatTime(finalTime);
            Debug.Log("Tempo final exibido: " + FormatTime(finalTime));
        }
        else
        {
            Debug.LogWarning("Texto para tempo final ou GameManager não encontrado");
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = (int)timeInSeconds / 60;
        int seconds = (int)timeInSeconds % 60;
        int milliseconds = (int)(timeInSeconds * 100) % 100;
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }
}