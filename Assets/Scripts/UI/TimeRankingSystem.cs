using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TimeRankingSystem : MonoBehaviour
{
    [System.Serializable]
    public class PlayerTime
    {
        public string playerName;
        public float time; // Em segundos
        public string formattedTime;

        public PlayerTime(string name, float time, string formattedTime)
        {
            this.playerName = name;
            this.time = time;
            this.formattedTime = formattedTime;
        }
    }

    private const string RANKING_KEY = "TimeRanking";
    private List<PlayerTime> ranking = new List<PlayerTime>();
    private TimerManager timerManager;

    void Start()
    {
        timerManager = FindObjectOfType<TimerManager>();
        LoadRanking();
    }

    // Carregar ranking salvo
    private void LoadRanking()
    {
        if (PlayerPrefs.HasKey(RANKING_KEY))
        {
            string json = PlayerPrefs.GetString(RANKING_KEY);
            RankingData data = JsonUtility.FromJson<RankingData>(json);

            if (data != null && data.times != null)
            {
                ranking = data.times;
            }
        }
    }

    // Salvar ranking
    private void SaveRanking()
    {
        RankingData data = new RankingData { times = ranking };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(RANKING_KEY, json);
        PlayerPrefs.Save();
    }

    // Adicionar novo tempo ao ranking
    public void AddTimeToRanking(string playerName, float time)
    {
        string formattedTime = FormatTime(time);
        PlayerTime newTime = new PlayerTime(playerName, time, formattedTime);
        ranking.Add(newTime);

        // Ordenar por tempo (menor tempo primeiro)
        ranking = ranking.OrderBy(x => x.time).ToList();

        // Manter apenas os top 10 tempos
        if (ranking.Count > 10)
        {
            ranking = ranking.Take(10).ToList();
        }

        SaveRanking();
    }

    // Adicionar tempo atual do timer
    public void AddCurrentTime(string playerName = "Player")
    {
        if (timerManager != null)
        {
            float currentTime = timerManager.GetCurrentTime();
            AddTimeToRanking(playerName, currentTime);
        }
        else
        {
            Debug.LogWarning("TimerManager não encontrado!");
        }
    }

    // Verificar se é um recorde
    public bool IsNewRecord(float time)
    {
        if (ranking.Count == 0) return true;
        return time < ranking[0].time; // Menor que o melhor tempo
    }

    // Obter posição do jogador
    public int GetPlayerPosition(string playerName)
    {
        for (int i = 0; i < ranking.Count; i++)
        {
            if (ranking[i].playerName == playerName)
            {
                return i + 1;
            }
        }
        return -1;
    }

    // Obter top N tempos
    public List<PlayerTime> GetTopTimes(int count = 10)
    {
        return ranking.Take(count).ToList();
    }

    // Formatar tempo (agora é público para ser acessado por outras classes)
    public string FormatTime(float timeInSeconds)
    {
        int minutes = (int)timeInSeconds / 60;
        int seconds = (int)timeInSeconds % 60;
        int milliseconds = (int)(timeInSeconds * 100) % 100;
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }

    // Para debug
    public void DisplayRanking()
    {
        Debug.Log("=== RANKING DE TEMPOS ===");
        for (int i = 0; i < ranking.Count; i++)
        {
            Debug.Log($"{i + 1}. {ranking[i].playerName} - {ranking[i].formattedTime}");
        }
    }

    [System.Serializable]
    private class RankingData
    {
        public List<PlayerTime> times = new List<PlayerTime>();
    }

    // Método para limpar ranking (para testes)
    public void ClearRanking()
    {
        ranking.Clear();
        PlayerPrefs.DeleteKey(RANKING_KEY);
        Debug.Log("Ranking limpo!");
    }
}