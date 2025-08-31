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
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Voc� precisa da chave para abrir esta porta!");
        }
    }

    // Opcional: Desenhar gizmo para visualizar a dist�ncia de intera��o
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}