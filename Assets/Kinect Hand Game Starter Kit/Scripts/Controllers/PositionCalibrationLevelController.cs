using UnityEngine;
using System.Collections;

public class PositionCalibrationLevelController : MonoBehaviour 
{
    /// <summary>
    /// Simulate kinect using mouse
    /// </summary>
    public bool UseMouse = false;

    public TopViewPlayerPositionDetector TopView;

    /// <summary>
    /// This textMesh appears on the screen (with tween) after successful calibration
    /// </summary>
    public TextMesh CalibrationCompletedText;

    /// <summary>
    /// Name of KinectManager singleton to find it and save a reference
    /// </summary>
    public string KinectManagerName = "My Kinect Manager";
    private MyKinectManager KinectManager;

    /// <summary>
    /// TopView Player Position
    /// </summary>
    public bool GoodPlayerPosition = false;

    /// <summary>
    /// At the end of the level, all objects move out of screen, so this flag means that Update must not be played
    /// </summary>
    private bool FinalAnimation = false;

	// Use this for initialization
	void Start () 
    {
        if (KinectManager == null)
            KinectManager = GameObject.Find(KinectManagerName).GetComponent<MyKinectManager>();
	}

    /// <summary>
    /// Message from TopView
    /// </summary>
    /// <param name="goodPlayerPosition"></param>
    public void ValidPlayerPosition(bool goodPlayerPosition)
    {
        GoodPlayerPosition = goodPlayerPosition;
    }

    /// <summary>
    /// Message from TopView
    /// </summary>
    public void FirstTimeInitialized()
    {
        FinalAnimation = true;
        StartCoroutine(NextLevelInAFewSec());
    }
    
    /// <summary>
    /// Coroutine with final animation
    /// </summary>
    /// <returns></returns>
    IEnumerator NextLevelInAFewSec()
    {
        iTween.MoveTo(CalibrationCompletedText.gameObject, iTween.Hash("y", 0.0f, "easeType", "easeOutElastic", "time", 1.4f));
        yield return new WaitForSeconds(1.4f);

        Transform[] allObjects = GameObject.FindObjectsOfType<Transform>();

        foreach (Transform t in allObjects)
        {
            if (t.tag != "MainCamera" && t.tag != "NoFinalAnimation")
            {
                iTween.MoveBy(t.gameObject, iTween.Hash("y", -10.0f, "easeType", "easeInOutQuad", "time", 0.5f));//, "delay", Random.value * 0.5f));
            }
        }

        yield return new WaitForSeconds(1);
        Application.LoadLevel(1);
    }


	// Update is called once per frame
	void Update () 
    {
        if (FinalAnimation) return;

        if (UseMouse)
        {
            // Simulate kinect by mouse
            float x = (Input.mousePosition.x) / Screen.width * 2.0f - 1.0f;
            if (x < -1.0f) x = -1.0f;
            else if (x > 1.0f) x = 1.0f;

            float z = 3.0f - (Input.mousePosition.y) / (float)Screen.height * 2.0f;

            TopView.SetHipCenterPosition(new Vector3(x, 0.0f, z));
        }
        else
        {
            if (KinectManager.SkeletonIsTracked)
            {
                TopView.SetHipCenterPosition(KinectManager.HipCenterPosition);
            }
        }
	}

    void OnGUI()
    {
        // Kinect angle controller is a useful thing =)
        GUI.Label(new Rect(Screen.width - 95, 15, 150, 25), "Kinect Angle");
        if (GUI.Button(new Rect(Screen.width - 85, 40, 50, 25), "20"))
        {
            KinectWrapper.NuiCameraElevationSetAngle(20);
        }
        if (GUI.Button(new Rect(Screen.width - 85, 70, 50, 25), "10"))
        {
            KinectWrapper.NuiCameraElevationSetAngle(10);
        }
        if (GUI.Button(new Rect(Screen.width - 85, 100, 50, 25), "0"))
        {
            KinectWrapper.NuiCameraElevationSetAngle(0);
        }
        if (GUI.Button(new Rect(Screen.width - 85, 130, 50, 25), "-10"))
        {
            KinectWrapper.NuiCameraElevationSetAngle(-10);
        }
    }

}
