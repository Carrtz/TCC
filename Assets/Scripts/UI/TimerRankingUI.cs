using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Adicione esta linha
using TMPro;

public class TimeRankingUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform rankingContainer;
    public GameObject rankingEntryPrefab;
    public int maxEntries = 10;

    private TimeRankingSystem rankingSystem;

    void Start()
    {
        rankingSystem = FindObjectOfType<TimeRankingSystem>();
        if (rankingSystem != null)
        {
            UpdateRankingUI();
        }
        else
        {
            Debug.LogWarning("TimeRankingSystem não encontrado!");
        }
    }

    public void UpdateRankingUI()
    {
        // Limpar entradas existentes
        foreach (Transform child in rankingContainer)
        {
            Destroy(child.gameObject);
        }

        // Obter top tempos
        List<TimeRankingSystem.PlayerTime> topTimes = rankingSystem.GetTopTimes(maxEntries);

        // Criar entradas de UI
        for (int i = 0; i < topTimes.Count; i++)
        {
            GameObject entry = Instantiate(rankingEntryPrefab, rankingContainer);
            SetupRankingEntry(entry, i + 1, topTimes[i]);
        }
    }

    private void SetupRankingEntry(GameObject entry, int position, TimeRankingSystem.PlayerTime timeData)
    {
        TextMeshProUGUI[] texts = entry.GetComponentsInChildren<TextMeshProUGUI>();

        if (texts.Length >= 3)
        {
            texts[0].text = $"{position}º";
            texts[1].text = timeData.playerName;
            texts[2].text = timeData.formattedTime;
        }

        // Destacar posições - com verificação de null
        Image entryImage = entry.GetComponent<Image>();
        if (entryImage != null)
        {
            if (position == 1)
            {
                entryImage.color = new Color(1f, 0.9f, 0f, 0.3f); // Ouro
            }
            else if (position == 2)
            {
                entryImage.color = new Color(0.8f, 0.8f, 0.8f, 0.3f); // Prata
            }
            else if (position == 3)
            {
                entryImage.color = new Color(0.8f, 0.5f, 0.2f, 0.3f); // Bronze
            }
        }
    }

    // Método para atualizar o ranking manualmente
    public void RefreshRanking()
    {
        UpdateRankingUI();
    }
}