using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour 
{
    public GameObject[] Listeners;
    internal object onClick;

    /// <summary>
    /// When we tween buttons we do not want them to be clickable
    /// </summary>
    public void EnableClick()
    {
        this.GetComponent<BoxCollider2D>().enabled = true;
    }

    public void DisableClick()
    {
        this.GetComponent<BoxCollider2D>().enabled = false;
    }

    public void SetCaption(string caption)
    {
        gameObject.GetComponentInChildren<TextMesh>().text = caption;
    }

    void OnMouseDown()
    {
        foreach (GameObject l in Listeners)
        {
            l.SendMessage("ButtonPressed", name, SendMessageOptions.DontRequireReceiver);
        }
    }

    void DestroyMe()
    {
        Destroy(gameObject);
    }

}
