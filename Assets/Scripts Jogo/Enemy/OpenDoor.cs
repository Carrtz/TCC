using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenDoor : MonoBehaviour
{
    public float interactionDistance = 2f; // Dist�ncia m�xima para interagir com a porta
    public KeyCode interactionKey = KeyCode.G; // Tecla para interagir

    private Transform player;
    private TimerManager timer; // Refer�ncia para o script Timer

    void Start()
    {
        // Encontrar o jogador pela tag
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Tentar encontrar o script Timer na cena
        timer = FindObjectOfType<TimerManager>();
    }

    void Update()
    {
        // Verificar se o jogador est� perto o suficiente e pressionou a tecla
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

            // Carregar a cena de vit�ria
            SceneManager.LoadScene("Win");
        }
        else
        {
            Debug.Log("Voc� precisa da chave para abrir esta porta!");
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
                Debug.LogWarning("GameManager n�o encontrado. Criando um tempor�rio...");

                // Criar um GameManager tempor�rio se n�o existir
                GameObject gmObject = new GameObject("TempGameManager");
                GameManager tempGM = gmObject.AddComponent<GameManager>();
                tempGM.PlayerWins(timer.GetCurrentTime());
                DontDestroyOnLoad(gmObject);
            }
        }
        else
        {
            Debug.LogWarning("Timer n�o encontrado na cena. N�o foi poss�vel salvar o tempo final.");
        }
    }

    // Opcional: Desenhar gizmo para visualizar a dist�ncia de intera��o
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}