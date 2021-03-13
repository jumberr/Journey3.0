using Client.Scripts.ScriptSO;
using UnityEngine;

namespace Client.Scripts.PlayerScripts
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerSO playerSo;

        public PlayerSO PlayerSo
        {
            get => playerSo;
            set => playerSo = value;
        }
    }
}
