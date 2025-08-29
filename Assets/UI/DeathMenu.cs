using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class DeathMenu : MonoBehaviour

{ 

    [SerializeField] Button Return;

    void Awake()
    {
        Return.onClick.AddListener(OnButtonReturn);
  
    }

    private void OnButtonReturn()
    {
        SceneManager.LoadScene("Menu");
    }
}
