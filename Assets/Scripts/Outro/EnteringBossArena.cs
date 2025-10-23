using UnityEngine;

public class EnteringBossArena : MonoBehaviour
{
    [Header("Configurações")]
    public Component cameraVirtual;
    public AreaDetector detector;
    public Transform alvo;
    
    void Start()
    {
        Debug.Log("✅ EnteringBossArena iniciado!");
        
        // Busca detector
        if (detector == null)
        {
            detector = FindObjectOfType<AreaDetector>();
            Debug.Log(detector != null ? "✅ Detector encontrado automaticamente!" : "❌ Nenhum detector encontrado!");
        }
        else
        {
            Debug.Log("✅ Detector atribuído manualmente!");
        }
        
        // Tenta conectar aos eventos
        if (detector != null)
        {
            detector.onObjetoEntrou.AddListener(QuandoObjetoEntrou);
            detector.onObjetoSaiu.AddListener(QuandoObjetoSaiu);
            Debug.Log("✅ Listeners conectados aos eventos!");
        }
        else
        {
            Debug.LogError("❌ Não foi possível conectar - detector é null!");
        }
        
        // Testa a câmera
        if (cameraVirtual != null)
        {
            Debug.Log("✅ Camera Virtual atribuída: " + cameraVirtual.name);
        }
        else
        {
            Debug.LogError("❌ Camera Virtual não atribuída!");
        }
    }
    
    private void QuandoObjetoEntrou()
    {
        Debug.Log("🎉 EVENTO RECEBIDO: QuandoObjetoEntrou() FOI CHAMADO!");
    }
    
    private void QuandoObjetoSaiu()
    {
        Debug.Log("🎉 EVENTO RECEBIDO: QuandoObjetoSaiu() FOI CHAMADO!");
    }
    
    void Update()
    {
        // Teste manual com tecla Espaço
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("🎮 Teste manual com Espaço!");
            QuandoObjetoEntrou();
        }
    }
}