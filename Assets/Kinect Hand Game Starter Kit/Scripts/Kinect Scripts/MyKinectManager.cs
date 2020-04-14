using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

public class MyKinectManager : MonoBehaviour
{
    /// <summary>
	/// Variables for outdoor communication
	/// </summary>

	public bool SkeletonIsTracked = false;
    public Vector3 RightHandPosition = new Vector3(-10f, -10f, -10f); // x, y ranged from -2 to 2, z is distance in meters
    public Vector3 LeftHandPosition = new Vector3(-10f, -10f, -10f);  // x, y ranged from -2 to 2, z is distance in meters
    public Vector3 HipCenterPosition = new Vector3(-10f, -10f, -10f); // x, y ranged from -2 to 2, z is distance in meters

    public static float MIN_XY_VALUE = -2.0f;
    public static float MAX_XY_VALUE = 2.0f;

	/// <summary>
	/// Public Configuration Variables
	/// </summary>

	public bool VideoOnGUI = false;
    public bool SmallVideo = true;
    public bool VideoOnObject = true;
    
    /// <summary>
    /// KinectManager has an option to show video on object if VideoOnObject == true
    /// KinectManager looks for an object with this name and trys to show vedeo on it
    /// This is useful when you change the scene, so Kinect Manager (singleton + DontDestroyOnLoad) can fing appropriate object on another level
    /// </summary>
    
    public string VideoObjectName = "VideoHolder";

    /// <summary>
    /// Pointers
    /// </summary>
    private TextMesh KinectText;
	private GameObject KinectVideo;
	
	
	// Singleton
	private static MyKinectManager instance;
	
	// Bool to keep track of whether Kinect has been initialized
	private bool KinectInitialized = false;
    
	// Video position on GUI, if used
	private Vector2 videoPosition = new Vector2(10, 10);

	// Color image data, if used
	private Color32[] colorImage;
	private Texture2D usersClrTex;
	private Rect usersClrRect;
	
	// Image stream handles for the kinect
	private IntPtr colorStreamHandle;

    // Data about skeletons
	private List<uint> lastFrameTrackedSkeletonDWTrackingId = new List<uint>();
	private bool skeletonWasTrackedOnLastFrame = false;

	// Skeleton related structures
	private KinectWrapper.NuiSkeletonFrame skeletonFrame;
	private KinectWrapper.NuiTransformSmoothParameters smoothParameters;

	// returns the single myKinecVideoManager instance
	public static MyKinectManager Instance
	{
		get
		{
			return instance;
		}
	}
	
	// checks if Kinect is initialized and ready to use. If not, there was an error during Kinect-sensor initialization
	public static bool IsKinectInitialized()
	{
		return instance != null ? instance.KinectInitialized : false;
	}

    /// <summary>
    /// MonoBehaviour function
    /// </summary>
	public void Awake()
	{
		if (instance)
		{
			Destroy(gameObject);
		}
		else
		{
			DontDestroyOnLoad(gameObject);
			instance = this;
		}
	}

    /// <summary>
    /// MonoBehaviour function
    /// </summary>
    public void Start()
	{
        KinectText = gameObject.GetComponent<TextMesh>();

        InitializeKinect();
	}

	/// <summary>
    /// This function usually works up to a second =)
	/// </summary>
    void InitializeKinect()
	{
        if (KinectText != null)
			KinectText.text = "Starting Kinect initialization";

        // Error code.
		int hr = 0;
		try
		{
			// Initialize kinect for skeleton tracking and video
			hr = KinectWrapper.NuiInitialize(KinectWrapper.NuiInitializeFlags.UsesColor | KinectWrapper.NuiInitializeFlags.UsesSkeleton);
			if (hr != 0)
			{
				if (KinectText != null)
				{
					KinectText.text = "Failed to Initialize Kinect";
				}
				throw new Exception("NuiInitialize Failed");
			}

			// Initialize video
			colorStreamHandle = IntPtr.Zero;
			hr = KinectWrapper.NuiImageStreamOpen(KinectWrapper.NuiImageType.Color, 
			                                      KinectWrapper.Constants.ImageResolution, 0, 2, IntPtr.Zero, ref colorStreamHandle);
			if (hr != 0)
			{
				if (KinectText != null)
					KinectText.text = "Failed to Initialize Color Stream";
				throw new Exception("Cannot open color stream");
			}

			// Initialize skeleton
			hr = KinectWrapper.NuiSkeletonTrackingEnable(IntPtr.Zero, 8);  // 0, 12,8
			if (hr != 0)
			{
				if (KinectText != null)
					KinectText.text = "Failed to Initialize Skeleton Stream";
				throw new Exception("Cannot initialize Skeleton Data");
			}

			// init skeleton structures
			skeletonFrame = new KinectWrapper.NuiSkeletonFrame()
			{
				SkeletonData = new KinectWrapper.NuiSkeletonData[KinectWrapper.Constants.NuiSkeletonCount]
			};
			
			// values used to pass to smoothing function
			smoothParameters = new KinectWrapper.NuiTransformSmoothParameters();
            smoothParameters.fSmoothing = 0.5f;
            smoothParameters.fCorrection = 0.5f;
            smoothParameters.fPrediction = 0.5f;
            smoothParameters.fJitterRadius = 0.05f;
            smoothParameters.fMaxDeviationRadius = 0.04f;

			// set kinect elevation angle
			//KinectWrapper.NuiCameraElevationSetAngle(15);

		}
		catch(DllNotFoundException e)
		{
			if (KinectText != null)
				KinectText.text = "Please check the Kinect SDK installation.";
			string message = "Please check the Kinect SDK installation.";
			Debug.LogError(message);
			Debug.LogError(e.ToString());
            return;
		}
		catch (Exception e)
		{
			string message = e.Message + " - " + KinectWrapper.GetNuiErrorString(hr);
			if (KinectText != null)
			{
                if ((uint)hr == (uint)KinectWrapper.NuiErrorCodes.DeviceNotConnected)
                    KinectText.text = "O-ops, where is my kinect";//"Упс, де мій Кінект ???";
                else if ((uint)hr == (uint)KinectWrapper.NuiErrorCodes.DeviceNotPowered)
                    KinectText.text = "O-ops, why my kinect is not powered";//"Упс, чому Кінект не у розетці ???";
                else if ((uint)hr == (uint)KinectWrapper.NuiErrorCodes.DeviceInUse)
                    KinectText.text = "O-ops, why someone else is using my kinect";//"Упс, хто використовує мій Кінект ???";
                else KinectText.text = message;

			}
			Debug.LogError(message);
            return;
		}
		
		
		// Initialize color map related stuff
		usersClrTex = new Texture2D(KinectWrapper.GetDepthWidth(), KinectWrapper.GetDepthHeight());
        if (SmallVideo)
        {
            usersClrRect = new Rect(10, 10, 160, 120);
        }
        else
        {
            usersClrRect = new Rect(10, 10, usersClrTex.width, usersClrTex.height);
        }
		
		colorImage = new Color32[KinectWrapper.GetDepthWidth() * KinectWrapper.GetDepthHeight()];
		
		KinectInitialized = true;
		if (KinectText != null)
			KinectText.text = "Kinect initialization successful";
	}

    /// <summary>
    /// MonoBehaviour function
    /// </summary>
	void Update()
	{

        // If we load another scene, kinect manager will not be destroyed, but all the other objects wiil be, so all the references will be lost
		if (VideoOnObject && KinectVideo == null)
		{
			KinectVideo = GameObject.Find(VideoObjectName);
		}
		
		if(KinectInitialized)
		{
			if (VideoOnObject || VideoOnGUI)
			{
				if(colorStreamHandle != IntPtr.Zero && KinectWrapper.PollColor(colorStreamHandle, ref colorImage))
				{
					usersClrTex.SetPixels32(colorImage);
					usersClrTex.Apply();

					if (VideoOnObject && KinectVideo != null)
					{
						KinectVideo.GetComponent<Renderer>().material.mainTexture = usersClrTex;
					}
				}
			}

			if (KinectWrapper.PollSkeleton(ref smoothParameters, ref skeletonFrame))
			{
				ProcessSkeleton();
			}
		}
		
	}

	// Make sure to kill the Kinect on quitting.
	void OnApplicationQuit()
	{
		if(KinectInitialized)
		{
			// Shutdown OpenNI
			KinectWrapper.NuiShutdown();
			instance = null;
		}
	}

	/// <summary>
    /// Process the skeleton data 
    /// </summary>
	void ProcessSkeleton()
	{
		List<KinectWrapper.NuiSkeletonData> allTrackedSkeletons = new List<KinectWrapper.NuiSkeletonData>();
		
		// Look for skeletons, that are tracked  now and on the previous frame
		for (int i = 0; i < KinectWrapper.Constants.NuiSkeletonCount; i++)
		{
			KinectWrapper.NuiSkeletonData skeletonData = skeletonFrame.SkeletonData[i];
			if (skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked && lastFrameTrackedSkeletonDWTrackingId.Contains(skeletonData.dwTrackingID))
			{
				allTrackedSkeletons.Add(skeletonData);
			}
		}
		
		// Now update lastFrameTrackedSkeletonDWTrackingId
		lastFrameTrackedSkeletonDWTrackingId.RemoveRange(0, lastFrameTrackedSkeletonDWTrackingId.Count);
		for (int i = 0; i < KinectWrapper.Constants.NuiSkeletonCount; i++)
		{
			KinectWrapper.NuiSkeletonData skeletonData = skeletonFrame.SkeletonData[i];
			if (skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
			{
				lastFrameTrackedSkeletonDWTrackingId.Add(skeletonData.dwTrackingID);
			}
		}
		
		// Now work with skeletons that are fully tracked at least two frames long
		if (allTrackedSkeletons.Count < 1)
		{
			// if skeleton was tracked last frame and is not tracked now then ...
			// public variable
			SkeletonIsTracked = false;
            RightHandPosition = new Vector3(-10f, -10f, -10f);
            LeftHandPosition = new Vector3(-10f, -10f, -10f);
            HipCenterPosition = new Vector3(-10f, -10f, -10f);

			if (KinectText != null)
				KinectText.text = "No players ...";//"Не бачу гравця ...";
			return;
		}

		// if skeleton is tracked now but was not tracked last frame then ...
		if (SkeletonIsTracked == false)
		{
			SkeletonIsTracked = true;
		}
		
		// Choose the closest tracked skeleton
		KinectWrapper.NuiSkeletonData activeSkeletonData;

		if (allTrackedSkeletons.Count == 1)
		{
			activeSkeletonData = allTrackedSkeletons[0];
            if (KinectText != null)
                KinectText.text = "I see one player.";//"Бачу гравця: ";//+ activeSkeletonData.dwTrackingID;
		}
		else
		{
			// sort skeletons by Z position and then choose the closest
			for (int i = 0; i < allTrackedSkeletons.Count - 1; i++)
			{
				for (int j = i + 1; j < allTrackedSkeletons.Count; j++)
				{
					if (allTrackedSkeletons[i].Position.z > allTrackedSkeletons[j].Position.z)
					{
						KinectWrapper.NuiSkeletonData temp = allTrackedSkeletons[i];
						allTrackedSkeletons[i] = allTrackedSkeletons[j];
						allTrackedSkeletons[j] = allTrackedSkeletons[i];
					}
				}
			}
			activeSkeletonData = allTrackedSkeletons[0];
            if (KinectText != null)
                KinectText.text = "I see too many players.";//"Надто багато гравців.";// + allTrackedSkeletons.Count.ToString();
		}

		// Now activeSkeletonData is active tracked skeleton
        HipCenterPosition = new Vector3(activeSkeletonData.SkeletonPositions[0].x, activeSkeletonData.SkeletonPositions[0].y, activeSkeletonData.SkeletonPositions[0].z);   // 0 == HipCenter
        RightHandPosition = new Vector3(activeSkeletonData.SkeletonPositions[11].x, activeSkeletonData.SkeletonPositions[11].y, activeSkeletonData.SkeletonPositions[11].z);// 11 == HandRight
        LeftHandPosition = new Vector3(activeSkeletonData.SkeletonPositions[7].x, activeSkeletonData.SkeletonPositions[7].y, activeSkeletonData.SkeletonPositions[7].z);    // 7 == HandLeft

        return;
	}

    /// <summary>
    /// MonoBehaviour function
    /// Draws the Histogram Map on the GUI.
    /// </summary>
    public void OnGUI()
    {
        if (KinectInitialized && VideoOnGUI)
        {
            GUI.DrawTexture(usersClrRect, usersClrTex);
        }
    }
	
}


