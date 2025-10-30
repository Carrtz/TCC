using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class GlobalInputBlocker : MonoBehaviour
{
    public void BlockAllInputForSeconds(float seconds)
    {
        StartCoroutine(BlockInputCoroutine(seconds));
    }

    private IEnumerator BlockInputCoroutine(float seconds)
    {
        // Desabilita completamente o EventSystem
        if (EventSystem.current != null)
        {
            EventSystem.current.enabled = false;
        }

        yield return new WaitForSeconds(seconds);

        // Reabilita o EventSystem
        if (EventSystem.current != null)
        {
            EventSystem.current.enabled = true;
        }
    }
}