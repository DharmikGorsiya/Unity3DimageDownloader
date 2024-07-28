using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ImageManager : MonoBehaviour
{
    public string imageUrl = "http://example.com/path/to/your/image.png";
    public Image targetImage;
    public bool forceDownload = true;

    private void Start()
    {
        StartCoroutine(LoadAndSetImage());
    }

    private IEnumerator LoadAndSetImage()
    {
        yield return ImageDownloader.Instance.SetImageAsync(imageUrl, targetImage, forceDownload);

        // Code here will execute after the image has been set
        Debug.Log("Image download and set complete");
    }
}
