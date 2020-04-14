public var SelectedObject : Transform;
public var LogoGraphic : Texture2D;

function OnGUI(){
	GUILayout.BeginArea(Rect(10, Screen.height-25, Screen.width-20, 25));
		GUILayout.BeginHorizontal();
			GUILayout.Label("LMB: Camera Rotate");
			GUILayout.Label("RMB: Camera Pan");
			GUILayout.Label("1: Translate Mode");
			GUILayout.Label("2: Rotate Mode");
			GUILayout.Label("3: Scale Mode");
			GUILayout.Label("S: Enable/Disable Snapping");
		GUILayout.EndHorizontal();
	GUILayout.EndArea();
	
	GUI.DrawTexture(Rect(Screen.width-130, 2, 128, 128), LogoGraphic);
}//OnGUI