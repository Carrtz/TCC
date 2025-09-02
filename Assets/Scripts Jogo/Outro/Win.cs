using UnityEngine;
using UnityEngine.SceneManagement;

public class Win : MonoBehaviour
{
    private TimerManager timer;

    void Start()
    {
        // Tentar encontrar o script Timer na cena
        timer = FindObjectOfType<TimerManager>();
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerCollision(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerCollision(other.gameObject);
        }
    }
    
    private void HandlePlayerCollision(GameObject playerObject)
    {
        PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            SaveFinalTime();
            SceneManager.LoadScene("Win");
        }
    }
}