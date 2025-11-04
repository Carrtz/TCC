using UnityEngine;

public class TriggerDetector : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private string targetTag;
    
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
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsTargetObject(other.gameObject))
        {
            isTouching = true;
            OnStartTouching();
            OnTouchStateChanged?.Invoke(true);
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (IsTargetObject(other.gameObject))
        {
            if (!isTouching)
            {
                isTouching = true;
                OnStartTouching();
                OnTouchStateChanged?.Invoke(true);
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsTargetObject(other.gameObject))
        {
            isTouching = false;
            OnStopTouching();
            OnTouchStateChanged?.Invoke(false);
        }
    }
    
    private bool IsTargetObject(GameObject obj)
    {
        if (targetObject != null)
        {
            return obj == targetObject;
        }
        else if (!string.IsNullOrEmpty(targetTag))
        {
            return obj.CompareTag(targetTag);
        }
        
        return false;
    }
    
    private void OnStartTouching()
    {
        string objectName = GetTargetName();
        Debug.Log($"Começou a tocar no objeto: {objectName}");
    }
    
    private void WhileTouching()
    {
    }
    
    private void OnStopTouching()
    {
        string objectName = GetTargetName();
        Debug.Log($"Parou de tocar no objeto: {objectName}");
    }
    
    private void WhileNotTouching()
    {
    }
    
    private string GetTargetName()
    {
        if (targetObject != null)
            return targetObject.name;
        else if (!string.IsNullOrEmpty(targetTag))
            return "Objeto com tag: " + targetTag;
        else
            return "Nenhum alvo definido";
    }
    
    public bool IsCurrentlyTouching()
    {
        return isTouching;
    }
}