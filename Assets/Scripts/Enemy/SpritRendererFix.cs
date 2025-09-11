using UnityEngine;

public class SpritRendererFix : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // No PlayerAnimator ou em um script separado
    [SerializeField] private SpriteRenderer _spriteRenderer;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        // Garanta uma ordem de renderização adequada
        _spriteRenderer.sortingOrder = 10;
    }

    // Ou configure no Inspector:
    // - Sorting Layer: Defina para um layer apropriado
    // - Order in Layer: Defina um valor positivo (ex: 10)
}
