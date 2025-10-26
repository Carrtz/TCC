using UnityEngine;

public class TriggerDetector : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private GameObject targetObject; // Objeto específico para detectar
    [SerializeField] private string targetTag; // Ou use tag como alternativa
    
    [Header("Debug")]
    [SerializeField] private bool isTouching = false;
    public System.Action<bool> OnTouchStateChanged;
    
    void Update()
    {
        if (isTouching)
        {
            WhileTouching();
        }
        else
        {
            WhileNotTouching();
        }
    }
    
    // MUDADO: Collider → Collider2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsTargetObject(other.gameObject))
        {
            isTouching = true;
            OnStartTouching();
            OnTouchStateChanged?.Invoke(true);
        }
    }
    
    // MUDADO: Collider → Collider2D
    private void OnTriggerStay2D(Collider2D other)
    {
        if (IsTargetObject(other.gameObject))
        {
            // Garante que está marcado como tocando
            if (!isTouching)
            {
                isTouching = true;
                OnStartTouching();
                OnTouchStateChanged?.Invoke(true);
            }
        }
    }
    
    // MUDADO: Collider → Collider2D
    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsTargetObject(other.gameObject))
        {
            isTouching = false;
            OnStopTouching();
            OnTouchStateChanged?.Invoke(false);
        }
    }
    
    // Método para verificar se é o objeto alvo
    private bool IsTargetObject(GameObject obj)
    {
        // Se tem targetObject definido, verifica por referência
        if (targetObject != null)
        {
            return obj == targetObject;
        }
        // Se não tem targetObject, verifica por tag
        else if (!string.IsNullOrEmpty(targetTag))
        {
            return obj.CompareTag(targetTag);
        }
        
        // Se não tem nenhum critério definido, não detecta nada
        return false;
    }
    
    // Chamado quando começa a tocar
    private void OnStartTouching()
    {
        string objectName = GetTargetName();
        Debug.Log($"🎯 Começou a tocar no objeto: {objectName}");
    }
    
    // Chamado enquanto está tocando (roda a cada frame)
    private void WhileTouching()
    {
        // Debug.Log("📌 Continuando contato...");
    }
    
    // Chamado quando para de tocar
    private void OnStopTouching()
    {
        string objectName = GetTargetName();
        Debug.Log($"🚫 Parou de tocar no objeto: {objectName}");
    }
    
    // Chamado enquanto NÃO está tocando (roda a cada frame)
    private void WhileNotTouching()
    {
        // Debug.Log("⏳ Aguardando contato...");
    }
    
    // Método auxiliar para pegar o nome do alvo
    private string GetTargetName()
    {
        if (targetObject != null)
            return targetObject.name;
        else if (!string.IsNullOrEmpty(targetTag))
            return "Objeto com tag: " + targetTag;
        else
            return "Nenhum alvo definido";
    }
    
    // Método público para verificar estado
    public bool IsCurrentlyTouching()
    {
        return isTouching;
    }
}