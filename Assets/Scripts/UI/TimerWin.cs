using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TimerWin : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text finalTimeText;
    [SerializeField] private TMP_Text recordText;
    [SerializeField] private GameObject newRecordEffect;
    [SerializeField] private Button rankingButton;
    [SerializeField] private Button menuButton;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            float finalTime = GameManager.Instance.GetFinalTime();
            DisplayFinalTime(finalTime);
            CheckRecord(finalTime);
        }

        // Configurar botões
        if (rankingButton != null)
        {
            rankingButton.onClick.AddListener(GoToRanking);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(GoToMenu);
        }
    }

    private void DisplayFinalTime(float time)
    {
        if (finalTimeText != null)
        {
            finalTimeText.text = $"Tempo: {FormatTime(time)}";
        }
    }

    private void CheckRecord(float time)
    {
        if (GameManager.Instance != null)
        {
            bool isNewRecord = GameManager.Instance.IsNewRecord(time);

            if (recordText != null)
            {
                if (isNewRecord)
                {
                    recordText.text = "NOVO RECORDE! 🏆";
                    recordText.color = Color.yellow;
                    if (newRecordEffect != null) newRecordEffect.SetActive(true);
                }
                else
                {
                    int position = GameManager.Instance.GetPlayerPosition("Player");
                    recordText.text = $"Posição: #{position}";
                    recordText.color = Color.white;
                }
            }
        }
    }

    private void GoToRanking()
    {
        GameManager.Instance.LoadRankingScene();
    }

    private void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = (int)timeInSeconds / 60;
        int seconds = (int)timeInSeconds % 60;
        int milliseconds = (int)(timeInSeconds * 100) % 100;
        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }
}