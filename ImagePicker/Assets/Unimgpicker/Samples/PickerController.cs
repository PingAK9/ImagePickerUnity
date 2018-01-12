using UnityEngine;
using System.Collections;

namespace Kakera
{
    public class PickerController : MonoBehaviour
    {
        [SerializeField]
        private Unimgpicker imagePicker;

        [SerializeField]
        private MeshRenderer imageRenderer;

        void Awake()
        {
            imagePicker.Completed += (Texture2D tex) =>
            {
                imageRenderer.material.mainTexture = tex;
            };
        }

        public void OnPressShowPicker()
        {
            imagePicker.ShowImagePicker("Select Image", "unimgpicker", 512);
        }
        public void OnPressShowCamera()
        {

            imagePicker.ShowCamera(512);
        }
    }
}