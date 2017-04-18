using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System.IO;

public class OneTimeScreenshooter : MonoBehaviour {

	// Use this for initialization
	void Start () {
        TakeScreenshot();
	}

    private void TakeScreenshot()
    {
        int resWidth = Screen.width;
        int resHeight = Screen.height;

        Camera camera = this.gameObject.GetComponent<Camera>();

        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = rt;

        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        RenderTexture.active = camera.targetTexture;
        camera.Render();

        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screenShot.Apply();

        SaveScreenshot(screenShot, Application.dataPath + "/Scenes/");

        camera.targetTexture = null;
        RenderTexture.active = null;
        
        DestroyImmediate(rt);
    }

    private void SaveScreenshot(Texture2D screenshot, string directory)
    {
        byte[] bytes = screenshot.EncodeToPNG();
        string filename = directory + EditorSceneManager.GetActiveScene().name + ".png";

        if (!File.Exists(filename))
        {
            File.WriteAllBytes(filename, bytes);
        }       
    }


}
