using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextProximity : MonoBehaviour
{
    private GameObject player;
    public Transform Area;
    private Rigidbody2D rb;
    public GameObject Text;
    public float fadeDuration = 1f;

    private Coroutine fadeCoroutine;
    private CanvasGroup textCanvasGroup;
    private bool isTextActive = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();

        // Garante que o texto tenha um CanvasGroup para controlar a transparência
        if (Text != null)
        {
            textCanvasGroup = Text.GetComponent<CanvasGroup>();
            if (textCanvasGroup == null)
            {
                textCanvasGroup = Text.AddComponent<CanvasGroup>();
            }

            // Começa com alpha 0 e desativa o GameObject
            textCanvasGroup.alpha = 0f;
            Text.SetActive(false);
            isTextActive = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Ativa o GameObject antes de fazer o fade in
            if (!isTextActive)
            {
                Text.SetActive(true);
                isTextActive = true;
            }

            // Para qualquer fade em andamento
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeText(0f, 1f)); // Fade in
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Para qualquer fade em andamento
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeText(1f, 0f)); // Fade out
        }
    }

    private IEnumerator FadeText(float startAlpha, float targetAlpha)
    {
        if (textCanvasGroup == null) yield break;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            textCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        textCanvasGroup.alpha = targetAlpha;

        // Se fez fade out completo, desativa o GameObject
        if (targetAlpha == 0f)
        {
            Text.SetActive(false);
            isTextActive = false;
        }

        fadeCoroutine = null;
    }
}