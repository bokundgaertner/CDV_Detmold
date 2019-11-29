using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlombergMapAnimation : MonoBehaviour
{
    private RectTransform myRect;
    public float scrollSpeed;

    /// <summary>
    /// Wenn MonoBehaviour aktiviert wird, wird das Kartenscrollen gestartet
    /// </summary>
    void OnEnable()
    {
        myRect = this.GetComponent<RectTransform>();
        Debug.Log(myRect.position);
        StartCoroutine(Animate());
    }

    /// <summary>
    /// Wenn MonoBehaviour deaktiviert wird, wird das Kartenscrollen beendet
    /// </summary>
    void OnDisable()
    {
        StopAllCoroutines();
        myRect.position = new Vector3(0f, 1220f, 0f);
    }


    /// <summary>
    /// Coroutine dient als Animation
    /// </summary>
    /// <returns></returns>
    private IEnumerator Animate()
    {
        while(this.GetComponent<RectTransform>().position.x > -3360f)
        {
            yield return new WaitForEndOfFrame();
            myRect.position += Vector3.left * Time.deltaTime * scrollSpeed;
        }
    }
}
