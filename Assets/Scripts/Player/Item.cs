using UnityEngine;

public class Item : MonoBehaviour
{
    public bool IsPickedUp { get; private set; } = false;
    public static bool HasKey { get; private set; } = false;

    public void PickUp(Transform holder)
    {
        IsPickedUp = true;
        HasKey = true;
        transform.SetParent(holder);
        transform.localPosition = Vector3.zero;
        GetComponent<Collider2D>().enabled = false;

        // Opcional: desativar renderizador para fazer a chave "desaparecer"
        GetComponent<SpriteRenderer>().enabled = false;

        Debug.Log("Chave coletada!");
    }

    // M�todo para destruir a chave
    public void ConsumeKey()
    {
        if (IsPickedUp)
        {
            HasKey = false;
            Destroy(gameObject);
            Debug.Log("Chave foi usada e destru�da!");
        }
    }
}