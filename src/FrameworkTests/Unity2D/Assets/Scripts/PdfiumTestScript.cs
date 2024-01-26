using SkiaSharp;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PdfiumTestScript : MonoBehaviour
{
    [SerializeField]
    private Text _title;

    [SerializeField]
    private RawImage _rawImage;

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            var test = new SkiaSharp.SKBitmap();

            using UnityWebRequest www = UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, "SocialPreview.pdf"));
            www.SendWebRequest();

            while (!www.isDone)
            {
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    throw new Exception($"Connection Error while reading File.");
                }
            }

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                throw new Exception($"Connection Error while reading File");
            }

            using var input = new MemoryStream(www.downloadHandler.data);
            using var bitmap = PDFtoImage.Conversion.ToImage(input);
            var output = $"SocialPreview.pdf size: {bitmap.Width}x{bitmap.Height}";

            if (_title != null)
                _title.text = output;

            if (_rawImage != null)
            {
                var texture = new Texture2D(bitmap.Width, bitmap.Height);
                using var ms = new MemoryStream();
                bitmap.Encode(ms, SKEncodedImageFormat.Png, 100);
                ms.Position = 0;
                texture.LoadImage(ms.ToArray());
                Debug.Log(_rawImage.rectTransform.localScale);
                _rawImage.rectTransform.sizeDelta = new Vector2(bitmap.Width / 10, bitmap.Height / 10);
                _rawImage.texture = texture;
            }

            Debug.Log(output);
        }
        catch (Exception ex)
        {
            if (_title != null)
                _title.text = ex.ToString();

            Debug.Log(ex.ToString());
        }
    }
}