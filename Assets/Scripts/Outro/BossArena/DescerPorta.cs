using UnityEngine;

public class DescerPorta : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float posicaoYNormal = 0f;
    [SerializeField] private float posicaoYArena = -4f;
    [SerializeField] private float velocidadeMovimento = 5f;
    
    private bool emTransicao = false;
    private float posicaoAlvoY;
    private Vector3 posicaoOriginal;

    void Start()
    {
        // Guarda a posição original
        posicaoOriginal = transform.position;
        posicaoAlvoY = posicaoYNormal;
        
        // Encontra e assina o evento do TriggerDetector
        TriggerDetector trigger = FindObjectOfType<TriggerDetector>();
        if (trigger != null)
        {
            trigger.OnTouchStateChanged += OnTouchStateChanged;
        }
    }

    void Update()
    {
        if (emTransicao)
        {
            // Move suavemente para a posição alvo em Y
            Vector3 novaPosicao = transform.position;
            novaPosicao.y = Mathf.Lerp(novaPosicao.y, posicaoAlvoY, velocidadeMovimento * Time.deltaTime);
            transform.position = novaPosicao;
            
            // Verifica se chegou perto o suficiente do alvo
            if (Mathf.Abs(transform.position.y - posicaoAlvoY) < 0.01f)
            {
                novaPosicao.y = posicaoAlvoY;
                transform.position = novaPosicao;
                emTransicao = false;
            }
        }
    }

    private void OnTouchStateChanged(bool estaTocando)
    {
        // Define a posição alvo baseado no estado
        posicaoAlvoY = estaTocando ? posicaoYArena : posicaoYNormal;
        emTransicao = true;
        
        Debug.Log($"🏃 Movendo objeto para Y: {posicaoAlvoY} (Entrou na arena: {estaTocando})");
    }

    void OnDestroy()
    {
        // Limpeza: remove a assinatura do evento
        TriggerDetector trigger = FindObjectOfType<TriggerDetector>();
        if (trigger != null)
        {
            trigger.OnTouchStateChanged -= OnTouchStateChanged;
        }
    }
}