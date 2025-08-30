using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    public float interactionDistance = 2f; // Dist�ncia m�xima para interagir com a porta
    public KeyCode interactionKey = KeyCode.G; // Tecla para interagir

    private Transform player;

    void Start()
    {
        // Encontrar o jogador pela tag (certifique-se de que seu jogador tem a tag "Player")
        player = GameObject.FindGameObjectWithTag("Player").transform;
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
            Debug.Log("Porta aberta!");
            Destroy(gameObject); // Destroi a porta
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