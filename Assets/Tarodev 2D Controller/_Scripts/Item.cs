using UnityEngine;

public class Item : MonoBehaviour
{
    public bool IsPickedUp { get; private set; } = false;

    public void PickUp(Transform holder)
    {
        IsPickedUp = true;
        transform.SetParent(holder); // Define o jogador como "pai" do item
        transform.localPosition = Vector3.zero; // Centraliza o item no jogador
        GetComponent<Collider2D>().enabled = false; // Desativa o colisor para evitar colisões indesejadas
    }

    public void Drop()
    {
        IsPickedUp = false;
        transform.SetParent(null); // Remove o "pai" do item
        GetComponent<Collider2D>().enabled = true; // Reativa o colisor
    }
}