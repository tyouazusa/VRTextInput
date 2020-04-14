using UnityEngine;
using System.Collections;

public class HandXYCalibrator : MonoBehaviour 
{
    public SpriteRenderer CalibrationHand;
    public SpriteRenderer CalibrationTexture;

    /// <summary>
    /// Range of XY positions, that kinect give
    /// Experimental numbers.
    /// </summary>
    private const float minKinectXYCoordinate = -2.0f;
    private const float maxKinectXYCoordinate = 2.0f;
    
    
    /// <summary>
    /// Camera is in ortographic projection with size = 5.
    /// This variable is set at Start according to screen resolution
    /// </summary>
    private Rect ScreenBounds;
    private float CameraOrtographicSize = 5.0f;

    /// <summary>
    /// All the positions are stored in units from kinect. Nearly from -2 to 2
    /// This variable is public just to be seen on debug panel
    /// </summary>
    public Vector2 CurrentHandPosition = new Vector2(0, 0);
    public Vector2 PreviousHandPosition = new Vector2(0, 0);
    public Vector2 MinHandPosition = new Vector2(0, 0);
    public Vector2 MaxHandPosition = new Vector2(0, 0);

	// Use this for initialization
	void Start () 
    {
        ScreenBounds = new Rect(-CameraOrtographicSize / Screen.height * Screen.width, -CameraOrtographicSize, CameraOrtographicSize / Screen.height * Screen.width * 2.0f, CameraOrtographicSize * 2.0f);
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (CurrentHandPosition != PreviousHandPosition)
        {
            CalibrationHand.transform.position = new Vector2(KinectUnitsXToMeters(CurrentHandPosition.x), KinectUnitsYToMeters(CurrentHandPosition.y));
            PreviousHandPosition = CurrentHandPosition;
            UpdateCalibrationTexture();
        }
	}

    private void UpdateCalibrationTexture()
    {
        bool haveToRescaleTexture = false;

        // Check if position is less than min
        if (CurrentHandPosition.x < MinHandPosition.x)
        {
            MinHandPosition = new Vector2(CurrentHandPosition.x, MinHandPosition.y);
            haveToRescaleTexture = true;
        }
        if (CurrentHandPosition.y < MinHandPosition.y) 
        {
            MinHandPosition = new Vector2(MinHandPosition.x, CurrentHandPosition.y);
            haveToRescaleTexture = true;
        }

        // Check if position is bigger than max
        if (CurrentHandPosition.x > MaxHandPosition.x)
        {
            MaxHandPosition = new Vector2(CurrentHandPosition.x, MaxHandPosition.y);
            haveToRescaleTexture = true;
        }
        if (CurrentHandPosition.y > MaxHandPosition.y)
        {
            MaxHandPosition = new Vector2(MaxHandPosition.x, CurrentHandPosition.y);
            haveToRescaleTexture = true;
        }

        // Rescale Texture if needed
        if (haveToRescaleTexture)
        {
            float scaleX = KinectUnitsXToMeters(MaxHandPosition.x) - KinectUnitsXToMeters(MinHandPosition.x);
            float scaleY = KinectUnitsYToMeters(MaxHandPosition.y) - KinectUnitsYToMeters(MinHandPosition.y);
            
            CalibrationTexture.transform.position = new Vector2(KinectUnitsXToMeters(MinHandPosition.x), KinectUnitsYToMeters(MinHandPosition.y));

            CalibrationTexture.transform.localScale = new Vector2(scaleX / 10.0f, scaleY / 10.0f); // We devide scale by 10.0 because real width of PNG image is 1000
        }
    }

    private float KinectUnitsXToMeters(float x)
    {
        return ScreenBounds.xMin + ScreenBounds.width * ((x - minKinectXYCoordinate) / (maxKinectXYCoordinate - minKinectXYCoordinate));
    }

    private float KinectUnitsYToMeters(float y)
    {
        return ScreenBounds.yMin + ScreenBounds.height * ((y - minKinectXYCoordinate) / (maxKinectXYCoordinate - minKinectXYCoordinate));
    }

    /// <summary>
    /// HipCenterPosition x, y are ranged in [-1; 1]; z is distance to sensor in meters
    /// </summary>
    /// <param name="hcp"></param>
    public void SetHandPosition(Vector2 hp)
    {
        CurrentHandPosition = hp;
    }

    /// <summary>
    /// The result is in units from kinect. Nearly from -2 to 2
    /// </summary>
    /// <returns></returns>
    public Vector2 GetMinHandPosition()
    {
        return MinHandPosition;
    }

    /// <summary>
    /// The result is in units from kinect. Nearly from -2 to 2
    /// </summary>
    /// <returns></returns>
    public Vector2 GetMaxHandPosition()
    {
        return MaxHandPosition;
    }

}
