using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TimerManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TMP_Text timerText;

    private float currentTime = 0f;
    private bool isTimerRunning = false;
    private string gameplaySceneName = "TutorialFinal"; // Ajuste com o nome da sua cena de gameplay


    private void OnDestroy()
    {
        // Cancelar a inscrição no evento
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Se carregou a cena de gameplay, destruir este timer
        if (scene.name == gameplaySceneName)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Iniciar o timer automaticamente
        StartTimer();
        FindTimerUI();
    }

    void Update()
    {
        if (isTimerRunning)
        {
            currentTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    private void FindTimerUI()
    {
        // Procurar o texto do timer na cena atual
        if (timerText == null)
        {
            timerText = GameObject.Find("TimerText")?.GetComponent<TMP_Text>();
        }

        // Atualizar o display do timer
        UpdateTimerDisplay();
    }

    public void StartTimer()
    {
        isTimerRunning = true;
    }

    public void PauseTimer()
    {
        isTimerRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = 0f;
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = "Tempo: " + FormatTime(currentTime);
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = (int)timeInSeconds / 60;
        int seconds = (int)timeInSeconds % 60;
        int milliseconds = (int)(timeInSeconds * 100) % 100;

        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public bool IsTimerRunning()
    {
        return isTimerRunning;
    }
}