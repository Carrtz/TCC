using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenDoor : MonoBehaviour
{
    public float interactionDistance = 2f; // Distância máxima para interagir com a porta
    public KeyCode interactionKey = KeyCode.G; // Tecla para interagir

    private Transform player;
    private TimerManager timer; // Referência para o script Timer

    void Start()
    {
        // Encontrar o jogador pela tag
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Tentar encontrar o script Timer na cena
        timer = FindObjectOfType<TimerManager>();
    }

    void Update()
    {
        // Verificar se o jogador está perto o suficiente e pressionou a tecla
        if (Vector2.Distance(transform.position, player.position) <= interactionDistance &&
            Input.GetKeyDown(interactionKey))
        {
            TryOpenDoor();
        }
    }

    void TryOpenDoor()
    {
        if (Item.HasKey)
        {
            // Salvar o tempo final antes de carregar a cena
            SaveFinalTime();

            // Carregar a cena de vitória
            SceneManager.LoadScene("Win");
        }
        else
        {
            Debug.Log("Você precisa da chave para abrir esta porta!");
        }
    }

    void SaveFinalTime()
    {
        // Se encontrou o timer, salvar o tempo final no GameManager
        if (timer != null)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerWins(timer.GetCurrentTime());
            }
            else
            {
                Debug.LogWarning("GameManager não encontrado. Criando um temporário...");

                // Criar um GameManager temporário se não existir
                GameObject gmObject = new GameObject("TempGameManager");
                GameManager tempGM = gmObject.AddComponent<GameManager>();
                tempGM.PlayerWins(timer.GetCurrentTime());
                DontDestroyOnLoad(gmObject);
            }
        }
        else
        {
            Debug.LogWarning("Timer não encontrado na cena. Não foi possível salvar o tempo final.");
        }
    }

    // Opcional: Desenhar gizmo para visualizar a distância de interação
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}