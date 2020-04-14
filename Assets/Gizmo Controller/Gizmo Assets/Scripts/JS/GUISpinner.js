
class GUISpinner{
	var LabelStyle : GUIStyle = null;
	var ButtonUpStyle : GUIStyle = null;
	var ButtonDownStyle : GUIStyle = null;
	var StepValue : float = 0.1;
	var ResetValue : float = 0;
	var ButtonWidth : int = 20;
	var ResetOnMouseClick : boolean = true;
	var IncrementSpeed : float = 0.5;
	
	private var _mouseDownTime : float = 0;
	
	function GUISpinner(){}
	
	function Update(){
		if(Input.GetMouseButton(0)){
			_mouseDownTime += 1 * Time.deltaTime;
		}//if
	
		if(Input.GetMouseButtonUp(0)){
			_mouseDownTime = 0;
		}//if		
	}//Update
	
	function Draw(rect : Rect, label : String, value : float, stepValue : float){
		StepValue = stepValue;
		return Draw(rect, label, value);
	}//Draw
	
	function Draw(rect : Rect, label : String, value : float){
		var LabelSize : Vector2 = Vector2.zero;
		if(LabelStyle != null)
			LabelSize = LabelStyle.CalcSize(GUIContent(label));
		else
			LabelSize = GUI.skin.GetStyle("Label").CalcSize(GUIContent(label));
		
		var labelWidth : int = LabelSize.x+4;		
		var textFieldWidth : int = rect.width - labelWidth - ButtonWidth;
		
		if(rect.Contains(Event.current.mousePosition) && Input.GetMouseButtonDown(1)){
			value = ResetValue;
		}//if
		
		GUI.BeginGroup(rect);
			if(LabelStyle != null)
				GUI.Label(Rect(0, 0, labelWidth, rect.height), label, LabelStyle);
			else
				GUI.Label(Rect(0, 0, labelWidth, rect.height), label);
				
			var valStr = GUI.TextField(Rect(labelWidth, 0, rect.width-labelWidth-20, rect.height), Math.Round(value, 2).ToString());
		
			if(GUI.changed){			
				if(!float.TryParse(valStr, value))
				value = ResetValue;
			}//if
		
			var ButtonPlusRect : Rect = Rect(labelWidth + textFieldWidth, 0, ButtonWidth, rect.height/2);
			var ButtonMinusRect : Rect = Rect(labelWidth + textFieldWidth, rect.height/2, ButtonWidth, rect.height/2);
			if(Event.current.type == EventType.Repaint){
				if(_mouseDownTime > IncrementSpeed && ButtonPlusRect.Contains(Event.current.mousePosition)){
					value += StepValue;
				}//if
			
				if(_mouseDownTime > IncrementSpeed && ButtonMinusRect.Contains(Event.current.mousePosition)){
					value -= StepValue;
				}//if
			}//if
			
			//if(ButtonUpStyle != null)
			//	if(GUI.Button (ButtonPlusRect, "", ButtonUpStyle)){ value += StepValue; GUI.changed = true;}
			//else
				if(GUI.Button (ButtonPlusRect, "+")){ value += StepValue; GUI.changed = true;}
			
			//if(ButtonDownStyle != null)
			//	if(GUI.Button (ButtonMinusRect, "", ButtonDownStyle)){value -= StepValue; GUI.changed = true;}		
			//else
				if(GUI.Button (ButtonMinusRect, "-")){value -= StepValue; GUI.changed = true;}		
		GUI.EndGroup();
		
		return value;
	}//Draw
	
}//GUISpinner