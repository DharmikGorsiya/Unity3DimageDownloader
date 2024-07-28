using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System;

public class ImageDownloader : MonoBehaviour
{
    private static ImageDownloader _instance;
    public static ImageDownloader Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("ImageDownloader");
                _instance = obj.AddComponent<ImageDownloader>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    public IEnumerator SetImageAsync(string imgUrl, MaskableGraphic uiElement)
    {
        if (uiElement != null)
        {
            yield return StartCoroutine(DownloadAndSetImage(imgUrl, uiElement));
        }
    }

    private IEnumerator DownloadAndSetImage(string imgUrl, MaskableGraphic uiElement)
    {
        // Extract the directory name from the URL
        string directoryName = ExtractDirectoryNameFromUrl(imgUrl);

        // Set the local directory path based on the platform
        string localDir = "";

        if (Application.platform == RuntimePlatform.Android)
        {
            localDir = Path.Combine(Application.persistentDataPath, directoryName);
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            localDir = Path.Combine(Application.persistentDataPath, directoryName);
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            localDir = Path.Combine(Application.dataPath, directoryName);
        }
        else
        {
            Debug.LogError("Platform not supported!");
            yield break;
        }

        // Ensure the directory exists
        if (!Directory.Exists(localDir))
        {
            Directory.CreateDirectory(localDir);
        }

        string fileName = Path.GetFileName(imgUrl);
        string localFilePath = Path.Combine(localDir, fileName);

        // Check if the file already exists
        if (File.Exists(localFilePath))
        {
            // Load the image from local storage
            yield return StartCoroutine(LoadImageFromLocal(localFilePath, uiElement));
        }
        else
        {
            // Download and save the image
            yield return StartCoroutine(DownloadImage(imgUrl, localFilePath, uiElement));
        }
    }

    private string ExtractDirectoryNameFromUrl(string url)
    {
        Uri uri = new Uri(url);
        string path = uri.AbsolutePath;
        string[] segments = path.Split('/');
        return segments.Length > 2 ? segments[segments.Length - 2] : "DefaultDirectory";
    }

    private IEnumerator DownloadImage(string url, string localFilePath, MaskableGraphic uiElement)
    {
        // Check if uiElement is null
        if (uiElement == null)
        {
            yield break;
        }

        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(webRequest.error);
            }
            else
            {
                // Get the downloaded texture
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);

                // Save the texture as a PNG file
                byte[] bytes = texture.EncodeToPNG();
                File.WriteAllBytes(localFilePath, bytes);

                // Apply the texture to the UI element
                ApplyTextureToElement(texture, uiElement);
            }
        }
    }

    private IEnumerator LoadImageFromLocal(string localFilePath, MaskableGraphic uiElement)
    {
        // Check if uiElement is null
        if (uiElement == null)
        {
            yield break;
        }

        // Load the image from local storage
        byte[] bytes = File.ReadAllBytes(localFilePath);

        // Create a texture and load the image data
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(bytes);

        // Apply the texture to the UI element
        ApplyTextureToElement(texture, uiElement);

        yield return null;
    }

    private void ApplyTextureToElement(Texture2D texture, MaskableGraphic uiElement)
    {
        if (uiElement is Image image)
        {
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else if (uiElement is RawImage rawImage)
        {
            rawImage.texture = texture;
        }
    }
}
