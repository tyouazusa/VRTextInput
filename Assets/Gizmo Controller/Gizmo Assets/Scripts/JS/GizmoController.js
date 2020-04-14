import System;

enum GIZMO_MODE{ TRANSLATE, ROTATE, SCALE}
enum GIZMO_AXIS{ NONE, X, XY, XZ, Y, YZ, Z}

var RotationSpeed = 250;

//Snap settings
var SnappedRotationSpeed = 500;
var SnappedScaleMultiplier = 4;
var Snapping : boolean = false;
var MoveSnapIncrement : float = 1;
var AngleSnapIncrement : float = 5;
var ScaleSnapIncrement : float = 1;

var GizmoControlButtonImages : Texture2D[];
var LayerID : int = 8;

//Mode Settings
var AllowTranslate : boolean = true;
var AllowRotate : boolean = true;
var AllowScale : boolean = true;
private var TranslateInLocalSpace : boolean = false;
var ShowGizmoControlWindow : boolean = true;

//Shortcut Keys
var EnableShortcutKeys : boolean = true;
var TranslateShortcutKey : String = "1";
var RotateShortcutKey : String = "2";
var ScaleShortcutKey : String = "3";
var SnapShortcutKey : String = "s";

//private
private var _mode : GIZMO_MODE;
private var _selectedObject : Transform;
private var _showGizmo : boolean = false;

private var _activeAxis : GIZMO_AXIS = GIZMO_AXIS.NONE;
private var _selectedAxis : Transform = null;

private var _lastIntersectPosition : Vector3 = Vector3.zero;
private var _currIntersectPosition : Vector3 = Vector3.zero;

private var _draggingAxis : boolean = false;
private var _translationDelta : Vector3 = Vector3.zero;

private var _rotationSnapDelta : float = 0;
private var _scaleSnapDelta : Vector3 = Vector3.zero;
private var _moveSnapDelta : Vector3 = Vector3.zero;

private var _ignoreRaycast : boolean = false;

private var _XRotationDisplayValue : float = 0;
private var _YRotationDisplayValue : float = 0;
private var _ZRotationDisplayValue : float = 0;

private var _controlWinPosition : Vector2 = Vector2.zero;
private var _controlWinWidth : int = 105;
private var _controlWinHeight : int = 160;

private var AxisSpinner : GUISpinner = new GUISpinner();
private var SnapSpinner : GUISpinner = new GUISpinner();

function Start(){
	Hide();	
}//Start

//Returns true if currently dragging the gizmo with the mouse
function IsDraggingAxis(){
	return _draggingAxis;
}//IsDragging

//Returns true if the mouse is currently hovering over one of the gizmo axis controls
function IsOverAxis(){
	if(_selectedAxis != null)
		return true;
	
	return false;
}//IsOverAxis

//Make the gizmo visible
//Pass in GIZMO_MODE value which will set the current mode of the gizmo control
function Show(mode : GIZMO_MODE){
	if(_selectedObject == null)
		return;
		
	for(var t : Transform in transform){
		if(t.GetComponent.<Renderer>())
			t.GetComponent.<Renderer>().enabled = true;
		
		if(t.GetComponent.<Collider>())
			t.GetComponent.<Collider>().isTrigger = false;
	}//for
	
	SetMode(mode);
	_showGizmo = true;
}//Show

//Hides the gizmo
function Hide(){
	for(var t : Transform in transform){
		if(t.GetComponent.<Renderer>())
			t.GetComponent.<Renderer>().enabled = false;
		
		if(t.GetComponent.<Collider>())
			t.GetComponent.<Collider>().isTrigger = true;
	}//for
	
	_selectedObject = null;
	_selectedAxis = null;	
	_showGizmo = false;
}//Hide

//Returns true if the Gizmo control is currently not visible
function IsHidden(){
	return !_showGizmo;
}//IsHidden

//Set's the X/Y position in screen coordinates of the gizmo control window
function SetControlWinPosition(position : Vector2){
	_controlWinPosition = position;
}//SetControlWinPosition

//Toggles snap mode on and off
function ToggleSnapping(){
	Snapping = !Snapping;
}//ToggleSnapping

//Set's snapping mode
// Pass in boolean True=Snap on, False=Snap off
function SetSnapping(snap : boolean){
	Snapping = snap;
}//SetSnapping

//Returns the active GIZMO_MODE
function GetMode(){
	return _mode;
}//GetMode

function ResetTransformToSelectedObject(){
	if(_selectedObject == null)
		return;
	
	transform.position = _selectedObject.position;
	if(_mode != GIZMO_MODE.TRANSLATE)
		transform.rotation = _selectedObject.rotation;	
}//ResetPositionToSelectedObject

//Set's the allowable modes for the Gizmo
//move - Can use move/translate mode
//rotate - Can use rotate mode
//scale - Can use scale mode
function SetAllowedModes(move : boolean, rotate : boolean, scale : boolean){
	AllowTranslate = move;
	AllowRotate = rotate;
	AllowScale = scale;
}//SetAllowsModes

function ResetTransformations(resetPosition : boolean){
	if(resetPosition){
		transform.position = Vector3.zero;		
	}//if
		
	transform.rotation = Quaternion.identity;
	transform.localScale = Vector3.one;
	
	if(_selectedObject != null){
		_selectedObject.transform.position = transform.position;
		_selectedObject.transform.rotation = transform.rotation;
		_selectedObject.transform.localScale = Vector3.one;
	}//if
	
	_XRotationDisplayValue = 0;
	_YRotationDisplayValue = 0;
	_ZRotationDisplayValue = 0;
}//ResetTransformations

function OnGUI(){
	if(_selectedObject == null)
		return;
		
	if(_draggingAxis && _mode == GIZMO_MODE.ROTATE){		
		var screenPos : Vector3 = Camera.main.WorldToScreenPoint (transform.position);
		screenPos.y = Screen.height - screenPos.y;
		var RotationValue : float = 0;
		switch(_activeAxis){
			case GIZMO_AXIS.X: RotationValue = _XRotationDisplayValue; break;
			case GIZMO_AXIS.Y: RotationValue = _YRotationDisplayValue; break;
			case GIZMO_AXIS.Z: RotationValue = _ZRotationDisplayValue; break;
		}//switch
		
		if(Snapping)
			GUI.Label(Rect(screenPos.x, screenPos.y, 140, 20), "Snap Rotation Amt: " + Math.Round(RotationValue));
		else
			GUI.Label(Rect(screenPos.x, screenPos.y, 120, 20), "Rotation Amt: " + Math.Round(RotationValue, 1));
	}//if	
	
	if(_showGizmo && ShowGizmoControlWindow){
		if(AllowTranslate || AllowRotate || AllowScale){
			var ControlRect : Rect = Rect(_controlWinPosition.x, _controlWinPosition.y, _controlWinWidth, _controlWinHeight);
			if(ControlRect.Contains(Event.current.mousePosition))
				GUI.color.a = 1;
			else
				GUI.color.a = .8;
			DrawGizmoControls(ControlRect);
			GUI.color.a = 1;
		}//if
	}//if
}//OnGUI

function DrawGizmoControls(groupRect : Rect){	
	GUI.Box(groupRect, "Gizmo Control");
	var innerRect : Rect = Rect(groupRect.x+4, groupRect.y+20, groupRect.width-8, groupRect.height-24);	
	GUI.BeginGroup(innerRect);
	
		if(GizmoControlButtonImages.Length != 0){
			var tmpMode : GIZMO_MODE = GUI.Toolbar(Rect(0, 0, innerRect.width, 20), _mode, GizmoControlButtonImages);
			if(GUI.changed){
				GUI.changed = false;
				if((AllowTranslate && tmpMode == GIZMO_MODE.TRANSLATE) ||
					AllowRotate && tmpMode == GIZMO_MODE.ROTATE ||
					AllowScale && tmpMode == GIZMO_MODE.SCALE)
					SetMode(tmpMode);					
			}//if
		}//if
		else{				
			var CtrlBtnWidth = innerRect.width / 3;
			var CtrlBtnOffset = 0;
			if(AllowTranslate && GUI.Button(Rect(CtrlBtnOffset, 0, CtrlBtnWidth, 20), "P")){ SetMode(GIZMO_MODE.TRANSLATE);}		
			CtrlBtnOffset += CtrlBtnWidth;
		
			if(AllowRotate && GUI.Button(Rect(CtrlBtnOffset, 0, CtrlBtnWidth, 20), "R")){ SetMode(GIZMO_MODE.ROTATE);}		
			CtrlBtnOffset += CtrlBtnWidth;
		
			if(AllowScale && GUI.Button(Rect(CtrlBtnOffset, 0, CtrlBtnWidth, 20), "S")){ SetMode(GIZMO_MODE.SCALE);}		
		}//else
		
		switch(_mode){
			case GIZMO_MODE.TRANSLATE:
				if(AllowTranslate){
					transform.position.x = AxisSpinner.Draw(Rect(0, 25, innerRect.width, 20), "X:", transform.position.x);
					transform.position.y = AxisSpinner.Draw(Rect(0, 47, innerRect.width, 20), "Y:", transform.position.y);
					transform.position.z = AxisSpinner.Draw(Rect(0, 69, innerRect.width, 20), "Z:", transform.position.z);					
					_selectedObject.transform.position = transform.position;
					Snapping = GUI.Toggle(Rect(0, 94, innerRect.width, 20), Snapping, "Snap Mode");					
					MoveSnapIncrement = SnapSpinner.Draw(Rect(0, 114, innerRect.width, 20), "Snap By:", MoveSnapIncrement, 1);					
				}//if
				else{
					GUI.Label(Rect(0, 40, innerRect.width, 40), "Disabled");
				}//else
			break;
			
			case GIZMO_MODE.ROTATE:
				if(AllowRotate){
					if(_draggingAxis){
						AxisSpinner.Draw(Rect(0, 25, innerRect.width, 20), "X:", transform.rotation.eulerAngles.x);
						AxisSpinner.Draw(Rect(0, 47, innerRect.width, 20), "Y:", transform.rotation.eulerAngles.y);
						AxisSpinner.Draw(Rect(0, 69, innerRect.width, 20), "Z:", transform.rotation.eulerAngles.z);						
					}//if
					else{
						transform.rotation.eulerAngles.x = AxisSpinner.Draw(Rect(0, 25, innerRect.width, 20), "X:", transform.rotation.eulerAngles.x);
						transform.rotation.eulerAngles.y = AxisSpinner.Draw(Rect(0, 47, innerRect.width, 20), "Y:", transform.rotation.eulerAngles.y);
						transform.rotation.eulerAngles.z = AxisSpinner.Draw(Rect(0, 69, innerRect.width, 20), "Z:", transform.rotation.eulerAngles.z);						
						if(GUI.changed || _selectedObject.transform.rotation != transform.rotation){
							GUI.changed = false;
							_selectedObject.transform.rotation = transform.rotation;
						}//if
					}//if
					
					Snapping = GUI.Toggle(Rect(0, 94, innerRect.width, 20), Snapping, "Snap Mode");
					AngleSnapIncrement = SnapSpinner.Draw(Rect(0, 114, innerRect.width, 20), "Snap By:", AngleSnapIncrement, 1);					
				}//if
				else{
					GUI.Label(Rect(0, 40, innerRect.width, 40), "Disabled");
				}//else
			break;
			
			case GIZMO_MODE.SCALE:
				if(AllowScale){
					_selectedObject.localScale.x = AxisSpinner.Draw(Rect(0, 25, innerRect.width, 20), "X:", _selectedObject.localScale.x);
					_selectedObject.localScale.y = AxisSpinner.Draw(Rect(0, 47, innerRect.width, 20), "Y:", _selectedObject.localScale.y);
					_selectedObject.localScale.z = AxisSpinner.Draw(Rect(0, 69, innerRect.width, 20), "Z:", _selectedObject.localScale.z);									
					Snapping = GUI.Toggle(Rect(0, 94, innerRect.width, 20), Snapping, "Snap Mode");
					ScaleSnapIncrement = SnapSpinner.Draw(Rect(0, 114, innerRect.width, 20), "Snap By:", ScaleSnapIncrement, 1);							
				}//if
				else{
					GUI.Label(Rect(0, 40, innerRect.width, 40), "Disabled");
				}//else
			break;
		}//switch
		
	GUI.EndGroup();
}//DrawGizmoControls

function SetSelectedAxisObject(axisObject : Transform){
	if(_selectedAxis != null){
		if(axisObject == null){
			_selectedAxis.SendMessage("SetAxisColor", AXIS_COLOR.NORMAL);
		}//if
		else if(_selectedAxis.name != axisObject.name){
			_selectedAxis.SendMessage("SetAxisColor", AXIS_COLOR.NORMAL);
		}//if
	}//if
	
	_selectedAxis = axisObject;
	
	if(_selectedAxis != null){
		_selectedAxis.SendMessage("SetAxisColor", AXIS_COLOR.HOVER);				
	}//if
}//SetSelectedAxisObject

function SetDragHilight(SetDrag : boolean){
	if(_selectedAxis == null)
		return;
		
		if(SetDrag)
			_selectedAxis.SendMessage("SetAxisColor", AXIS_COLOR.DRAG);
		else
			_selectedAxis.SendMessage("SetAxisColor", AXIS_COLOR.HOVER);		
}//SetDragHihlight

function Update(){
	if(!_showGizmo || _selectedObject == null)
		return;
	
	if(EnableShortcutKeys){
		if(Input.GetKeyUp(TranslateShortcutKey)){
			if(AllowTranslate)
			SetMode(GIZMO_MODE.TRANSLATE);
		}//if
		
		if(Input.GetKeyUp(RotateShortcutKey)){
			if(AllowRotate)
			SetMode(GIZMO_MODE.ROTATE);
		}//if
		
		if(Input.GetKeyUp(ScaleShortcutKey)){
			if(AllowScale)
			SetMode(GIZMO_MODE.SCALE);
		}//if
		
		if(Input.GetKeyUp(SnapShortcutKey)){
			ToggleSnapping();
		}//if
	}//if
	
	AxisSpinner.Update();
	SnapSpinner.Update();
	
	//Scale Gizmo relative to the the distance from the camera for consistant sizing
	var distance = Mathf.Abs(Vector3.Distance(Camera.main.transform.position, transform.position));
	transform.localScale = Vector3(-1, 1, 1) * (distance/8);
	
	var layerMask = 1 << LayerID;
	var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
	var hit : RaycastHit;	
	if (Physics.Raycast (ray, hit, Mathf.Infinity, layerMask) && !_ignoreRaycast) {
		if(!_draggingAxis){
			if(hit.transform.name.Contains("X"))
				_activeAxis = GIZMO_AXIS.X;			
		
			if(hit.transform.name.Contains("Y"))
				_activeAxis = GIZMO_AXIS.Y;
			
			if(hit.transform.name.Contains("Z"))
				_activeAxis = GIZMO_AXIS.Z;
				
			if(hit.transform.name.Contains("XY"))
				_activeAxis = GIZMO_AXIS.XY;		
			
			if(hit.transform.name.Contains("XZ"))
				_activeAxis = GIZMO_AXIS.XZ;
			
			if(hit.transform.name.Contains("YZ"))
				_activeAxis = GIZMO_AXIS.YZ;
			
			SetSelectedAxisObject(hit.transform);
		}//if
		
		if(Input.GetMouseButtonUp(0)){
			_activeAxis = GIZMO_AXIS.NONE;
			_lastIntersectPosition = Vector3.zero;
			_draggingAxis = false;
			SetDragHilight(false);
			Camera.main.transform.SendMessage("setEnabled", true);
		}//if		
	}//if
	else{
		if(Input.GetMouseButtonDown(0)){
			//Set this to true if Mouse button was down so we aren't handling gizmo transformations when draging the mouse
			//over the gizmo when we don't mean to be interacting with it
			_ignoreRaycast = true;
			_activeAxis = GIZMO_AXIS.NONE;			
		}//if
		
		if(!_draggingAxis)
			SetSelectedAxisObject(null);
	}//else
	
	if(Input.GetMouseButtonUp(0)){
		_ignoreRaycast = false;
	}//if	
	
	if((_activeAxis != GIZMO_AXIS.NONE && Input.GetMouseButtonUp(0)) || (Input.GetMouseButtonDown(0) && !_draggingAxis)){
		_activeAxis = GIZMO_AXIS.NONE;
		_lastIntersectPosition = Vector3.zero;
		_currIntersectPosition = _lastIntersectPosition;
		_rotationSnapDelta = 0;
		_scaleSnapDelta = Vector3.zero;
		_moveSnapDelta = Vector3.zero;
		XRotationDisplayValue = 0;
		YRotationDisplayValue = 0;
		ZRotationDisplayValue = 0;
		_draggingAxis = false;
		SetDragHilight(false);
		Camera.main.transform.SendMessage("setEnabled", true);
	}//if
		
	if(_activeAxis != GIZMO_AXIS.NONE && Input.GetMouseButton(0)){
		var objPos : Vector3 = transform.position;
		var objMovement : Vector3 = Vector3.zero;		
		var plane : Plane;
		var hitDistance : float = 0;
		var MouseXDelta : float = Input.GetAxis("Mouse X");
		var MouseYDelta : float = Input.GetAxis("Mouse Y");
		var snapValue : float;		
		_draggingAxis = true;
		SetDragHilight(true);
		
		Camera.main.transform.SendMessage("setEnabled", false);
		_currIntersectPosition = _lastIntersectPosition;
		switch(_activeAxis){
			case GIZMO_AXIS.X:				
				switch(_mode){					
					case GIZMO_MODE.TRANSLATE:						
						plane = new Plane(Vector3.forward, transform.position);
						hitDistance = 0;
						if(plane.Raycast(ray, hitDistance)){
							_currIntersectPosition = GetIntersectPoint(hitDistance, ray);
							
							if(_lastIntersectPosition != Vector3.zero){								
								_translationDelta =  _currIntersectPosition - _lastIntersectPosition;
								if(Snapping && MoveSnapIncrement > 0){
									_moveSnapDelta.x += _translationDelta.x;
									snapValue = Mathf.Round(_moveSnapDelta.x / MoveSnapIncrement) * MoveSnapIncrement;
									_moveSnapDelta.x -= snapValue;
									
									//objMovement = Vector3.right * snapValue;
									objMovement = (TranslateInLocalSpace ? transform.right : Vector3.right) * snapValue;
									objPos += objMovement;
								}//if
								else{								
									//objMovement = Vector3.right * _translationDelta.x;
									objMovement = (TranslateInLocalSpace ? transform.right : Vector3.right) * _translationDelta.x;
									objPos += objMovement;
								}//else
								
								transform.position = objPos;
								_selectedObject.transform.position = transform.position;
							}//if						
						}//if					
						
						_lastIntersectPosition = _currIntersectPosition;
					break;
					
					case GIZMO_MODE.ROTATE:						
						var rotXDelta = MouseXDelta * Time.deltaTime;										
						
						if(Snapping && AngleSnapIncrement > 0){			
							rotXDelta *= SnappedRotationSpeed;
							_rotationSnapDelta += rotXDelta;		
							
							snapValue = Mathf.Round(_rotationSnapDelta / AngleSnapIncrement) * AngleSnapIncrement;																					
							_rotationSnapDelta -= snapValue;
							
							XRotationDisplayValue += snapValue;							
							transform.rotation *= Quaternion.AngleAxis(snapValue, Vector3.right);							
							_selectedObject.transform.rotation = transform.rotation;
						}//if
						else{
							rotXDelta *= RotationSpeed;
							XRotationDisplayValue += rotXDelta;							
							transform.Rotate(Vector3.right * rotXDelta);							
							_selectedObject.transform.rotation = transform.rotation;
						}//else
						
						
						XRotationDisplayValue = ClampRotationAngle(XRotationDisplayValue);
					break;
					
					case GIZMO_MODE.SCALE:						
						plane = new Plane(Vector3.forward, transform.position);
						hitDistance = 0;
						if(plane.Raycast(ray, hitDistance)){
							_currIntersectPosition = GetIntersectPoint(hitDistance, ray);
							
							if(_lastIntersectPosition != Vector3.zero){
								_translationDelta =  _currIntersectPosition - _lastIntersectPosition;								
								if(Snapping && ScaleSnapIncrement > 0){
									_translationDelta *= SnappedScaleMultiplier;
									
									_scaleSnapDelta.x += _translationDelta.x;
									snapValue = Mathf.Round(_scaleSnapDelta.x / ScaleSnapIncrement) * ScaleSnapIncrement;
									_scaleSnapDelta.x -= snapValue;
									
									objMovement = Vector3.right * snapValue;									
								}//if
								else{									
									objMovement = transform.right * _translationDelta.x;
								}//else
																
								_selectedObject.transform.localScale += objMovement;								
							}//if						
						}//if		
						
						_lastIntersectPosition = _currIntersectPosition;
					break;
				}//switch
			break;
			
			case GIZMO_AXIS.XY:
				switch(_mode){					
					case GIZMO_MODE.TRANSLATE:						
						plane = new Plane(Vector3.forward, transform.position);
						hitDistance = 0;
						if(plane.Raycast(ray, hitDistance)){
							_currIntersectPosition = GetIntersectPoint(hitDistance, ray);
							
							if(_lastIntersectPosition != Vector3.zero){								
								_translationDelta =  _currIntersectPosition - _lastIntersectPosition;
								if(Snapping && MoveSnapIncrement > 0){
									_moveSnapDelta.x += _translationDelta.x;
									snapValue = Mathf.Round(_moveSnapDelta.x / MoveSnapIncrement) * MoveSnapIncrement;
									_moveSnapDelta.x  -= snapValue;																											
									
									//objMovement = Vector3.right * snapValue;		
									objMovement = (TranslateInLocalSpace ? transform.right : Vector3.right) * snapValue;
									
									_moveSnapDelta.y += _translationDelta.y;
									snapValue = Mathf.Round(_moveSnapDelta.y / MoveSnapIncrement) * MoveSnapIncrement;
									_moveSnapDelta.y  -= snapValue;		
									
									//objMovement += Vector3.up * snapValue;
									objMovement += (TranslateInLocalSpace ? transform.up : Vector3.up) * snapValue;									
									objPos += objMovement;
								}//if
								else{								
									//objMovement = Vector3.right * _translationDelta.x;
									//objMovement += Vector3.up * _translationDelta.y;
									objMovement = (TranslateInLocalSpace ? transform.right : Vector3.right) * _translationDelta.x;
									objMovement += (TranslateInLocalSpace ? transform.up : Vector3.up) * _translationDelta.y;
									
									objPos += objMovement;
								}//else
								
								transform.position = objPos;
								_selectedObject.transform.position = transform.position;
							}//if						
						}//if					
						
						_lastIntersectPosition = _currIntersectPosition;
					break;
					
					case GIZMO_MODE.SCALE:						
						plane = new Plane(Vector3.forward, transform.position);
						hitDistance = 0;
						if(plane.Raycast(ray, hitDistance)){
							_currIntersectPosition = GetIntersectPoint(hitDistance, ray);
							
							if(_lastIntersectPosition != Vector3.zero){
								_translationDelta =  _currIntersectPosition - _lastIntersectPosition ;
								if(Snapping && ScaleSnapIncrement > 0){
									_translationDelta *= SnappedScaleMultiplier;
									
									_scaleSnapDelta.x += _translationDelta.x;
									snapValue = Mathf.Round(_scaleSnapDelta.x / ScaleSnapIncrement) * ScaleSnapIncrement;
									_scaleSnapDelta.x -= snapValue;
									
									objMovement = Vector3.right * snapValue;

									_scaleSnapDelta.y += _translationDelta.y;
									snapValue = Mathf.Round(_scaleSnapDelta.y / ScaleSnapIncrement) * ScaleSnapIncrement;
									_scaleSnapDelta.y -= snapValue;									
									
									objMovement += Vector3.up * snapValue;		
								}//if
								else{									
									objMovement = transform.right * _translationDelta.x;
									objMovement += transform.up * _translationDelta.y;
								}//else
																
								_selectedObject.transform.localScale += objMovement;								
							}//if						
						}//if		
						
						_lastIntersectPosition = _currIntersectPosition;
					break;
				}//switch
			break;			
			
			case GIZMO_AXIS.Y:
				switch(_mode){
					case GIZMO_MODE.TRANSLATE:
						plane = new Plane(Vector3.forward, transform.position);						
						hitDistance = 0;
						
						if(plane.Raycast(ray, hitDistance)){
							
							_currIntersectPosition = GetIntersectPoint(hitDistance, ray);
							
							if(_lastIntersectPosition != Vector3.zero){								
								_translationDelta =  _currIntersectPosition - _lastIntersectPosition ;
								if(Snapping && MoveSnapIncrement > 0){
									_moveSnapDelta.y += _translationDelta.y;
									snapValue = Mathf.Round(_moveSnapDelta.y / MoveSnapIncrement) * MoveSnapIncrement;
									_moveSnapDelta.y -= snapValue;
									
									//objMovement = Vector3.up * snapValue;			
									objMovement = (TranslateInLocalSpace ? transform.up : Vector3.up) * snapValue;
									objPos += objMovement;
								}//if
								else{
									//objMovement = Vector3.up * _translationDelta.y;
									objMovement = (TranslateInLocalSpace ? transform.up : Vector3.up) * _translationDelta.y;
									objPos += objMovement;
								}//else
								
								transform.position = objPos;
								_selectedObject.transform.position = transform.position;
							}//if							
						}//if						
						
						_lastIntersectPosition = _currIntersectPosition;
					break;
					
					case GIZMO_MODE.ROTATE:
						var rotYDelta = MouseXDelta * Time.deltaTime;
						
						if(Snapping && AngleSnapIncrement > 0){			
							rotYDelta *= SnappedRotationSpeed;
							_rotationSnapDelta += rotYDelta;		
							
							snapValue = Mathf.Round(_rotationSnapDelta / AngleSnapIncrement) * AngleSnapIncrement;																					
							_rotationSnapDelta -= snapValue;
							
							YRotationDisplayValue += snapValue;							
							transform.rotation *= Quaternion.AngleAxis(snapValue, Vector3.up);							
							_selectedObject.transform.rotation = transform.rotation;
						}//if
						else{
							rotYDelta *= RotationSpeed;
							YRotationDisplayValue += rotYDelta;							
							transform.Rotate(Vector3.up * rotYDelta);
							_selectedObject.transform.rotation = transform.rotation;
						}//else
						
						YRotationDisplayValue = ClampRotationAngle(YRotationDisplayValue);
					break;
					
					case GIZMO_MODE.SCALE:						
						plane = new Plane(Vector3.forward, transform.position);
						hitDistance = 0;
						if(plane.Raycast(ray, hitDistance)){
							_currIntersectPosition = GetIntersectPoint(hitDistance, ray);
							
							if(_lastIntersectPosition != Vector3.zero){
								_translationDelta =  _currIntersectPosition - _lastIntersectPosition ;
								if(Snapping && ScaleSnapIncrement > 0){
									_translationDelta *= SnappedScaleMultiplier;
									
									_scaleSnapDelta.y += _translationDelta.y;
									snapValue = Mathf.Round(_scaleSnapDelta.y / ScaleSnapIncrement) * ScaleSnapIncrement;
									_scaleSnapDelta.y -= snapValue;
									
									objMovement = Vector3.up * snapValue;									
								}//if
								else{									
									objMovement = Vector3.up * _translationDelta.y;
								}//else
																
								_selectedObject.transform.localScale += objMovement;								
							}//if						
						}//if		
						
						_lastIntersectPosition = _currIntersectPosition;
					break;
				}//switch
			break;
			
			case GIZMO_AXIS.XZ:
				switch(_mode){					
					case GIZMO_MODE.TRANSLATE:						
						plane = new Plane(Vector3.up, transform.position);
						hitDistance = 0;
						if(plane.Raycast(ray, hitDistance)){
							_currIntersectPosition = GetIntersectPoint(hitDistance, ray);
							
							if(_lastIntersectPosition != Vector3.zero){								
								_translationDelta =  _currIntersectPosition - _lastIntersectPosition;
								if(Snapping && MoveSnapIncrement > 0){
									_moveSnapDelta.x += _translationDelta.x;
									snapValue = Mathf.Round(_moveSnapDelta.x / MoveSnapIncrement) * MoveSnapIncrement;
									_moveSnapDelta.x  -= snapValue;																											
									
									//objMovement = Vector3.right * snapValue;		
									objMovement = (TranslateInLocalSpace ? transform.right : Vector3.right) * snapValue;
									
									_moveSnapDelta.z += _translationDelta.z;
									snapValue = Mathf.Round(_moveSnapDelta.z / MoveSnapIncrement) * MoveSnapIncrement;
									_moveSnapDelta.z  -= snapValue;		
									
									//objMovement += Vector3.forward * snapValue;
									objMovement += (TranslateInLocalSpace ? transform.forward : Vector3.forward) * snapValue;
									
									objPos += objMovement;
								}//if
								else{								
									//objMovement = Vector3.forward * _translationDelta.z;
									//objMovement += Vector3.right * _translationDelta.x;
									objMovement = (TranslateInLocalSpace ? transform.forward : Vector3.forward) * _translationDelta.z;
									objMovement += (TranslateInLocalSpace ? transform.right : Vector3.right) * _translationDelta.x;
									
									objPos += objMovement;
								}//else
								
								transform.position = objPos;
								_selectedObject.transform.position = transform.position;
							}//if						
						}//if					
						
						_lastIntersectPosition = _currIntersectPosition;
					break;
					
					case GIZMO_MODE.SCALE:						
						plane = new Plane(Vector3.up, transform.position);
						hitDistance = 0;
						if(plane.Raycast(ray, hitDistance)){
							_currIntersectPosition = GetIntersectPoint(hitDistance, ray);
							
							if(_lastIntersectPosition != Vector3.zero){
								_translationDelta =  _currIntersectPosition - _lastIntersectPosition ;
								if(Snapping && ScaleSnapIncrement > 0){
									_translationDelta *= SnappedScaleMultiplier;
									
									_scaleSnapDelta.x += _translationDelta.x;
									snapValue = Mathf.Round(_scaleSnapDelta.x / ScaleSnapIncrement) * ScaleSnapIncrement;
									_scaleSnapDelta.x -= snapValue;
									
									objMovement = Vector3.right * snapValue;

									_scaleSnapDelta.z += _translationDelta.z;
									snapValue = Mathf.Round(_scaleSnapDelta.z / ScaleSnapIncrement) * ScaleSnapIncrement;
									_scaleSnapDelta.z -= snapValue;									
									
									objMovement += Vector3.forward * snapValue;		
								}//if
								else{									
									objMovement = transform.right * _translationDelta.x;
									objMovement += transform.forward * _translationDelta.z;
								}//else
																
								_selectedObject.transform.localScale += objMovement;								
							}//if						
						}//if		
						
						_lastIntersectPosition = _currIntersectPosition;
					break;
				}//switch
			break;
			
			case GIZMO_AXIS.YZ:
				switch(_mode){					
					case GIZMO_MODE.TRANSLATE:						
						plane = new Plane(Vector3.right, transform.position);
						hitDistance = 0;
						if(plane.Raycast(ray, hitDistance)){
							_currIntersectPosition = GetIntersectPoint(hitDistance, ray);
							
							if(_lastIntersectPosition != Vector3.zero){								
								_translationDelta =  _currIntersectPosition - _lastIntersectPosition;
								if(Snapping && MoveSnapIncrement > 0){
									_moveSnapDelta.y += _translationDelta.y;
									snapValue = Mathf.Round(_moveSnapDelta.y / MoveSnapIncrement) * MoveSnapIncrement;
									_moveSnapDelta.y  -= snapValue;																											
									
									//objMovement = Vector3.up * snapValue;
									objMovement = (TranslateInLocalSpace ? transform.up : Vector3.up) * snapValue;
									
									_moveSnapDelta.z += _translationDelta.z;
									snapValue = Mathf.Round(_moveSnapDelta.z / MoveSnapIncrement) * MoveSnapIncrement;
									_moveSnapDelta.z  -= snapValue;		
									
									//objMovement += Vector3.forward * snapValue;
									objMovement += (TranslateInLocalSpace ? transform.forward : Vector3.forward) * snapValue;
									objPos += objMovement;
								}//if
								else{								
									//objMovement = Vector3.up * _translationDelta.y;
									//objMovement += Vector3.forward * _translationDelta.z;
									objMovement = (TranslateInLocalSpace ? transform.up : Vector3.up) * _translationDelta.y;
									objMovement += (TranslateInLocalSpace ? transform.forward : Vector3.forward) * _translationDelta.z;
									
									objPos += objMovement;
								}//else
								
								transform.position = objPos;
								_selectedObject.transform.position = transform.position;
							}//if						
						}//if					
						
						_lastIntersectPosition = _currIntersectPosition;
					break;
					
					case GIZMO_MODE.SCALE:						
						plane = new Plane(Vector3.right, transform.position);
						hitDistance = 0;
						if(plane.Raycast(ray, hitDistance)){
							_currIntersectPosition = GetIntersectPoint(hitDistance, ray);
							
							if(_lastIntersectPosition != Vector3.zero){
								_translationDelta =  _currIntersectPosition - _lastIntersectPosition ;
								if(Snapping && ScaleSnapIncrement > 0){
									_translationDelta *= SnappedScaleMultiplier;
									
									_scaleSnapDelta.y += _translationDelta.y;
									snapValue = Mathf.Round(_scaleSnapDelta.y / ScaleSnapIncrement) * ScaleSnapIncrement;
									_scaleSnapDelta.y -= snapValue;
									
									objMovement = Vector3.up * snapValue;

									_scaleSnapDelta.z += _translationDelta.z;
									snapValue = Mathf.Round(_scaleSnapDelta.z / ScaleSnapIncrement) * ScaleSnapIncrement;
									_scaleSnapDelta.z -= snapValue;									
									
									objMovement += Vector3.forward * snapValue;		
								}//if
								else{									
									objMovement = transform.up * _translationDelta.y;
									objMovement += transform.forward * _translationDelta.z;
								}//else
																
								_selectedObject.transform.localScale += objMovement;								
							}//if						
						}//if		
						
						_lastIntersectPosition = _currIntersectPosition;
					break;
				}//switch
			break;
			
			case GIZMO_AXIS.Z:
				switch(_mode){
					case GIZMO_MODE.TRANSLATE:
						plane = new Plane(Vector3.up, transform.position);						
						hitDistance = 0;
						
						if(plane.Raycast(ray, hitDistance)){
							
							_currIntersectPosition = GetIntersectPoint(hitDistance, ray);
							
							if(_lastIntersectPosition != Vector3.zero){								
								_translationDelta =  _currIntersectPosition - _lastIntersectPosition ;
								if(Snapping && MoveSnapIncrement > 0){
									_moveSnapDelta.z += _translationDelta.z;
									snapValue = Mathf.Round(_moveSnapDelta.z / MoveSnapIncrement) * MoveSnapIncrement;
									_moveSnapDelta.z -= snapValue;
									
									//objMovement = Vector3.forward * snapValue;			
									objMovement = (TranslateInLocalSpace ? transform.forward : Vector3.forward) * snapValue;
									objPos += objMovement;
								}//if
								else{
									//objMovement = Vector3.forward * _translationDelta.z;
									objMovement = (TranslateInLocalSpace ? transform.forward : Vector3.forward) * _translationDelta.z;
									objPos += objMovement;
								}//else
								
								transform.position = objPos;
								_selectedObject.transform.position = transform.position;
							}//if							
						}//if						
						
						_lastIntersectPosition = _currIntersectPosition;
					break;
					
					case GIZMO_MODE.ROTATE:
						var rotZDelta = MouseXDelta * Time.deltaTime;
						
						if(Snapping && AngleSnapIncrement > 0){			
							rotZDelta *= SnappedRotationSpeed;
							_rotationSnapDelta += rotZDelta;		
							
							snapValue = Mathf.Round(_rotationSnapDelta / AngleSnapIncrement) * AngleSnapIncrement;																					
							_rotationSnapDelta -= snapValue;
							
							ZRotationDisplayValue += snapValue;							
							transform.rotation *= Quaternion.AngleAxis(snapValue, Vector3.forward);							
							_selectedObject.transform.rotation = transform.rotation;
						}//if
						else{
							rotZDelta *= RotationSpeed;
							ZRotationDisplayValue += rotZDelta;
							
							transform.Rotate(Vector3.forward * rotZDelta);
							_selectedObject.transform.rotation = transform.rotation;
						}//else
						
						ZRotationDisplayValue = ClampRotationAngle(ZRotationDisplayValue);
					break;
					
					case GIZMO_MODE.SCALE:						
						plane = new Plane(Vector3.up, transform.position);
						hitDistance = 0;
						if(plane.Raycast(ray, hitDistance)){
							_currIntersectPosition = GetIntersectPoint(hitDistance, ray);
							
							if(_lastIntersectPosition != Vector3.zero){
								_translationDelta =  _currIntersectPosition - _lastIntersectPosition;
								if(Snapping && ScaleSnapIncrement > 0){
									_translationDelta *= SnappedScaleMultiplier;
									
									_scaleSnapDelta.z += _translationDelta.z;
									snapValue = Mathf.Round(_scaleSnapDelta.z / ScaleSnapIncrement) * ScaleSnapIncrement;
									_scaleSnapDelta.z -= snapValue;
									
									objMovement = Vector3.forward * snapValue;									
								}//if
								else{									
									objMovement = Vector3.forward * _translationDelta.z;
								}//else
																
								_selectedObject.transform.localScale += objMovement;								
							}//if						
						}//if		
						
						_lastIntersectPosition = _currIntersectPosition;
					break;
				}//switch
			break;
		}//switch
	}//if	
}//Update

//Sets the currently selected object transform
//Pass in GameObject Transform
function SetSelectedObject(ObjectTransform : Transform){
	if(ObjectTransform == null)
		return;
		
	_selectedObject = ObjectTransform;	
	transform.position = _selectedObject.transform.position;

	SetMode(_mode);
	
	XRotationDisplayValue = 0;
	YRotationDisplayValue = 0;
	ZRotationDisplayValue = 0;
}//ObjectTransform

//Set's the active GIZMO_MODE
//GIZMO_MODE.TRANSLATE, GIZMO_MODE.ROTATE, GIZMO_MODE.SCALE
function SetMode(mode : GIZMO_MODE){		
	_mode = mode;
	
	switch(_mode){
		case GIZMO_MODE.TRANSLATE:
			transform.rotation = Quaternion.identity;
		break;
		
		case GIZMO_MODE.ROTATE:
			transform.rotation = _selectedObject.transform.rotation;
		break;
		
		case GIZMO_MODE.SCALE:
			transform.rotation = _selectedObject.transform.rotation;
		break;
	}//switch
	
	for(var t : Transform in transform){
		if(t.name.Contains("Pivot"))
			continue;
			
		switch(_mode){
			case GIZMO_MODE.TRANSLATE:
				if(t.name.Contains("Translate") || t.name.Contains("XY") || t.name.Contains("XZ") || t.name.Contains("YZ")){
					t.GetComponent.<Renderer>().enabled = true;
					t.gameObject.layer = LayerID;
				}//if
				else{
					t.GetComponent.<Renderer>().enabled = false;
					t.gameObject.layer = 2;
				}//else
			break;
			
			case GIZMO_MODE.ROTATE:
				if(t.name.Contains("Rotate")){					
					t.GetComponent.<Renderer>().enabled = true;
					t.gameObject.layer = LayerID;
				}//if
				else{
					t.GetComponent.<Renderer>().enabled = false;
					t.gameObject.layer = 2;
				}//else
			break;
			
			case GIZMO_MODE.SCALE:
				if(t.name.Contains("Scale") || t.name.Contains("XY") || t.name.Contains("XZ") || t.name.Contains("YZ")){					
					t.GetComponent.<Renderer>().enabled = true;
					t.gameObject.layer = LayerID;
				}//if
				else{
					t.GetComponent.<Renderer>().enabled = false;
					t.gameObject.layer = 2;
				}//else
			break;
		}//switch
	}//for
}//SetMode

function ClampRotationAngle(value : float){
	if(value > 360){ value -= 360; }	
	if(value < 0){ value += 360; }
	
	return value;
}//ClampRotation

function GetIntersectPoint(HitDistance : float, ray : Ray){
	var ReturnPoint = Vector3.zero;
	
	if (Camera.main.orthographic)
		ReturnPoint = ray.origin;
	else
		ReturnPoint = ray.direction * HitDistance;
		
	return ReturnPoint;
}//GetintersectPoint