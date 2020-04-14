using UnityEngine;
using System.Collections;

public enum HAND_CALIBRATION_LEVEL_STATES
{
    StartingAnimation,
    HandSelection,
    HandCalibration,
    FinalAnimation
}

public enum BUTTON_NAMES
{
    LeftHand,
    RightHand,
    CalibrationDone
}

public class HandXYCalibrationLevelController : MonoBehaviour 
{
    /// <summary>
    /// Simulate kinect using mouse
    /// </summary>
    public bool UseMouse = true;

    /// <summary>
    /// Name of KinectManager singleton to find it and save a reference
    /// </summary>
    public string KinectManagerName = "My Kinect Manager";
    private MyKinectManager KinectManager;

    /// <summary>
    /// Prefabs
    /// </summary>
    public Button ButtonPrefab;
    public Button ButtonWidePrefab; // The same button, but different shape
    public HandXYCalibrator HandCalibratorPrefab;
    
    private HandXYCalibrator HandCalibrator;

    /// <summary>
    /// We can place on our scene a little TopViewPlayerPositionDetector to check if player's position is valid
    /// </summary>
    public TopViewPlayerPositionDetector TopView;

    /// <summary>
    /// Text Mesh in the same GameObject with this script
    /// </summary>
    private TextMesh LevelTitle;

    // Kinect Manager will be on the scene from the previous scene, so we choose good position for it's text
    private Vector2 KinectManagerPosition = new Vector2(0.0f, -4.6f);

    /// <summary>
    /// Level state
    /// </summary>
    private HAND_CALIBRATION_LEVEL_STATES state = HAND_CALIBRATION_LEVEL_STATES.StartingAnimation;

    /// <summary>
    /// TopView Player Position
    /// </summary>
    public bool GoodPlayerPosition = false;

    /// <summary>
    /// Final Calibration Data
    /// </summary>
    public bool rightHand = true;
    public Vector2 MaxHandPosition;
    public Vector2 MinHandPosition;


    // Use this for initialization
	void Start () 
    {
        // Find links to objects
        if (KinectManager == null)
            KinectManager = GameObject.Find(KinectManagerName).GetComponent<MyKinectManager>();

        if (KinectManager == null) Debug.LogError("No KinectManager Found On The Scene");

        LevelTitle = GetComponent<TextMesh>();
        LevelTitle.text = "Choose Hand To Play";

        // Coroutine with animations.
        // At the end it changes state
        StartCoroutine(StartingAnimation());

	}

    /// <summary>
    /// All objects appear on the scene
    /// </summary>
    /// <returns></returns>
    IEnumerator StartingAnimation()
    {
        float startingAnimationTime = 1.0f;
        float buttonAppearenceAnimationTime = 1.0f;


        // move KinectManager (with TextMesh in it)
        iTween.MoveTo(KinectManager.gameObject, iTween.Hash("x", KinectManagerPosition.x, "y", KinectManagerPosition.y, "easeType", "easeInOutQuad", "time", startingAnimationTime));

        // move level title text
        Vector2 TitlePosition = LevelTitle.transform.position;
        LevelTitle.transform.position = new Vector2(LevelTitle.transform.position.x, LevelTitle.transform.position.y + 2.0f);
        iTween.MoveTo(LevelTitle.gameObject, iTween.Hash("y", TitlePosition.y, "easeType", "easeInOutQuad", "time", startingAnimationTime));

        // move TopView
        if (TopView != null)
        {
            Vector2 TopViewPosition = TopView.transform.position;
            TopView.transform.position = new Vector2(TopView.transform.position.x, TopView.transform.position.y - 2.0f);
            iTween.MoveTo(TopView.gameObject, iTween.Hash("y", TopViewPosition.y, "easeType", "easeInOutQuad", "time", startingAnimationTime));
        }

        yield return new WaitForSeconds(startingAnimationTime);

        // Instantiate Buttons and tween them
        Button leftButton = Instantiate(ButtonPrefab, new Vector3(-11.6f, -0.29f, 0.0f), Quaternion.identity) as Button;
        leftButton.DisableClick();
        leftButton.name = BUTTON_NAMES.LeftHand.ToString();
        leftButton.SetCaption("Left");
        leftButton.Listeners = new GameObject[1];
        leftButton.Listeners[0] = gameObject;

        iTween.MoveTo(leftButton.gameObject, iTween.Hash("x", -3.6f, "easeType", "easeInOutQuad", "time", buttonAppearenceAnimationTime));


        Button rightButton = Instantiate(ButtonPrefab, new Vector3(11.6f, -0.29f, 0.0f), Quaternion.identity) as Button;
        rightButton.DisableClick();
        rightButton.name = BUTTON_NAMES.RightHand.ToString();
        rightButton.SetCaption("Right");
        rightButton.Listeners = new GameObject[1];
        rightButton.Listeners[0] = gameObject;
        iTween.MoveTo(rightButton.gameObject, iTween.Hash("x", 3.6f, "easeType", "easeInOutQuad", "time", buttonAppearenceAnimationTime));

        yield return new WaitForSeconds(startingAnimationTime);

        leftButton.EnableClick();
        rightButton.EnableClick();
        state++;
    }

    /// <summary>
    /// All objects are moved out of the scene
    /// </summary>
    /// <returns></returns>
    IEnumerator FinalAnimation()
    {
        float finalAnimationTime = 1.0f;
        
        Transform[] allObjects = GameObject.FindObjectsOfType<Transform>();

        foreach (Transform t in allObjects)
        {
            if (t.tag != "MainCamera" && t.tag != "NoFinalAnimation")
            {
                iTween.MoveBy(t.gameObject, iTween.Hash("y", -10.0f, "easeType", "easeInOutQuad", "time", finalAnimationTime));//, "delay", Random.value * 0.5f));
            }
        }

        yield return new WaitForSeconds(finalAnimationTime);

        Application.LoadLevel(2);
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
    /// Message from Button Components
    /// </summary>
    /// <param name="name">Button name</param>
    void ButtonPressed(string name)
    {
        // If hand is selected
        if (state == HAND_CALIBRATION_LEVEL_STATES.HandSelection && (name == BUTTON_NAMES.LeftHand.ToString() || name == BUTTON_NAMES.RightHand.ToString()))
        {
            // hide and destroy buttons
            GameObject[] buttons = GameObject.FindGameObjectsWithTag("Button");
            foreach (GameObject button in buttons)
            {
                button.GetComponent<Button>().DisableClick();
                if (button.transform.position.x > 0)    iTween.MoveTo(button, iTween.Hash("x",  11.6f, "easeType", "easeInOutQuad", "oncomplete", "DestroyMe"));//, "time", 1.0f));
                else                                    iTween.MoveTo(button, iTween.Hash("x", -11.6f, "easeType", "easeInOutQuad", "oncomplete", "DestroyMe"));//, "time", 1.0f));
            }

            if (name == BUTTON_NAMES.LeftHand.ToString())
            {
                rightHand = false;
            }
            else if (name == BUTTON_NAMES.RightHand.ToString())
            {
                rightHand = true;
            }

            // Create HandCalibrator to use in next state
            HandCalibrator = Instantiate(HandCalibratorPrefab) as HandXYCalibrator;
            HandCalibrator.transform.position = new Vector2(0.0f, 0.0f);

            // Create button that must be pressed after calibration
            Button calibrationDone = Instantiate(ButtonWidePrefab, new Vector2(0.0f, 6.0f), Quaternion.identity) as Button;
            calibrationDone.name = BUTTON_NAMES.CalibrationDone.ToString();
            calibrationDone.SetCaption("Done");
            calibrationDone.Listeners = new GameObject[1];
            calibrationDone.Listeners[0] = gameObject;
            iTween.MoveTo(calibrationDone.gameObject, iTween.Hash("y", 3.1f, "easeType", "easeInOutQuad"));//, "time", buttonAppearenceAnimationTime));

            state++;
        }
        // If calibration is over
        else if (state == HAND_CALIBRATION_LEVEL_STATES.HandCalibration && name == BUTTON_NAMES.CalibrationDone.ToString())
        {
            // hide and destroy button
            GameObject[] buttons = GameObject.FindGameObjectsWithTag("Button");
            foreach (GameObject button in buttons)
            {
                button.GetComponent<Button>().DisableClick();
            }

            MinHandPosition = HandCalibrator.GetMinHandPosition();
            MaxHandPosition = HandCalibrator.GetMaxHandPosition();

            CalibrationData.Instance().SetData(MinHandPosition, MaxHandPosition, rightHand);

            state++;
            StartCoroutine(FinalAnimation());
        }
    }

    private void SetDataToHandCalibrator()
    {
        if (UseMouse)
        {
            // Simulate kinect by mouse
            float x = (Input.mousePosition.x) / Screen.width * 4.0f - 2.0f;
            if (x < -2.0f) x = -2.0f;
            else if (x > 2.0f) x = 2.0f;

            float y = (Input.mousePosition.y) / Screen.height * 4.0f - 2.0f;
            if (y < -2.0f) y = -2.0f;
            else if (y > 2.0f) y = 2.0f;

            HandCalibrator.SetHandPosition(new Vector2(x, y));
        }
        else
        {
            if (KinectManager.SkeletonIsTracked && GoodPlayerPosition)
            {
                if (rightHand)
                {
                    HandCalibrator.SetHandPosition(new Vector2(KinectManager.RightHandPosition.x, KinectManager.RightHandPosition.y));
                }
                else
                {
                    HandCalibrator.SetHandPosition(new Vector2(KinectManager.LeftHandPosition.x, KinectManager.LeftHandPosition.y));
                }
            }
        }
    }

    private void SetDataToTopView()
    {
        // In this scene we just set data to TopView
        // We also can track it's messages to act according to them
        if (TopView != null)
        {
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
    }


	// Update is called once per frame
	void Update () 
    {
        switch (state)
        {
            case HAND_CALIBRATION_LEVEL_STATES.StartingAnimation:
                break;
            case HAND_CALIBRATION_LEVEL_STATES.HandSelection:
                break;
            case HAND_CALIBRATION_LEVEL_STATES.HandCalibration:
                // Fill HandCalibrator with data
                SetDataToHandCalibrator();
                break;
            case HAND_CALIBRATION_LEVEL_STATES.FinalAnimation:
                break;
        }

        // Set data to TopView
        if (TopView != null)
        {
            SetDataToTopView();
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
