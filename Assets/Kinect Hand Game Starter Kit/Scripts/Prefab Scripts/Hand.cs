using UnityEngine;
using System.Collections;

public class Hand : MonoBehaviour 
{
    public int BallCount = 0;

    private Animator animator;

    private Collider2D currentCollision;

	// Use this for initialization
	void Start () 
    {
        animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () 
    {
	    
	}

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Ball")
        {
            currentCollision = col;
            animator.SetBool("Hover", true);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.tag == "Ball")
        {
            currentCollision = null;
            animator.SetBool("Hover", false);
        }
    }

    /// <summary>
    /// AnimationEvent on the last frame of animation
    /// </summary>
    public void DestroyBall()
    {
        if (currentCollision != null)
        {
            Destroy(currentCollision.gameObject);
            animator.SetBool("Hover", false);
        }
    }
}
