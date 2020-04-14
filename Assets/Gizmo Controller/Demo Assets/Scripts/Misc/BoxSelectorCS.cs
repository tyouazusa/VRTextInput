using UnityEngine;
using System.Collections;

public class BoxSelectorCS : MonoBehaviour {

	private Color NormalColor;
	private Color HilightColor;
	public bool OverBox = false;
	public GizmoControllerCS GC = null;
	
	
	void Start(){
		if(!GetComponent<Renderer>())
			return;
			
		NormalColor = GetComponent<Renderer>().material.color;
		HilightColor = new Color(NormalColor.r * 1.2f, NormalColor.g * 1.2f, NormalColor.b * 1.2f, 1.0f);
		GC = GameObject.Find("GizmoAdvancedCS").GetComponent<GizmoControllerCS>();
		
		if(GC == null){
			Debug.LogError (this + " Unable To Find Gizmo Advanced Controller. Please be sure one is in the scene.");	
		}
		else{
			GC.Hide();
		}
	}//Start
	
	void OnMouseEnter(){
		GetComponent<Renderer>().material.color = HilightColor;
		OverBox = true;
	}//OnMouseEnter
	
	void OnMouseExit(){
		GetComponent<Renderer>().material.color = NormalColor;
		OverBox = false;
	}//OnMouseEnter
	
	void OnMouseDown(){	
		if(GC == null)
			return;
			
		if(GC.IsOverAxis())
			return;
		
		GC.SetSelectedObject(transform);
		
		if(GC.IsHidden())
			GC.Show(GizmoControllerCS.GIZMO_MODE.TRANSLATE);
	}//OnMouseDown

}
