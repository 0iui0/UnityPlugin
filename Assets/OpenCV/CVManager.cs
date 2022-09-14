using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System;

public class CVManager : MonoBehaviour
{
    private const string LIBRARY_NAME = "unitycv";

    [DllImport(LIBRARY_NAME)]
    private static extern void InitOpenCVFrame(int width, int height);

    [DllImport(LIBRARY_NAME)]
    private static extern void GetFeatures(ref Color32[] rawData, IntPtr points);

	// bool camAvailable;
    WebCamTexture webcamTexture;
    Texture2D texture;
    public RawImage cameraView;
	public bool frontFacing;

    struct CornerInfo 
    {
        public int x;
        public int y;
    };

    CornerInfo[] corners;

	// Use this for initialization
    void Start() {
		WebCamDevice[] devices = WebCamTexture.devices;
		
        if (devices.Length == 0)
			return;

		for (int i = 0; i < devices.Length; i++)
		{
			var curr = devices[i];

			if (curr.isFrontFacing == frontFacing)
			{
				webcamTexture = new WebCamTexture(curr.name, 640, 320, 60);
				break;
			}
		}	

		if (webcamTexture == null)
			return;
        		
        webcamTexture.Play (); // Start the camera
        texture = new Texture2D(webcamTexture.width, webcamTexture.height);
		cameraView.texture = texture; // Set the texture
        
        // camAvailable = true; // Set the camAvailable for future purposes.

        corners = new CornerInfo[20];
        InitOpenCVFrame(webcamTexture.width, webcamTexture.height);
    }

    Color32[] rawColors;

    void ProcessOpenCVFrame()
    {
        rawColors = webcamTexture.GetPixels32();
        unsafe
        {
            fixed (CornerInfo* cornerInfo = corners)
            {
                GetFeatures(ref rawColors, (IntPtr)cornerInfo);
            }
        }
        Debug.Log($"Corner: (" + corners[0].x + ", " + corners[0].y + ")");
        Debug.Log($"rawColors: (" + rawColors[0].ToString() + ")");
        texture.SetPixels32(rawColors);
        texture.Apply();
        cameraView.texture = texture; // Set the texture
    }
    
    // Update is called once per frame
    void Update() {
        ProcessOpenCVFrame();
    }
}