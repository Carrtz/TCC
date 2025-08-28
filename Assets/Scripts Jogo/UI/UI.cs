using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    [SerializeField] Button ButtonPlay;
    [SerializeField] Button ButtonOptions;
    [SerializeField] Button Exit;
    void Awake()
    {
        ButtonPlay.onClick.AddListener(OnButtonPlay);
        Exit.onClick.AddListener(OnButtonExit);
        ButtonOptions.onClick.AddListener(OnButtonOptions);
    }

    private void OnButtonPlay()
    {
        SceneManager.LoadScene("Tutorial");
    }
    private void OnButtonOptions()
    {
        Debug.Log("Options");
    }
    private void OnButtonExit()
    {
        Application.Quit();
    }
}
