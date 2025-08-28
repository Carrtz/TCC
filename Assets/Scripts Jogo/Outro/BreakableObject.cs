using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    [SerializeField] private int hitsToBreak = 3;
    private int currentHits = 0;

    public void TakeDamage(int damage)
    {
        currentHits += damage;
        Debug.Log($"Parede atingida! Hits: {currentHits}/{hitsToBreak}");

        if (currentHits >= hitsToBreak)
        {
            DestroyWall();
        }
    }

    private void DestroyWall()
    {

        Destroy(gameObject);
    }
}