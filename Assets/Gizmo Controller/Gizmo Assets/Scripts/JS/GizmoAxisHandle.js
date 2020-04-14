var NormalColor : Color;
var HoverColor : Color;
var DragColor : Color;

enum AXIS_COLOR{ NORMAL, HOVER, DRAG}
private var _axisColor : AXIS_COLOR = AXIS_COLOR.NORMAL;


function Start(){
	if(!transform.GetComponent.<Renderer>())
		return;
		
	NormalColor = transform.GetComponent.<Renderer>().material.color;
	HoverColor = Color(NormalColor.r, NormalColor.g, NormalColor.b, 1);
	DragColor = Color(1, 1, 0, .8);
}//Start

function SetAxisColor(axisColor : AXIS_COLOR){
	_axisColor = axisColor;
	
	if(!transform.GetComponent.<Renderer>())
		return;
		
	
	switch(_axisColor){
		case AXIS_COLOR.NORMAL:
			transform.GetComponent.<Renderer>().material.color = NormalColor;
		break;
		
		case AXIS_COLOR.HOVER:
			transform.GetComponent.<Renderer>().material.color = HoverColor;
		break;
		
		case AXIS_COLOR.DRAG:
			transform.GetComponent.<Renderer>().material.color = DragColor;
		break;
	}//switch
}//SetAxisColor