using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RankingManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform rankingContainer;
    public GameObject rankingEntryPrefab;
    public Button backButton;
    public Button clearButton;
    public TMP_Text noRecordsText;

    [Header("Colors")]
    public Color firstPlaceColor = new Color(1f, 0.8f, 0f, 1f);
    public Color secondPlaceColor = new Color(0.75f, 0.75f, 0.75f, 1f);
    public Color thirdPlaceColor = new Color(0.8f, 0.5f, 0.2f, 1f);
    public Color defaultColor = Color.white;

    void Start()
    {
        // Configurar botões
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBack);
        }

        if (clearButton != null)
        {
            clearButton.onClick.AddListener(ClearRanking);
        }

        // Carregar ranking
        LoadRankingUI();
    }

    public void LoadRankingUI()
    {
        // Limpar container
        foreach (Transform child in rankingContainer)
        {
            Destroy(child.gameObject);
        }

        if (GameManager.Instance != null)
        {
            List<GameManager.PlayerTime> topTimes = GameManager.Instance.GetTopTimes(10);

            if (topTimes.Count == 0)
            {
                if (noRecordsText != null)
                {
                    noRecordsText.gameObject.SetActive(true);
                    noRecordsText.text = "Nenhum recorde ainda!\nComplete o jogo para aparecer aqui.";
                }
                return;
            }

            if (noRecordsText != null)
            {
                noRecordsText.gameObject.SetActive(false);
            }

            // Criar entradas do ranking
            for (int i = 0; i < topTimes.Count; i++)
            {
                CreateRankingEntry(i + 1, topTimes[i]);
            }
        }
    }

    private void CreateRankingEntry(int position, GameManager.PlayerTime timeData)
    {
        GameObject entry = Instantiate(rankingEntryPrefab, rankingContainer);

        // Configurar textos
        TextMeshProUGUI[] texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length >= 3)
        {
            texts[0].text = $"{position}º";
            texts[1].text = timeData.playerName;
            texts[2].text = timeData.formattedTime;

            // Aplicar cores baseadas na posição
            Color textColor = GetPositionColor(position);
            foreach (var text in texts)
            {
                text.color = textColor;
            }
        }

        // Configurar fundo (opcional)
        Image background = entry.GetComponent<Image>();
        if (background != null)
        {
            background.color = GetBackgroundColor(position);
        }
    }

    private Color GetPositionColor(int position)
    {
        return position switch
        {
            1 => firstPlaceColor,
            2 => secondPlaceColor,
            3 => thirdPlaceColor,
            _ => defaultColor
        };
    }

    private Color GetBackgroundColor(int position)
    {
        return position switch
        {
            1 => new Color(1f, 0.9f, 0f, 0.1f),
            2 => new Color(0.8f, 0.8f, 0.8f, 0.1f),
            3 => new Color(0.8f, 0.5f, 0.2f, 0.1f),
            _ => new Color(0f, 0f, 0f, 0.05f)
        };
    }

    private void ClearRanking()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearRanking();
            LoadRankingUI(); // Recarregar UI
        }
    }

    private void GoBack()
    {
        SceneManager.LoadScene("Menu"); // Ou a cena anterior
    }

    // Para atualizar via botão
    public void RefreshRanking()
    {
        LoadRankingUI();
    }
}