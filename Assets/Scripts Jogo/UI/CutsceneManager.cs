using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CutsceneManager : MonoBehaviour
{
    [Header("Configurações da Cutscene")]
    public Sprite[] cutsceneImages;
    public float fadeDuration = 0.5f;
    public float autoLoadDelay = 5f;

    [Header("Referências UI")]
    public Image displayImage;
    public Button nextButton;
    public Button previousButton;
    public Button skipButton;
    public Text pageCounterText;

    [Header("Tela de Loading com Vídeo")]
    public GameObject loadingScreen;
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;
    public AudioSource videoAudio;
    public GameObject skipLoadingButton;

    private int currentImageIndex = 0;
    private bool isTransitioning = false;
    private Coroutine autoLoadCoroutine;
    private AsyncOperation loadingOperation;
    private bool isAutoLoading = false; // Nova flag para controlar o auto-load

    void Start()
    {
        FindReferences();

        // Esconde a tela de loading inicialmente
        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        // Configura botões
        if (nextButton != null)
            nextButton.onClick.AddListener(NextImage);
        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousImage);
        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipButtonClicked);

        // Configura o video player
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }

        StartCutscene();
    }

    // NOVO MÉTODO: Chamado quando o botão de pular é clicado
    public void OnSkipButtonClicked()
    {
        // Cancela o auto-load se estiver ativo
        if (isAutoLoading)
        {
            CancelAutoLoad();
        }

        StartVideoLoading();
    }

    // NOVO MÉTODO: Cancela o carregamento automático
    private void CancelAutoLoad()
    {
        if (autoLoadCoroutine != null)
        {
            StopCoroutine(autoLoadCoroutine);
            autoLoadCoroutine = null;
        }
        isAutoLoading = false;
        Debug.Log("Auto-load cancelado pelo jogador");
    }

    public void StartVideoLoading()
    {
        // Para o carregamento automático se estiver rodando
        CancelAutoLoad();

        // Mostra a tela de loading
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        // Esconde os botões da cutscene
        SetCutsceneUIVisible(false);

        // Prepara e inicia o vídeo
        StartCoroutine(PrepareAndPlayVideo());
    }

    // NOVO MÉTODO: Controla a visibilidade da UI da cutscene
    private void SetCutsceneUIVisible(bool visible)
    {
        if (nextButton != null) nextButton.gameObject.SetActive(visible);
        if (previousButton != null) previousButton.gameObject.SetActive(visible);
        if (skipButton != null) skipButton.gameObject.SetActive(visible);
        if (pageCounterText != null) pageCounterText.gameObject.SetActive(visible);
    }

    IEnumerator PrepareAndPlayVideo()
    {
        if (videoPlayer != null)
        {
            // Prepara o vídeo
            videoPlayer.Prepare();

            // Espera o vídeo terminar de preparar
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }

            // Inicia o vídeo
            videoPlayer.Play();

            // Inicia o carregamento da cena em background
            loadingOperation = SceneManager.LoadSceneAsync("TutorialFinal");
            loadingOperation.allowSceneActivation = false;

            // Mostra botão de pular loading após alguns segundos
            if (skipLoadingButton != null)
            {
                StartCoroutine(ShowSkipButtonAfterDelay(3f));
            }
        }
    }

    void OnVideoPrepared(VideoPlayer source)
    {
        // Vídeo está pronto para tocar
        Debug.Log("Vídeo preparado e pronto");
    }

    void OnVideoFinished(VideoPlayer source)
    {
        // Quando o vídeo termina, ativa a cena
        if (loadingOperation != null)
        {
            loadingOperation.allowSceneActivation = true;
        }
    }

    IEnumerator ShowSkipButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (skipLoadingButton != null)
        {
            skipLoadingButton.SetActive(true);
        }
    }

    // Método para pular o vídeo e ir direto para o jogo
    public void SkipVideoLoading()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        if (loadingOperation != null)
        {
            loadingOperation.allowSceneActivation = true;
        }
        else
        {
            // Se não houver operação de loading, carrega normalmente
            SceneManager.LoadScene("TutorialFinal");
        }
    }

    void FindReferences()
    {
        // Tenta encontrar automaticamente as referências se não estiverem atribuídas
        if (displayImage == null)
            displayImage = GetComponentInChildren<Image>();

        if (nextButton == null)
        {
            GameObject nextBtn = GameObject.Find("NextButton");
            if (nextBtn != null) nextButton = nextBtn.GetComponent<Button>();
        }

        if (previousButton == null)
        {
            GameObject prevBtn = GameObject.Find("PreviousButton");
            if (prevBtn != null) previousButton = prevBtn.GetComponent<Button>();
        }

        if (skipButton == null)
        {
            GameObject skipBtn = GameObject.Find("SkipButton");
            if (skipBtn != null) skipButton = skipBtn.GetComponent<Button>();
        }

        if (pageCounterText == null)
        {
            GameObject counter = GameObject.Find("PageCounter");
            if (counter != null) pageCounterText = counter.GetComponent<Text>();
        }
    }

    void StartCutscene()
    {
        if (cutsceneImages != null && cutsceneImages.Length > 0 && displayImage != null)
        {
            displayImage.sprite = cutsceneImages[0];
            displayImage.color = new Color(1, 1, 1, 1);
            UpdateUI();
        }
        else
        {
            Debug.LogError("Cutscene images não configuradas ou displayImage não encontrado!");
        }
    }

    public void NextImage()
    {
        if (isTransitioning || cutsceneImages == null || currentImageIndex >= cutsceneImages.Length - 1)
        {
            // Se não há mais imagens, vai direto para o tutorial
            if (currentImageIndex >= cutsceneImages.Length - 1)
            {
                // Cancela o auto-load se o jogador clicou manualmente
                if (isAutoLoading)
                {
                    CancelAutoLoad();
                }
                StartVideoLoading();
            }
            return;
        }

        // Cancela o auto-load se estiver ativo
        if (isAutoLoading)
        {
            CancelAutoLoad();
        }

        StartCoroutine(TransitionToImage(currentImageIndex + 1));
    }

    public void PreviousImage()
    {
        if (isTransitioning || cutsceneImages == null || currentImageIndex <= 0)
            return;

        // Cancela o auto-load se estiver ativo
        if (isAutoLoading)
        {
            CancelAutoLoad();
        }

        StartCoroutine(TransitionToImage(currentImageIndex - 1));
    }

    IEnumerator TransitionToImage(int newIndex)
    {
        isTransitioning = true;

        // Fade out da imagem atual
        yield return StartCoroutine(FadeOut());

        // Troca para a nova imagem
        currentImageIndex = newIndex;
        if (displayImage != null && cutsceneImages != null)
            displayImage.sprite = cutsceneImages[currentImageIndex];

        // Fade in da nova imagem
        yield return StartCoroutine(FadeIn());

        // Atualiza a UI
        UpdateUI();

        // Se for a última imagem, inicia o carregamento automático
        if (currentImageIndex == cutsceneImages.Length - 1)
        {
            autoLoadCoroutine = StartCoroutine(AutoLoadTutorial());
        }

        isTransitioning = false;
    }

    IEnumerator AutoLoadTutorial()
    {
        isAutoLoading = true; // Marca que o auto-load está ativo
        Debug.Log("Auto-load iniciado, aguardando " + autoLoadDelay + " segundos");

        yield return new WaitForSeconds(autoLoadDelay);

        // Só executa se ainda estiver no modo auto-load (não foi cancelado)
        if (isAutoLoading)
        {
            Debug.Log("Auto-load concluído, iniciando loading...");
            StartVideoLoading();
        }
    }

    IEnumerator FadeIn()
    {
        if (displayImage == null) yield break;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            displayImage.color = new Color(1, 1, 1, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        displayImage.color = new Color(1, 1, 1, 1);
    }

    IEnumerator FadeOut()
    {
        if (displayImage == null) yield break;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            displayImage.color = new Color(1, 1, 1, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        displayImage.color = new Color(1, 1, 1, 0);
    }

    void UpdateUI()
    {
        // Atualiza o contador de páginas se existir
        if (pageCounterText != null && cutsceneImages != null)
        {
            pageCounterText.text = $"{currentImageIndex + 1}/{cutsceneImages.Length}";
        }

        // Habilita/desabilita botões conforme a posição
        if (previousButton != null)
            previousButton.interactable = currentImageIndex > 0;

        if (nextButton != null)
        {
            nextButton.interactable = currentImageIndex < cutsceneImages.Length - 1;

            // Muda o texto do último botão para "Iniciar Jogo"
            Text nextButtonText = nextButton.GetComponentInChildren<Text>();
            if (nextButtonText != null)
            {
                nextButtonText.text = (currentImageIndex == cutsceneImages.Length - 1) ?
                    "Iniciar Jogo" : "Avançar";
            }
        }
    }

    // Método para avançar ou iniciar o jogo na última imagem
    public void HandleNextAction()
    {
        if (cutsceneImages != null && currentImageIndex == cutsceneImages.Length - 1)
        {
            // Cancela o auto-load se estiver ativo
            if (isAutoLoading)
            {
                CancelAutoLoad();
            }
            StartVideoLoading();
        }
        else
        {
            NextImage();
        }
    }

    // Método para pular para o jogo
    public void SkipCutscene()
    {
        // Cancela o auto-load se estiver ativo
        if (isAutoLoading)
        {
            CancelAutoLoad();
        }

        StartVideoLoading();
    }

    // Input por teclado
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            NextImage();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            PreviousImage();
        }
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            HandleNextAction();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            SkipCutscene();
        }
    }

    void OnDestroy()
    {
        // Limpa os event handlers para evitar memory leaks
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }
}