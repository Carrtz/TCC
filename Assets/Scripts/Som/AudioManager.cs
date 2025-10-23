using UnityEngine;
using UnityEngine.UIElements;

public class AudioManager : MonoBehaviour
{
    [Header("------- Audio Source -------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("------- Audio Clip -------")]
    public AudioClip background;
    public AudioClip morte;
    public AudioClip WallTouch;
    public AudioClip Running;
    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }
}
