using UnityEngine;
using System.Collections;

public class CalibrationData
{
    /// <summary>
    /// Singleton Data
    /// </summary>
    private Vector2 MaxHandPosition = new Vector2(0.0f, 0.0f);
    private Vector2 MinHandPosition = new Vector2(0.0f, 0.0f);
    private bool RightHand = true;


    private static CalibrationData instance = null;

    private CalibrationData() { }

    public static CalibrationData Instance()
    {
        if (instance == null)
        {
            instance = new CalibrationData();
        }
        return instance;
    }

    public void SetData(Vector2 minHandPosition, Vector2 maxHandPosition, bool rightHand)
    {
        MinHandPosition = minHandPosition;
        MaxHandPosition = maxHandPosition;
        RightHand = rightHand;
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
    /// true if right hand is selected; false when left
    /// </summary>
    /// <returns></returns>
    public bool CheckIfRightHand()
    {
        return RightHand;
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
