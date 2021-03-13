using UnityEngine;
using UnityEngine.UI;

namespace Client.Scripts.MiscScripts
{
    public class Compass : MonoBehaviour
    {
        [SerializeField] private RawImage compassImage;
        [SerializeField] private Transform player;

        private void Update()
        {
            compassImage.uvRect = new Rect(player.localEulerAngles.y / 360,0,1,1);
        }
    }
}
