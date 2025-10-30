using UnityEngine;

public class BossArenaTrigger : MonoBehaviour
{
    [Header("Referências")]
    public BossController bossController;
    public GameObject arenaWalls;

    private bool fightStarted = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!fightStarted && other.CompareTag("Player"))
        {
            fightStarted = true;
            
            if (bossController != null)
            {
                bossController.StartBossFight();
            }

            if (arenaWalls != null)
            {
                arenaWalls.SetActive(true);
            }

            Debug.Log("Player entrou na arena! Luta iniciada.");
            GetComponent<Collider2D>().enabled = false;
        }
    }
}