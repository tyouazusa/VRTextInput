var target : Transform;
var distance = 6.0;
var moveSpeed : float = 500;

var xSpeed = 250.0;
var ySpeed = 120.0;

var yMinLimit = -20;
var yMaxLimit = 88;

var zoomSpeed = 10;
var zoomNearLimit : float = 1;
var zoomFarLimit : float = 200;

private var x = 0.0;
private var y = 0.0;
private var z = 0.0;

private var moveMultiplier : float = .2;

private var ignoreHotControl : boolean = false;
private var isEnabled : boolean = true;

private var downButtonPressed : boolean = false;
private var leftButtonPressed : boolean = false;
private var rightButtonPressed : boolean = false;
private var upButtonPressed : boolean = false;

@script AddComponentMenu("Camera-Control/Mouse Orbit")

function Start () {
    resetCamera();
		
}//Start

function resetCamera(){
	if(target){
		distance = 6;
		x = 180;
		y = 15;
		
		target.transform.position = Vector3.zero;
		
		var rotation = Quaternion.Euler(y, x, 0);
        var position = rotation * Vector3(0.0, 0.0, -distance) + target.position;
        
        transform.rotation = rotation;
        transform.position = position;
	}
}//resetCamera

function setEnabled(enabled : boolean){
	isEnabled = enabled;
}//setEnabled

function zoom(val : float){
	distance += val * Time.deltaTime;
	distance = Mathf.Clamp(distance, zoomNearLimit, zoomFarLimit);
	ignoreHotControl = true;
}//zoom

function rotate(val : float){
	x += (val* Time.deltaTime) * xSpeed * 0.02;
	ignoreHotControl = true;
}//rotate

function move(dir : String){
	switch(dir){
		case "up":
			upButtonPressed = true;
		break;
		
		case "down":
			downButtonPressed = true;
		break;
		
		case "left":
			leftButtonPressed = true;
		break;
		
		case "right":
			rightButtonPressed = true;
		break;
	}//switch
	
	ignoreHotControl = true;
}//move

function LateUpdate () {
	
	if(GUIUtility.hotControl && !ignoreHotControl)
		return;
		
	if(!isEnabled)
		return;
	
    if (target) {

    	/*Rotation*/
    	
    	if(target && Input.GetAxis("Mouse ScrollWheel")){
			distance += Input.GetAxis("Mouse ScrollWheel");
			distance = Mathf.Clamp(distance, zoomNearLimit, zoomFarLimit);
		}//if    	
    	
    	if(Input.GetAxis("Fire1")){
        	x += Input.GetAxis("Mouse X") * xSpeed * 0.02;
        	y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02;
 		
 			y = ClampAngle(y, yMinLimit, yMaxLimit);
    	}//if
    	
        var rotation = Quaternion.Euler(y, x, 0);
        var position = rotation * Vector3(0.0, 0.0, -distance) + target.position;
        
        transform.rotation = rotation;
        transform.position = position;
        
                
        
        /*Movement*/
        var dir : Vector3;
        var move : Vector3;
    	if(Input.GetKey("up") || upButtonPressed){
    		//Debug.Log("Moving Camera");
			dir = transform.position - target.transform.position;
			
			move = (dir.normalized) * moveSpeed * Time.deltaTime;
			
		
			if(Input.GetKey(KeyCode.LeftShift)){
				transform.position.x -= move.x*moveMultiplier;
				transform.position.z -= move.z*moveMultiplier;
				
				target.transform.position.x -= move.x*moveMultiplier;
				target.transform.position.z -= move.z*moveMultiplier;
			}
			else{
				transform.position.x -= move.x;
				transform.position.z -= move.z;
				target.transform.position.x -= move.x;
				target.transform.position.z -= move.z;
			}
			
			if(upButtonPressed)
				upButtonPressed = false;
		}
		
		if(Input.GetKey("down") || downButtonPressed){
    		//Debug.Log("Moving Camera");
			dir = transform.position - target.transform.position;
			
			move = (dir.normalized) * moveSpeed * Time.deltaTime;
			
			
			if(Input.GetKey(KeyCode.LeftShift)){
				transform.position.x += move.x*moveMultiplier;
				transform.position.z += move.z*moveMultiplier;
				
				target.transform.position.x += move.x*moveMultiplier;
				target.transform.position.z += move.z*moveMultiplier;
			}
			else{
				transform.position.x += move.x;
				transform.position.z += move.z;
				target.transform.position.x += move.x;
				target.transform.position.z += move.z;
			}
			
			if(downButtonPressed)
				downButtonPressed = false;
		}
		
		if(Input.GetKey("left") || leftButtonPressed){
    		//Debug.Log("Moving Camera");
			dir = transform.position - target.transform.position;
			//dir += ;
			
			
			move = -(transform.right) * moveSpeed * Time.deltaTime;
			
			if(Input.GetKey(KeyCode.LeftShift)){
				transform.position.x += move.x*moveMultiplier;
				transform.position.z += move.z*moveMultiplier;
				
				target.transform.position.x += move.x*moveMultiplier;
				target.transform.position.z += move.z*moveMultiplier;
			}
			else{
				transform.position.x += move.x;
				transform.position.z += move.z;
				target.transform.position.x += move.x;
				target.transform.position.z += move.z;
			}
			
			if(leftButtonPressed)
				leftButtonPressed = false;
			
		}
		
		if(Input.GetKey("right") || rightButtonPressed){
    		//Debug.Log("Moving Camera");
			dir = transform.position - target.transform.position;
			//dir += ;
			
			
			move = transform.right * moveSpeed * Time.deltaTime;
			
			
			if(Input.GetKey(KeyCode.LeftShift)){
				transform.position.x += move.x*moveMultiplier;
				transform.position.z += move.z*moveMultiplier;
				
				target.transform.position.x += move.x*moveMultiplier;
				target.transform.position.z += move.z*moveMultiplier;
			}
			else{
				transform.position.x += move.x;
				transform.position.z += move.z;
				target.transform.position.x += move.x;
				target.transform.position.z += move.z;
			}
			
			if(rightButtonPressed)
				rightButtonPressed = false;
		}
		
		if(Input.GetMouseButton(1)){
        	
        	var hMove : Vector3 = transform.right * (moveSpeed*2) * Time.deltaTime;
        	var vMove : Vector3 = transform.up *  (moveSpeed*2) * Time.deltaTime;
        	
        	transform.position.x += hMove.x*Input.GetAxis("Mouse X");
        	transform.position.z += hMove.z*Input.GetAxis("Mouse X");
        	
			transform.position.y += vMove.y*Input.GetAxis("Mouse Y");
			
			target.transform.position.x += hMove.x*Input.GetAxis("Mouse X");
			target.transform.position.z += hMove.z*Input.GetAxis("Mouse X");
			
			target.transform.position.y += vMove.y*Input.GetAxis("Mouse Y");
        	
        }

		
    }//if
    
    if(ignoreHotControl)
    	ignoreHotControl = false;
}//LateUpdate

static function ClampAngle (angle : float, min : float, max : float) {
	if (angle < -360)
		angle += 360;
	if (angle > 360)
		angle -= 360;
	return Mathf.Clamp (angle, min, max);
}//ClampAngle