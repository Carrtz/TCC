using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private float finalGameTime = 0f;

    [System.Serializable]
    public class PlayerTime
    {
        public string playerName;
        public float time;
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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadRanking();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayerWins(float time)
    {
        finalGameTime = time;
        Debug.Log("Tempo final salvo: " + FormatTime(finalGameTime));

        // Adicionar ao ranking
        AddTimeToRanking("Player", finalGameTime);

        // Carregar cena de vitória
        SceneManager.LoadScene("Win");
    }

    public void LoadRankingScene()
    {
        SceneManager.LoadScene("Ranking");
    }

    public float GetFinalTime()
    {
        return finalGameTime;
    }

    // ========== SISTEMA DE RANKING ========== //
    private void LoadRanking()
    {
        if (PlayerPrefs.HasKey(RANKING_KEY))
        {
            string json = PlayerPrefs.GetString(RANKING_KEY);
            RankingData data = JsonUtility.FromJson<RankingData>(json);
            ranking = data.times ?? new List<PlayerTime>();
        }
    }

    private void SaveRanking()
    {
        RankingData data = new RankingData { times = ranking };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(RANKING_KEY, json);
        PlayerPrefs.Save();
    }

    public void AddTimeToRanking(string playerName, float time)
    {
        string formattedTime = FormatTime(time);
        PlayerTime newTime = new PlayerTime(playerName, time, formattedTime);
        ranking.Add(newTime);

        ranking = ranking.OrderBy(x => x.time).ToList();

        if (ranking.Count > 10)
        {
            ranking = ranking.Take(10).ToList();
        }

        SaveRanking();
    }

    public bool IsNewRecord(float time)
    {
        return ranking.Count == 0 || time < ranking[0].time;
    }

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

    public List<PlayerTime> GetTopTimes(int count = 10)
    {
        return ranking.Take(count).ToList();
    }

    public void ClearRanking()
    {
        ranking.Clear();
        PlayerPrefs.DeleteKey(RANKING_KEY);
        Debug.Log("Ranking limpo!");
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = (int)timeInSeconds / 60;
        int seconds = (int)timeInSeconds % 60;
        int milliseconds = (int)(timeInSeconds * 100) % 100;
        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }

    [System.Serializable]
    private class RankingData
    {
        public List<PlayerTime> times = new List<PlayerTime>();
    }
}