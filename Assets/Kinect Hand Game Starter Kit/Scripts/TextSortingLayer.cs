using UnityEngine;
using System.Collections;

/// <summary>
/// This class must be attached to a component with Text Mesh for it to be seen in front of all sprites which are in 0 and 1 sorting order
/// </summary>
public class TextSortingLayer : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
    {
        GetComponent<Renderer>().sortingOrder = 2;
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
