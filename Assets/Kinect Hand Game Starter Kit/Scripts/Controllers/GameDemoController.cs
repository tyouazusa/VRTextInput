using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameDemoController : MonoBehaviour 
{
    /// <summary>
    /// Name of KinectManager singleton to find it and save a reference
    /// </summary>
    public string KinectManagerName = "My Kinect Manager";
    private MyKinectManager KinectManager;

    /// <summary>
    /// Prefabs
    /// </summary>
    public GameObject BallPrefab;

    // Link to current Ball
    private GameObject CurrentBall;

    // Link to the Hand
    public Hand CurrentHand;

    /// <summary>
    /// We can place on our scene a little TopViewPlayerPositionDetector to validate player's position
    /// </summary>
    public TopViewPlayerPositionDetector TopView;

    /// <summary>
    /// TopView Player Position
    /// </summary>
    public bool GoodPlayerPosition = false;

    /// <summary>
    /// Text Mesh in the same GameObject with this Component
    /// </summary>
    private TextMesh LevelTitle;

    // Kinect Manager will be on the scene from the previous scene, so we choose good position for it's text
    private Vector2 KinectManagerPosition = new Vector2(0.0f, -4.6f);

    public Rect BallPositionBounds = new Rect(-6.5f, -3.5f, 13.0f, 7.0f);

    /// <summary>
    /// Variables for extrapolation
    /// </summary>
    private List<float> xExtrapolationList = new List<float>();
    private List<float> yExtrapolationList = new List<float>();
    public int extrapolationCount = 5;


	// Use this for initialization
	void Start () 
    {
        // Find links to objects
        if (KinectManager == null)
            KinectManager = GameObject.Find(KinectManagerName).GetComponent<MyKinectManager>();

        if (KinectManager == null) Debug.LogError("No KinectManager Found On The Scene");

        LevelTitle = GetComponent<TextMesh>();
        LevelTitle.text = "Catch all the balls !!!";

        for(int i = 0; i < extrapolationCount; i++)
        {
            xExtrapolationList.Add(0.0f);
            yExtrapolationList.Add(0.0f);
        }

        // move KinectManager (with TextMesh in it)
        iTween.MoveTo(KinectManager.gameObject, iTween.Hash("x", KinectManagerPosition.x, "y", KinectManagerPosition.y, "easeType", "easeInOutQuad", "time", 1.0f));
	}

    private float GetNextXExtrapolationValue(float x)
    {
        // if extrapolationCount was changed from the debug panel
        if (xExtrapolationList.Count != extrapolationCount)
        {
            if (xExtrapolationList.Count < extrapolationCount)
            {
                // Add elements
                for (int i = 0; i < extrapolationCount - xExtrapolationList.Count; i++)
                {
                    xExtrapolationList.Add(0.0f);
                }
            }
            else if (xExtrapolationList.Count > extrapolationCount)
            {
                // Remove elements
                for (int i = 0; i < xExtrapolationList.Count - extrapolationCount; i++)
                {
                    xExtrapolationList.RemoveAt(0);
                }
            }
        }

        xExtrapolationList.Add(x);
        xExtrapolationList.RemoveAt(0);

        float average = 0.0f;
        foreach (float value in xExtrapolationList)
        {
            average += value;
        }
        average = average / (float)xExtrapolationList.Count;
        return average;
    }

    private float GetNextYExtrapolationValue(float y)
    {
        // if extrapolationCount was changed from the debug panel
        if (yExtrapolationList.Count != extrapolationCount)
        {
            if (yExtrapolationList.Count < extrapolationCount)
            {
                // Add elements
                for (int i = 0; i < extrapolationCount - yExtrapolationList.Count; i++)
                {
                    yExtrapolationList.Add(0.0f);
                }
            }
            else if (yExtrapolationList.Count > extrapolationCount)
            {
                // Remove elements
                for (int i = 0; i < yExtrapolationList.Count - extrapolationCount; i++)
                {
                    yExtrapolationList.RemoveAt(0);
                }
            }
        }

        yExtrapolationList.Add(y);
        yExtrapolationList.RemoveAt(0);

        float average = 0.0f;
        foreach (float value in yExtrapolationList)
        {
            average += value;
        }
        average = average / (float)yExtrapolationList.Count;
        return average;
    }

    private void SetDataToTopView()
    {
        // In this scene we just set data to TopView
        // We also can track it's messages to act according to them
        if (TopView != null)
        {
            if (KinectManager.SkeletonIsTracked)
            {
                TopView.SetHipCenterPosition(KinectManager.HipCenterPosition);
            }
        }
    }

    /// <summary>
    /// Message from TopView
    /// </summary>
    /// <param name="goodPlayerPosition"></param>
    public void ValidPlayerPosition(bool goodPlayerPosition)
    {
        GoodPlayerPosition = goodPlayerPosition;
    }

	// Update is called once per frame
	void Update () 
    {
        // If there is no ball, let's create a new one
        if (CurrentBall == null)
        {
            CurrentBall = Instantiate(BallPrefab) as GameObject;
            CurrentBall.transform.position = new Vector3(   UnityEngine.Random.Range(BallPositionBounds.xMin, BallPositionBounds.xMax), 
                                                            UnityEngine.Random.Range(BallPositionBounds.yMin, BallPositionBounds.yMax), 0.0f);
            
            // Simple scaling animation
            CurrentBall.transform.localScale = new Vector2(0.1f, 0.1f);
            iTween.ScaleTo(CurrentBall, iTween.Hash("x", 1.0f, "y", 1.0f, "easeType", "easeInOutQuad"));
        }

        if (KinectManager.SkeletonIsTracked && GoodPlayerPosition)
        {
            // Move our hand
            float calibratedX = 0.0f;
            float calibratedY = 0.0f;
            
            // Get data from kinect
            if (CalibrationData.Instance().CheckIfRightHand())
            {
                calibratedX = KinectManager.RightHandPosition.x;
                calibratedY = KinectManager.RightHandPosition.y;
            }
            else
            {
                calibratedX = KinectManager.LeftHandPosition.x;
                calibratedY = KinectManager.LeftHandPosition.y;
            }

            // Normalize
            calibratedX = (calibratedX - CalibrationData.Instance().GetMinHandPosition().x) / (CalibrationData.Instance().GetMaxHandPosition().x - CalibrationData.Instance().GetMinHandPosition().x);
            calibratedY = (calibratedY - CalibrationData.Instance().GetMinHandPosition().y) / (CalibrationData.Instance().GetMaxHandPosition().y - CalibrationData.Instance().GetMinHandPosition().y);
            if (calibratedX > 1) calibratedX = 1;
            if (calibratedX < 0) calibratedX = 0;
            if (calibratedY > 1) calibratedY = 1;
            if (calibratedY < 0) calibratedY = 0;

            // Now calibratedX and calibratedY are in [0; 1]
            // Lo let's add some smoothing
            calibratedX = GetNextXExtrapolationValue(calibratedX);
            calibratedY = GetNextYExtrapolationValue(calibratedY);

            //Debug.Log(calibratedX);

            CurrentHand.transform.position = new Vector2(BallPositionBounds.xMin + BallPositionBounds.width * calibratedX, BallPositionBounds.yMin + BallPositionBounds.height * calibratedY);
        }

        // Set data to TopView
        if (TopView != null)
        {
            SetDataToTopView();
        }
	}
}
