using UnityEngine;
using Unity.Cinemachine;

public class MoveCamera : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform alvoPrincipal;
    [SerializeField] private Transform alvoSecundario;
    
    [Header("Configurações Gerais")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -5);
    [SerializeField] private float distanciaMinima = 0.1f;
    
    [Header("Suavização - Principal → Secundário")]
    [SerializeField] private float velocidadeParaSecundario = 8f;
    
    [Header("Suavização - Secundário → Principal")]
    [SerializeField] private float velocidadeParaPrincipal = 3f;
    
    [Header("Configurações Orthographic Size")]
    [SerializeField] private float tamanhoNormal = 5f;
    [SerializeField] private float tamanhoZoom = 8f;
    [SerializeField] private float velocidadeZoomParaSecundario = 3f;
    [SerializeField] private float velocidadeZoomParaPrincipal = 2f;
    
    private Transform alvoAtual;
    private bool emTransicao = false;
    private float velocidadeAtual;
    private CinemachineCamera cinemachineCamera;
    private float tamanhoAlvo;
    private float tamanhoAtual;
    private float velocidadeZoomAtual;

    void Start()
    {
        alvoAtual = alvoPrincipal;
        tamanhoAlvo = tamanhoNormal;
        velocidadeZoomAtual = velocidadeZoomParaPrincipal;
        
        // Encontra a CinemachineCamera
        cinemachineCamera = FindObjectOfType<CinemachineCamera>();
        if (cinemachineCamera != null)
        {
            tamanhoAtual = cinemachineCamera.Lens.OrthographicSize;
        }
        
        TriggerDetector trigger = FindObjectOfType<TriggerDetector>();
        if (trigger != null)
        {
            trigger.OnTouchStateChanged += OnTouchStateChanged;
        }
    }

    void LateUpdate()
    {
        if (alvoAtual != null)
        {
            Vector3 posicaoDesejada = alvoAtual.position + offset;
            
            if (emTransicao)
            {
                transform.position = Vector3.Lerp(transform.position, posicaoDesejada, velocidadeAtual * Time.deltaTime);
                
                if (Vector3.Distance(transform.position, posicaoDesejada) <= distanciaMinima)
                {
                    transform.position = posicaoDesejada;
                    emTransicao = false;
                }
            }
            else
            {
                transform.position = posicaoDesejada;
            }
        }
        
        // Atualiza o Orthographic Size suavemente
        AtualizarOrthographicSize();
    }

    private void OnTouchStateChanged(bool estaTocando)
    {
        Transform novoAlvo = estaTocando ? alvoSecundario : alvoPrincipal;
        
        if (novoAlvo != alvoAtual && novoAlvo != null)
        {
            // Determina a direção da transição para escolher a velocidade
            bool indoParaSecundario = (novoAlvo == alvoSecundario);
            velocidadeAtual = indoParaSecundario ? velocidadeParaSecundario : velocidadeParaPrincipal;
            
            // Define o tamanho alvo e velocidade do zoom baseado na direção da transição
            tamanhoAlvo = indoParaSecundario ? tamanhoZoom : tamanhoNormal;
            velocidadeZoomAtual = indoParaSecundario ? velocidadeZoomParaSecundario : velocidadeZoomParaPrincipal;
            
            alvoAtual = novoAlvo;
            emTransicao = true;
            
            Debug.Log($"🎥 Transição: {(indoParaSecundario ? "Principal → Secundário" : "Secundário → Principal")} " +
                      $"(Velocidade: {velocidadeAtual}, Zoom: {velocidadeZoomAtual}, Orthographic Size: {tamanhoAlvo})");
        }
    }

    private void AtualizarOrthographicSize()
    {
        if (cinemachineCamera != null)
        {
            // Interpola suavemente para o tamanho alvo com a velocidade atual
            tamanhoAtual = Mathf.Lerp(tamanhoAtual, tamanhoAlvo, velocidadeZoomAtual * Time.deltaTime);
            
            // Aplica o novo tamanho no Lens da CinemachineCamera
            var lens = cinemachineCamera.Lens;
            lens.OrthographicSize = tamanhoAtual;
            cinemachineCamera.Lens = lens;
        }
    }

    void OnDestroy()
    {
        TriggerDetector trigger = FindObjectOfType<TriggerDetector>();
        if (trigger != null)
        {
            trigger.OnTouchStateChanged -= OnTouchStateChanged;
        }
    }
}