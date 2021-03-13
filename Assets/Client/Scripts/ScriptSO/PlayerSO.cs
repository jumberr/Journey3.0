using UnityEngine;

namespace Client.Scripts.ScriptSO
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerSO")]
    public class PlayerSO : ScriptableObject
    {
        [SerializeField] private float health;
        [SerializeField] private float speed = 6f;
        [SerializeField] private float speedRunMultiplayer = 2f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpForce = 2f;
        [SerializeField] private AudioClip walkingAudio;
        [SerializeField] private AudioClip runningAudio;
        public float Health => health;
        public float MoveSpeed => speed;
        public float SpeedRunMultiplayer => speedRunMultiplayer;
        public float Gravity => gravity;
        public float JumpForce => jumpForce;
        public AudioClip WalkingAudio => walkingAudio;
        public AudioClip RunningAudio => runningAudio;
    }
}