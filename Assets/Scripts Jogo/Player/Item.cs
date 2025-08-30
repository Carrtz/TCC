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

    public void Drop()
    {
        IsPickedUp = false;
        HasKey = false;
        transform.SetParent(null);
        GetComponent<Collider2D>().enabled = true;

        // Opcional: reativar renderizador
        GetComponent<SpriteRenderer>().enabled = true;

        Debug.Log("Chave largada!");
    }

    // Método para destruir a chave
    public void ConsumeKey()
    {
        if (IsPickedUp)
        {
            HasKey = false;
            Destroy(gameObject);
            Debug.Log("Chave foi usada e destruída!");
        }
    }
}