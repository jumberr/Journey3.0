using Client.Classes;
using Client.Scripts.Misc_Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Client.Scripts.PlayerScripts
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private CharacterController characterController;

        [Header("Audio:")]
        [SerializeField] private AudioSource source;
        private AudioClip walkingAudio;
        private AudioClip runningAudio;
        
        private float speed;
        private float speedRunMultiplayer;
        private float gravity;
        private float jumpForce;
        
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int Walk = Animator.StringToHash("Walk");
        private static readonly int Run = Animator.StringToHash("Run");
        private static readonly int Jump = Animator.StringToHash("Jump");
        
        private Vector3 velocity;
        private bool isGround;
        
        private UnityEvent staminaUp;
        private UnityEvent staminaDown;
        private JumpEvent staminaDownJump;

        private enum State
        {
            Idle,
            Walk,
            Run
        }
        private State playerState;

        public float stamina;
        public float staminaLess;
        public float staminaRenew;
        private void Awake()
        {
            if (staminaUp == null)
                staminaUp = new UnityEvent();
            if (staminaDown == null)
                staminaDown = new UnityEvent();
            if (staminaDownJump == null)
                staminaDownJump = new JumpEvent();

            staminaUp.AddListener(RenewStamina);
            staminaDown.AddListener(LessStamina);
            staminaDownJump.AddListener(LessStaminaJump);

            InitializeStats();
        }

        private void InitializeStats()
        {
            speed = Player.localPlayer.MoveSpeed;
            speedRunMultiplayer = Player.localPlayer.PlayerSo.SpeedRunMultiplayer;
            gravity = Player.localPlayer.PlayerSo.Gravity;
            jumpForce = Player.localPlayer.PlayerSo.JumpForce;
            walkingAudio = Player.localPlayer.PlayerSo.WalkingAudio;
            runningAudio = Player.localPlayer.PlayerSo.RunningAudio;
        }

        private void Update()
        {
            isGround = characterController.isGrounded; // check for ground

            if (isGround && velocity.y < 0)
                velocity.y = -2f; //We're standing at the floor

            //movement
            var vertical = Input.GetAxis("Vertical");
            var horizontal = Input.GetAxis("Horizontal");

            var direction = transform.right * horizontal + transform.forward * vertical; //Get direction were we go
        
            staminaUp.Invoke();
            playerState = State.Idle;
            if (horizontal != 0 || vertical != 0) // walk
            {
                playerState = State.Walk;
            }
            if (Input.GetKey(KeyCode.LeftShift) && Player.localPlayer.StaminaCurrent > 0 && (horizontal != 0 || vertical != 0)) // if player use shift x2 speed
            {
                direction *= speedRunMultiplayer;
                playerState = State.Run;
                staminaDown.Invoke();
            }

            characterController.Move(direction * (speed * Time.deltaTime));

            //jumps
            if (isGround && Input.GetButtonDown("Jump"))
            {
                staminaDownJump.Invoke(Player.localPlayer.StaminaLess);
            }
            //gravity
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
            //anims
            SetAnimation(playerState);
        }
        private void LessStamina()
        {
            if (Player.localPlayer.StaminaCurrent > 0 && Input.GetKey(KeyCode.LeftShift))
            {
                Player.localPlayer.StaminaCurrent -= Player.localPlayer.StaminaLess * Time.deltaTime;
                CanvasController.UI.SliderStamina.value = Player.localPlayer.StaminaCurrent / Player.localPlayer.Stamina;
            }
        }
        private void LessStaminaJump(float value)
        {
            if (Player.localPlayer.StaminaCurrent > value)
            {
                Player.localPlayer.WeaponController.IsAiming = false;
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                Player.localPlayer.StaminaCurrent -= value;
                Player.localPlayer.PlayerAnimator.SetTrigger(Jump);
                CanvasController.UI.SliderStamina.value = Player.localPlayer.StaminaCurrent / Player.localPlayer.Stamina;
            }
        }
    
        private void RenewStamina()
        {
            if (Player.localPlayer.StaminaCurrent < Player.localPlayer.Stamina && !Input.GetKey(KeyCode.LeftShift))
            {
                Player.localPlayer.StaminaCurrent += Player.localPlayer.StaminaRenew * Time.deltaTime;
                CanvasController.UI.SliderStamina.value = Player.localPlayer.StaminaCurrent / Player.localPlayer.Stamina;
            }
        }
    
        private void AnimationState(bool idle, bool walk, bool run)
        {
        
            Player.localPlayer.PlayerAnimator.SetBool(Idle, idle);
            Player.localPlayer.PlayerAnimator.SetBool(Walk, walk);
            Player.localPlayer.PlayerAnimator.SetBool(Run, run);
        }
    
        private void SetAnimation(State state)
        {
            switch (state)
            {
                case State.Idle:
                    AnimationState(true,false,false);
                    if (source.isPlaying)
                    {
                        source.Stop();
                    }
                    break;
                case State.Walk:
                    AnimationState(false,true,false);
                    // enable walking steps
                    if (!source.isPlaying)
                    {
                        source.volume = 0.5f;
                        source.PlayOneShot(walkingAudio);
                    }
                    break;
                case State.Run:
                    AnimationState(false,false,true);
                    // enable running steps
                    if (!source.isPlaying)
                    {
                        source.volume = 1f;
                        source.PlayOneShot(runningAudio);
                    }
                    break;
                default:
                    AnimationState(true,false,false);
                    break;
            }
        }
    }

    public class JumpEvent : UnityEvent<float> {}
}