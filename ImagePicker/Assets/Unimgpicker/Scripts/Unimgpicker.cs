using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Kakera
{
    public class Unimgpicker : MonoBehaviour
    {
        public delegate void ImageDelegate(Texture2D path);

        public delegate void ErrorDelegate(string message);

        public event ImageDelegate Completed;

        public event ErrorDelegate Failed;


        private IPicker picker = 
#if UNITY_IOS && !UNITY_EDITOR
            new PickeriOS();
#elif UNITY_ANDROID && !UNITY_EDITOR
            new PickerAndroid();
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
			new Picker_editor();
#else
            new PickerUnsupported();
#endif

        WebCamTexture webCamTexture;
        public GameObject objCamera;
        public RawImage rendererCamera;
        AspectRatioFitter fit;
        int maxSize = -1;
        public void ShowImagePicker(string title, string outputFileName, int maxSize)
        {
            this.maxSize = maxSize;
            picker.Show(title, outputFileName, maxSize);
        }
        public void ShowCamera(int maxSize)
        {
            this.maxSize = maxSize;
            if (webCamTexture == null)
            { 
                webCamTexture = new WebCamTexture();
                fit = rendererCamera.gameObject.GetComponent<AspectRatioFitter>();
            }
            objCamera.SetActive(true);
            webCamTexture.Play();
            rendererCamera.texture = webCamTexture;
        }
        public void OnTakePhoto()
        {
            Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
            photo.SetPixels(webCamTexture.GetPixels());
            photo.Apply();
            FinishLoadTexture(photo);
            OnBackCamera();
        }
        public void OnChangeCamera()
        {
            webCamTexture.Stop();
            webCamTexture.deviceName = (webCamTexture.deviceName == WebCamTexture.devices[0].name) ? WebCamTexture.devices[1].name : WebCamTexture.devices[0].name;
            webCamTexture.Play();
        }
        public void OnBackCamera()
        {
            objCamera.SetActive(false);
            webCamTexture.Stop();
        }
        void Update()
        {
            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                float ratio = (float)webCamTexture.width / (float)webCamTexture.height;
                fit.aspectRatio = ratio; // Set the aspect ratio
                float scaleY = webCamTexture.videoVerticallyMirrored ? -1f : 1f; // Find if the camera is mirrored or not
                rendererCamera.rectTransform.localScale = new Vector3(1f, scaleY, 1f); // Swap the mirrored camera
                int orient = -webCamTexture.videoRotationAngle;
                rendererCamera.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
            }
        }
        private void OnComplete(string path)
        {
            StartCoroutine(LoadImage(path));            
        }

        private void OnFailure(string message)
        {
            var handler = Failed;
            if (handler != null)
            {
                handler(message);
            }
        }
        void FinishLoadTexture(Texture2D texture)
        {
            if (texture.width > maxSize)
            {
                TextureScale.Bilinear(texture, maxSize, texture.height * maxSize / texture.width);
            }
            else if (texture.height > maxSize)
            {
                TextureScale.Bilinear(texture, texture.width * maxSize / texture.height, maxSize);
            }
            var handler = Completed;
            if (handler != null)
            {
                handler(texture);
            }
        }

        private IEnumerator LoadImage(string path)
        {
            var url = "file://" + path;
            var www = new WWW(url);
            yield return www;

            var texture = www.texture;
            if (texture == null)
            {
                OnFailure("Failed to load texture url:" + url);
            }
            else
            {
                FinishLoadTexture(texture);
            }

        }
    }
}