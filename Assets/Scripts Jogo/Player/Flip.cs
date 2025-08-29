using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private bool isFacingLeft = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Check if enemy needs to face left (example: based on player position)
        if (ShouldFaceLeft() && !isFacingLeft)
        {
            FaceLeft();
        }
        else if (!ShouldFaceLeft() && isFacingLeft)
        {
            FaceRight();
        }
    }

    bool ShouldFaceLeft()
    {
        // Example: face left when player is to the left
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return player.transform.position.x < transform.position.x;
        }
        return false;
    }

    void FaceLeft()
    {
        spriteRenderer.flipX = true;
        isFacingLeft = true;
    }

    void FaceRight()
    {
        spriteRenderer.flipX = false;
        isFacingLeft = false;
    }
}