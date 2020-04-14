using UnityEngine;
using System.Collections;

public class GizmoAxisHandleCS : MonoBehaviour {

	private Color NormalColor;
	private Color HoverColor;
	private Color DragColor;
	
	public enum AXIS_COLOR{ NORMAL, HOVER, DRAG}
	private AXIS_COLOR _axisColor = AXIS_COLOR.NORMAL;
	
	
	void Start(){
		if(!transform.GetComponent<Renderer>())
			return;
			
		NormalColor = transform.GetComponent<Renderer>().material.color;
		HoverColor = new Color(NormalColor.r, NormalColor.g, NormalColor.b, 1.0f);
		DragColor = new Color(1.0f, 1.0f, 0.0f, 0.8f);
	}//Start
	
	void SetAxisColor(AXIS_COLOR axisColor){
		_axisColor = axisColor;
		
		if(!transform.GetComponent<Renderer>())
			return;
			
		
		switch(_axisColor){
			case AXIS_COLOR.NORMAL:
				transform.GetComponent<Renderer>().material.color = NormalColor;
			break;
			
			case AXIS_COLOR.HOVER:
				transform.GetComponent<Renderer>().material.color = HoverColor;
			break;
			
			case AXIS_COLOR.DRAG:
				transform.GetComponent<Renderer>().material.color = DragColor;
			break;
		}//switch
	}//SetAxisColor
}
