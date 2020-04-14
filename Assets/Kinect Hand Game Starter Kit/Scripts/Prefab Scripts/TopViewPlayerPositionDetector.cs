using UnityEngine;
using System.Collections;

public class TopViewPlayerPositionDetector : MonoBehaviour 
{
    public Vector3 hipCenterPosition = new Vector3(-10, -10, -10); // x, y are ranged in [-1; 1]; z is distance to sensor in meters

    /// <summary>
    /// Sprite inside to indicate player position from the top
    /// </summary>
    private Transform PlayerIco;

    /// <summary>
    /// All object that want to recieve messages from this class
    /// </summary>
    public GameObject[] Listeners;

    private Animator animator;

    private const float minPlayerZPos = 1; // in meters
    private const float maxPlayerZPos = 3; // in meters

	// Use this for initialization
	void Start () 
    {
        // PlayerIco is the only child of the prefab
        PlayerIco = gameObject.GetComponentsInChildren<Transform>()[1];

        if (PlayerIco == null)
        {
            Debug.Log("No player ico in the TopViewPlayerPositionDetector");
            Debug.Break();
        }

        animator = GetComponent<Animator>();
	}

    /// <summary>
    /// When player is located correctly
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.name == PlayerIco.name && animator != null)
        {
            animator.SetBool("Hover", true);
        }
    }

    /// <summary>
    /// When player is NOT located correctly
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.name == PlayerIco.name && animator != null)
        {
            animator.SetBool("Hover", false);
        }
        
        if (animator.GetBool("FirstTimeInitialized") == true)
        {
            foreach (GameObject l in Listeners)
            {
                l.SendMessage("ValidPlayerPosition", false, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    /// <summary>
    /// This function is an AnimationEvent, is called from the last frame of animation.
    /// This means that player is calibrated
    /// 
    /// </summary>
    public void HoverCompleted()
    {
        if (animator.GetBool("FirstTimeInitialized") == false)
        {
            foreach (GameObject l in Listeners)
            {
                l.SendMessage("FirstTimeInitialized", true, SendMessageOptions.DontRequireReceiver);
            }
        }

        animator.SetBool("FirstTimeInitialized", true);

        foreach (GameObject l in Listeners)
        {
            l.SendMessage("ValidPlayerPosition", true, SendMessageOptions.DontRequireReceiver);
        }
    }
    
    /// <summary>
    /// HipCenterPosition x, y are ranged in [-1; 1]; z is distance to sensor in meters
    /// </summary>
    /// <param name="hcp"></param>
    public void SetHipCenterPosition(Vector3 hcp)
    {
        hipCenterPosition = hcp;
    }

	// Update is called once per frame
	void Update () 
    {
        // if input data is correct we must move PlayerIco
        if (hipCenterPosition.x > -5f && hipCenterPosition.x < 5f) // if data is valid 
        {
            float x = hipCenterPosition.x;

            float y;
            if (hipCenterPosition.z < minPlayerZPos) y = 1f;
            else if (hipCenterPosition.z > maxPlayerZPos) y = -1f;
            else y = (-1) * (hipCenterPosition.z) * (2.0f / (maxPlayerZPos - minPlayerZPos)) + (maxPlayerZPos + minPlayerZPos) / (maxPlayerZPos - minPlayerZPos);

            PlayerIco.transform.localPosition = new Vector3(GetComponent<SpriteRenderer>().sprite.bounds.size.x / 2.0f * x, GetComponent<SpriteRenderer>().sprite.bounds.size.y / 2.0f * y);
        }
	}
    
}
