private var NormalColor : Color;
private var HilightColor : Color;
var OverBox : boolean = false;
var GC : GizmoController = null;


function Start(){
	if(!GetComponent.<Renderer>())
		return;
		
	NormalColor = GetComponent.<Renderer>().material.color;
	HilightColor = new Color(NormalColor.r * 1.2, NormalColor.g * 1.2, NormalColor.b * 1.2, 1);
	GC = GameObject.Find("GizmoAdvanced").GetComponent("GizmoController");
	GC.Hide();
}//Start

function OnMouseEnter(){
	GetComponent.<Renderer>().material.color = HilightColor;
	OverBox = true;
}//OnMouseEnter

function OnMouseExit(){
	GetComponent.<Renderer>().material.color = NormalColor;
	OverBox = false;
}//OnMouseEnter

function OnMouseDown(){	
	if(GC == null)
		return;
		
	if(GC.IsOverAxis())
		return;
	
	GC.SetSelectedObject(transform);
	
	if(GC.IsHidden())
		GC.Show(GIZMO_MODE.TRANSLATE);
}//OnMouseDown
/*
function Update(){
	if(GC == null)
		return;
		
	if(Input.GetMouseButtonUp(0) && !OverBox){
		GC.Hide();
	}//if
	else if(OverBox){
		if(!GC.IsOverAxis()){
			if(Input.GetMouseButtonUp(0))				
				GC.SetSelectedObject(transform);
				if(GC.IsHidden())
					GC.Show(GIZMO_MODE.TRANSLATE);
		}//if
	}//else if
}//Update
*/