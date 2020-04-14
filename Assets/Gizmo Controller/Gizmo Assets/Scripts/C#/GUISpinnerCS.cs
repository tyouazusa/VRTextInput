using UnityEngine;
using System;
using System.Collections;


public class GUISpinnerCS{
	public GUIStyle LabelStyle = null;
	public GUIStyle ButtonUpStyle = null;
	public GUIStyle ButtonDownStyle = null;
	public float StepValue = 0.1f;
	public float ResetValue = 0.0f;
	public int ButtonWidth = 20;
	public bool ResetOnMouseClick = true;
	public float IncrementSpeed = 0.5f;
	
	private float _mouseDownTime = 0.0f;
	
	public GUISpinnerCS(){}
	
	public void Update(){
		if(Input.GetMouseButton(0)){
			_mouseDownTime += 1 * Time.deltaTime;
		}//if
	
		if(Input.GetMouseButtonUp(0)){
			_mouseDownTime = 0;
		}//if		
	}//Update
	
	public float Draw(Rect rect, string label, float value, float stepValue){
		StepValue = stepValue;
		return Draw(rect, label, value);
	}//Draw
	
	public float Draw(Rect rect, string label, float value){
		Vector2 LabelSize = Vector2.zero;
		if(LabelStyle != null)
			LabelSize = LabelStyle.CalcSize(new GUIContent(label));
		else
			LabelSize = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(label));
		
		int labelWidth = (int)LabelSize.x+4;		
		int textFieldWidth = (int)rect.width - labelWidth - ButtonWidth;
		
		if(rect.Contains(Event.current.mousePosition) && Input.GetMouseButtonDown(1)){
			value = ResetValue;
		}//if
		
		GUI.BeginGroup(rect);
			if(LabelStyle != null)
				GUI.Label(new Rect(0, 0, labelWidth, rect.height), label, LabelStyle);
			else
				GUI.Label(new Rect(0, 0, labelWidth, rect.height), label);
				
			var valStr = GUI.TextField(new Rect(labelWidth, 0, rect.width-labelWidth-20, rect.height), Math.Round(value, 2).ToString());
		
			if(GUI.changed){			
				if(!float.TryParse(valStr, out value))
				value = ResetValue;
			}//if
		
			Rect ButtonPlusRect = new Rect(labelWidth + textFieldWidth, 0, ButtonWidth, rect.height/2);
			Rect ButtonMinusRect = new Rect(labelWidth + textFieldWidth, rect.height/2, ButtonWidth, rect.height/2);
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
