using UnityEngine;
using UnityEngine.Events;

public class AreaDetector : MonoBehaviour
{
    [Header("Configurações")]
    public string tagDoObjeto = "MeioDaCamera"; // Tag do objeto específico que ativa
    
    [Header("Eventos")]
    public UnityEvent onObjetoEntrou;
    public UnityEvent onObjetoSaiu;

    void Start()
    {
        Debug.Log("✅ AreaDetector iniciado! Tag procurada: " + tagDoObjeto);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("🎯 Trigger ENTER detectado com: " + other.name + " | Tag: " + other.tag);
        
        if (other.CompareTag(tagDoObjeto))
        {
            Debug.Log("✅ OBJETO 'MEIO DA CAMERA' ENTROU NA ÁREA! Disparando evento...");
            onObjetoEntrou?.Invoke();
        }
        else
        {
            Debug.Log("❌ Tag não coincide. Esperada: " + tagDoObjeto + ", Recebida: " + other.tag);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("🚪 Trigger EXIT detectado com: " + other.name);
        
        if (other.CompareTag(tagDoObjeto))
        {
            Debug.Log("✅ OBJETO 'MEIO DA CAMERA' SAIU DA ÁREA! Disparando evento...");
            onObjetoSaiu?.Invoke();
        }
    }
}