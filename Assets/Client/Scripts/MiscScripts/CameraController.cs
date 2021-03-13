using UnityEngine;

namespace Client.Scripts.MiscScripts
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private Transform playerBody;
        [SerializeField] private float xRotation = 0f;

        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            var moveX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            var moveY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= moveY;
            xRotation = Mathf.Clamp(xRotation, -90, 90);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            playerBody.Rotate(Vector3.up * moveX);
        }
    }
}
