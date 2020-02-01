using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityStandardAssets.Cameras;
using UnityStandardAssets.Utility;
using UnityEngine.Animations.Rigging;
using System;
/// <summary>
/// The awesomest player controller around by @Sidwasnothere
/// </summary>

public class Player : MonoBehaviour
{


    #region STATIC PROPERTIES
    private float handAboveHeadHeight = 0.3f;
    private float megamanHeight = 1.63f;

    private const float m_StationaryTurnSpeed = 5;
    private const float m_MovingTurnSpeed = 10;
    [SerializeField] private int m_JumpCount = 0;
    [SerializeField] private int m_DashCount = 0;
    [SerializeField] LayerMask updateStateLayerMask;
    #endregion

    #region CONTROLLABLE NONSTATIC PROPERTIES
    [Header("Controllable Properties")]
    private float m_MoveMultiplier = 6f;
    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)]
    float maxGroundSnapSpeed = 100f;
    [SerializeField, Range(0f, 5f)]
    float snapDistance = 0.5f;
    [SerializeField, Range(0f, 100f)]
    public float m_GroundCheckDistance = 0.1f;
    [SerializeField]
    LayerMask snapMask = -1; //configure player snap-probe to probe all layers specified in the editor. We're mostly trying to disclude props/damage objects/etc that dont make sense to snap to

    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f;
    [SerializeField, Range(0f, 100f)]
    float maxAirAcceleration = 1f;

    [SerializeField, Range(0f, 100f)]
    public float FallRateModifier = 1.5f;

    [SerializeField, Range(0f, 100f)] //Note: Running upside down is cool. Other Note: This is only here in case I want to switch to a hard angle limit on running. Atm I think some kind of stamina system is better
    float maxGroundAngle = 45f;

    [SerializeField, Range(0f, 10f)]
    public float m_WallRunMultiplier = 1.5f;
    [SerializeField, Range(0f, 100f)]
    public float wallRunMaxDistance = 10f;
    [SerializeField, Range(0f, 100f)]
    public float wallRunMaxHeight = 6f;
    [SerializeField, Range(0f, 100f)]
    public float wallRunSlowSpeed = 2f;
    [SerializeField, Range(0f, 100f)]
    public float wallRunForce;

    [SerializeField, Range(0f, 100f)]
    public float m_DashMultiplier = 5f;


    [SerializeField, Range(0f, 20f)]
    public float m_jumpHeight = 0.5f;
    [SerializeField, Range(0, 20)]
    public int m_MaxJumpCount = 3;

    [SerializeField, Range(0f, 100f)]
    public float maxLockOnDistance = 70f;

    [SerializeField, Range(0f, 10f)]
    public float dodgeAmount = 1f;

    #endregion

    #region NON-CONTROL VARIABLE PROPERTIES
    [Header("Visual Debug Values (READ ONLY AT EDITOR LEVEL)")]
    public bool m_IsNearWall;
    public bool m_IsGrounded;
    public bool m_IsWallRunning;
    public bool m_IsInAir = false;
    public bool m_canLedgeGrab = false;
    public bool m_IsLedgeGrabbing = false;
    public bool m_canWallRun = false;
    public string movementType = "grounded";
    public bool m_IsDodging = false;
    [SerializeField] private int lockedIndex = -1;
    public List<Transform> lockableEnemies;
    public Transform lockedEnemy;
    public bool lockedOn;
    public float lastLockTime;

    [SerializeField] public float maxSpeedChange;
    private int stepsSinceLastGrounded = 0;
    private int stepsSinceLastJump = 0;
    private int stepsSinceLastDodge = 0;

    [SerializeField] private Vector3 desiredVelocity;
    private Vector3 velocity;

    private int busterShotIndex = -1;
    [SerializeField]private Vector3 m_GroundNormal;

    private int wallSide;
    private bool isVerticalWallRun;
    private float onWallRotation;
    private float turnSmoothVelocity;
    private Vector3 wallBitangent;
    private Vector3 m_CamForward;
    private Vector3 m_CamRight;
    private Vector3 newMoveDirection;
    private Quaternion tiltTarget;
    private Vector3 tiltDir;
    #endregion

    #region COMPONENT REFS
    private PlayerInput playerInput;
    private Rigidbody m_Rigidbody;
    private Animator m_Animator;
    private Transform m_Armature;
    private Transform grabbedTransform;
    public List<Transform> Buster_Shots;
    private Vector3 m_closestVertex;
    private AudioSource audioSource;
    [Header("Necessary Component Refs")]
    [SerializeField] public Camera Camera;
    [SerializeField] private List<MultiAimConstraint> lookAtBones;
    [SerializeField] public Transform pointOfInterest;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private List<AudioClip> SFX;
    #endregion
    
    #region EVENTS
    public delegate void LockOnEvent(object sender, LockOnEventArgs e);
    public event LockOnEvent LockOnChanged;
    public LockOnEventArgs eventArgs;

    protected virtual void OnLockOnChanged(LockOnEventArgs e)
    {
        e.lockedOn = lockedOn;
        e.lockedEnemy = lockedEnemy;
        LockOnChanged?.Invoke(this, e);

    }
    #endregion

    #region CHARACTER LOGIC

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        playerInput = GetComponent<PlayerInput>();
        Camera = Camera.main;
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Animator = GetComponent<Animator>();
        m_Armature = transform.Find("Megaman_Armature");
        eventArgs = new LockOnEventArgs();
    }

    /// <summary>
    /// TODO:
    /// 1. Validate order of execution here later
    /// </summary>
    private void FixedUpdate()
    {
        //Update player state
        DebugUpdateState();
        UpdateState();


        m_CamForward = Camera.transform.forward;
        m_CamRight = Camera.transform.right;
        OnMovement();

    }

    private void Update()
    {
        m_CamForward = Camera.transform.forward;
        m_CamRight = Camera.transform.right;
    }

    //Music triggers live here
    private void OnCollisionEnter(Collision collision)
    {

        //Move's body toward velocity when colliding
        //y-component here deal with getting stuck on geometry when your stick is at non-zero
        //Vector3 moveTowardsVelocity = new Vector3(Mathf.MoveTowards(m_Rigidbody.velocity.x, desiredVelocity.x, maxSpeedChange * 10f)
        //                                            , Mathf.MoveTowards(m_Rigidbody.velocity.y, -1f, maxSpeedChange * 10f)
        //                                            , (Mathf.MoveTowards(m_Rigidbody.velocity.z, desiredVelocity.z, maxSpeedChange * 10f)));
        //m_Rigidbody.velocity += moveTowardsVelocity;

       
    }

    private void DebugUpdateState()
    {

#if UNITY_EDITOR
        //visualize ground check ray
        //down
        Vector3 groundCheckOffset = Vector3.up * 0.25f;
        Debug.DrawLine(m_Rigidbody.position + groundCheckOffset, m_Rigidbody.position - (groundCheckOffset * m_GroundCheckDistance));
                                         
        //Cardinal Directions            
        Debug.DrawLine(m_Rigidbody.position + groundCheckOffset, m_Rigidbody.position + (groundCheckOffset) + (transform.forward * m_GroundCheckDistance));
        Debug.DrawLine(m_Rigidbody.position + groundCheckOffset, m_Rigidbody.position + (groundCheckOffset) + (-transform.forward * m_GroundCheckDistance));
        Debug.DrawLine(m_Rigidbody.position + groundCheckOffset, m_Rigidbody.position + (groundCheckOffset) + (transform.right * m_GroundCheckDistance));
        Debug.DrawLine(m_Rigidbody.position + groundCheckOffset, m_Rigidbody.position + (groundCheckOffset) + (-transform.right * m_GroundCheckDistance));

        //45* Directions              
        Debug.DrawLine(m_Rigidbody.position + groundCheckOffset, m_Rigidbody.position + (groundCheckOffset) + ((transform.forward - transform.right).normalized * m_GroundCheckDistance));
        Debug.DrawLine(m_Rigidbody.position + groundCheckOffset, m_Rigidbody.position + (groundCheckOffset) + ((transform.forward + transform.right).normalized * m_GroundCheckDistance));
        Debug.DrawLine(m_Rigidbody.position + groundCheckOffset, m_Rigidbody.position + (groundCheckOffset) + ((transform.forward + transform.right).normalized * m_GroundCheckDistance));
        Debug.DrawLine(m_Rigidbody.position + groundCheckOffset, m_Rigidbody.position + (groundCheckOffset) + ((-transform.forward + transform.right).normalized * m_GroundCheckDistance));
        Debug.DrawLine(m_Rigidbody.position + groundCheckOffset, m_Rigidbody.position + (groundCheckOffset) + ((-transform.forward - transform.right).normalized * m_GroundCheckDistance));

        //Surveyor wheel for running debug                                                                         y-coordinate                                    z-coordinate
        //Vector3 verticalOffset = Vector3.Lerp(transform.up * 0.5f, transform.up, m_Rigidbody.velocity.magnitude / maxSpeed);
        //Vector3 forwardOffset = Vector3.Lerp(transform.forward * 0.5f, transform.forward, m_Rigidbody.velocity.magnitude / maxSpeed);
        //Debug.DrawLine(m_Rigidbody.position + verticalOffset, m_Rigidbody.position + transform.up + (verticalOffset * Mathf.Sin(Time.time * m_Rigidbody.velocity.magnitude)) + (forwardOffset * -Mathf.Cos(Time.time * m_Rigidbody.velocity.magnitude))); //up
        //Debug.DrawLine(m_Rigidbody.position + verticalOffset, m_Rigidbody.position + transform.up + (-verticalOffset * Mathf.Sin(Time.time * m_Rigidbody.velocity.magnitude)) + (forwardOffset * Mathf.Cos(Time.time * m_Rigidbody.velocity.magnitude))); //down
        //Debug.DrawLine(m_Rigidbody.position + verticalOffset, m_Rigidbody.position + transform.up + (forwardOffset * Mathf.Sin(Time.time * m_Rigidbody.velocity.magnitude)) + (verticalOffset * Mathf.Cos(Time.time * m_Rigidbody.velocity.magnitude))); //foward
        //Debug.DrawLine(m_Rigidbody.position + verticalOffset, m_Rigidbody.position + transform.up + (forwardOffset * -Mathf.Sin(Time.time * m_Rigidbody.velocity.magnitude)) + (verticalOffset * -Mathf.Cos(Time.time * m_Rigidbody.velocity.magnitude))); //

#endif
    }

    /// <summary>
    /// TODO:
    /// 1. Average the last ground normal with current ground normal to avoid absurd rotation changes. Could also lerp normal from last to current over x time/x translation delta
    /// </summary>
    private void UpdateState()
    {

        //Counting physics steps since last grounded and last jump
        stepsSinceLastGrounded++;
        stepsSinceLastJump++;
        stepsSinceLastDodge++;

        //Stop dodge loop
        if(stepsSinceLastDodge > 40)
        {
            m_Animator.SetBool("canDodge", false);
            m_IsDodging = false;
        }
        RaycastHit hitInfo;
        Vector3 raycastOffset = Vector3.up * 0.25f;
        Vector3 raycastOrigin = m_Rigidbody.position + raycastOffset;

        if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Movement"))
            m_IsDodging = false;

        m_IsNearWall = false; //Set isNearWall to false so it can be set to true if it is true. Otherwise it should default to false

        if (Physics.Raycast(raycastOrigin, transform.forward, out hitInfo, m_GroundCheckDistance, updateStateLayerMask))
        {
            m_IsNearWall = true;
        }
        if (Physics.Raycast(raycastOrigin, -transform.forward, out hitInfo, m_GroundCheckDistance, updateStateLayerMask))
        {
            m_IsNearWall = true;
        }
        if (Physics.Raycast(raycastOrigin, transform.right, out hitInfo, m_GroundCheckDistance, updateStateLayerMask))
        {
            m_IsNearWall = true;
        }
        if (Physics.Raycast(raycastOrigin, -transform.right, out hitInfo, m_GroundCheckDistance, updateStateLayerMask))
        {
            m_IsNearWall = true;
        }
        if (Physics.Raycast(raycastOrigin, (transform.forward + transform.right).normalized, out hitInfo, m_GroundCheckDistance, updateStateLayerMask))
        {
            m_IsNearWall = true;
        }
        if (Physics.Raycast(raycastOrigin, (transform.forward - transform.right).normalized, out hitInfo, m_GroundCheckDistance, updateStateLayerMask))
        {
            m_IsNearWall = true;
        }
        if (Physics.Raycast(raycastOrigin, (-transform.forward + transform.right).normalized, out hitInfo, m_GroundCheckDistance, updateStateLayerMask))
        {
            m_IsNearWall = true;
        }
        if (Physics.Raycast(raycastOrigin, (-transform.forward - transform.right).normalized, out hitInfo, m_GroundCheckDistance, updateStateLayerMask))
        {
            m_IsNearWall = true;
        }

        m_GroundNormal = hitInfo.normal;

        //Scalars on raycast distance are how far player's legs should be able to reach for a double/triple jump off any rigidbody
        if (Physics.Raycast(raycastOrigin, -transform.up, out hitInfo, m_GroundCheckDistance, updateStateLayerMask))
        {
            m_GroundNormal = hitInfo.normal;

            //Only consider grounded when slope is less than max ground angle
            if (m_GroundNormal.y >= Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad))
            {
                m_IsGrounded = true;
                stepsSinceLastGrounded = 0;
            }
            else
            {
                m_IsGrounded = false;
            }
            if (m_Rigidbody.velocity.y < 0 && m_Rigidbody.velocity.y > -0.1f)
                m_IsInAir = false;
            m_JumpCount = 0;
            m_DashCount = 0;
        }
        else
        {
            m_IsGrounded = false;
            m_IsInAir = true;
            m_GroundNormal = Vector3.up;
        }

        if (!m_IsGrounded && m_IsNearWall)
        {
            m_canWallRun = true;
        }
        else
        {
            m_canWallRun = false;
        }
    }

    /* TO DO: 
     * When we get hit while ledgegrabbing we have to handle the physics of that ourselves
     */

    #endregion

    #region INPUT ACTION EVENTS


    public void OnDodge(InputAction.CallbackContext context)
    {
        if (m_IsGrounded)
        {
            m_Animator.SetBool("canDodge", true);
            m_IsDodging = true;
            stepsSinceLastDodge = 0;

            //Move rotation to dodge direction if moving, force movement forward during dodge if not moving
            if (desiredVelocity.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(desiredVelocity, transform.up);
            }
            else
            {
                m_Rigidbody.velocity += transform.forward * dodgeAmount;
            }
            //StartCoroutine(DodgeClean());
        }
    }

    private IEnumerator DodgeClean()
    {
        yield return new WaitForSeconds(0.3f);
        yield return new WaitUntil(() => m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Megaman_DodgeRoll") == false);
        m_Animator.SetBool("canDodge", false);
        m_IsDodging = false;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if ((m_IsGrounded || m_IsNearWall) & m_JumpCount <= m_MaxJumpCount)
            {
                StopCoroutine(Jump());
                StartCoroutine(Jump());
            }
        }
    }

    private IEnumerator Jump()
    {
        stepsSinceLastJump = 0;
        m_JumpCount += 1;
        //If we're ledge grabbing, get up and then proceed
        if (m_IsLedgeGrabbing)
        {
            m_Animator.SetBool("canGetUp", true);
            yield return new WaitUntil(() => m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Megaman_LedgeGetUp") == true);
            yield return new WaitForSecondsRealtime(0.3f);
            GetComponentInParent<Megaman_Rig>().ToggleMove(Vector3.zero, Vector3.zero, "LedgeGrab");
            m_Rigidbody.isKinematic = false;
            m_Rigidbody.useGravity = true;
            //Vector3 transformPosition = transform.position;
            //transform.position = m_Armature.Find("Root").position;
            //m_Armature.Find("Root").position = transformPosition;
            movementType = "grounded";
            m_IsLedgeGrabbing = false;
        }

        //Derivation of desired jumpspeed as a fx of jump height can be found on catlikecoding.com/unity/tutorials/movement/physics
        float jumpSpeed = Mathf.Sqrt(-Physics.gravity.y * m_jumpHeight / 2);

        float alignedSpeed = Vector3.Dot(velocity, m_GroundNormal);

        if (alignedSpeed > 0f)
        {
            //prevents jumps from exceeding max speed
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 1f);
        }

        //If moving slowly, use m_JumpConstant as source of jumpheight
        Vector3 adjustedJumpDirection;

        //When doing grounded jump(incuding walls upto maxGroundAngle), adjust jump towards groundNormal(+Vector3.up upwards jump bias) based on groundNormal slope
        //In the air/on a wall, adjust the jump direction upwards
        if (m_IsGrounded)
        {
            adjustedJumpDirection = Vector3.Lerp(Vector3.up, Vector3.ProjectOnPlane(m_GroundNormal, m_Rigidbody.velocity), 1 - Mathf.Clamp(Vector3.Dot(Vector3.up, m_GroundNormal), 0f, 1f));
        }
        else
        {
            adjustedJumpDirection = Vector3.ProjectOnPlane(transform.up, m_Rigidbody.velocity) + Vector3.up * 0.25f;

        }

        m_Rigidbody.velocity += adjustedJumpDirection * jumpSpeed;

        m_Animator.SetBool("isJumping", true);

        StartCoroutine(PlayJumpSounds());
    }

    private IEnumerator PlayJumpSounds()
    {
        audioSource.PlayOneShot(SFX[1], 0.5f);
        yield return new WaitUntil(() => m_JumpCount == 0);
        audioSource.PlayOneShot(SFX[2], 0.5f);
    }
  

 

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (m_DashCount == 0 && m_Rigidbody.velocity.magnitude <= maxSpeed + 5f)
            {
                //var dashDirection = new Vector3(moveDirection.normalized.x * m_DashConstant,0, moveDirection.normalized.z * m_DashConstant);
                //m_Rigidbody.AddForce(dashDirection, ForceMode.Impulse);
                //StopCoroutine(Dash());
                //StartCoroutine(Dash());
                var dashDirection = new Vector3(desiredVelocity.x, 0f, desiredVelocity.z) * m_DashMultiplier;

                RaycastHit hit;
                Debug.DrawRay(m_Rigidbody.position + Vector3.up, dashDirection.normalized * 1f, Color.grey, 1f);
                if (Physics.Raycast(m_Rigidbody.position + Vector3.up, dashDirection.normalized, out hit, dashDirection.normalized.magnitude * 1f))
                    return;
                m_Rigidbody.velocity += dashDirection;
                m_DashCount += 1;
                audioSource.PlayOneShot(SFX[1], 0.5f);
            }
        }
    }
    public IEnumerator Dash()
    {
        //var dashDirection = new Vector3(transform.forward.x, 0.1f, transform.forward.z) * m_DashConstant;
        var dashDirection = new Vector3(desiredVelocity.normalized.x, velocity.normalized.y + 0.5f, desiredVelocity.normalized.z) * m_DashMultiplier;
        Vector3 newPos = m_Rigidbody.position + dashDirection;
        m_Rigidbody.velocity += dashDirection;
        //while (Vector3.Magnitude(m_Rigidbody.position - newPos) > 2f)
        //{
        //    //transform.Translate(dashDirection * 10 * Time.deltaTime, Space.World);
        //    RaycastHit hit;
        //    Debug.DrawRay(m_Rigidbody.position + Vector3.up , dashDirection.normalized * 1f, Color.grey, 1f);
        //    if (Physics.Raycast(m_Rigidbody.position + Vector3.up , dashDirection.normalized, out hit, dashDirection.normalized.magnitude * 1f)){
        //        break;
        //    }
        //    m_Rigidbody.MovePosition(m_Rigidbody.position + 0.3f * dashDirection);
        //    yield return null;
        //}
        yield return null;
    }
    public void OnMovement()
    {
        //Control loop speed for run as a function of movedirection.magnitude
        if ((!m_IsGrounded && m_IsInAir && m_Rigidbody.velocity.y < -0.1f) && !m_IsWallRunning)
        {
            m_Animator.SetBool("isJumping", false);
            m_Animator.SetBool("isFalling", true);
        }

        if (m_IsGrounded && Mathf.Abs(m_Rigidbody.velocity.y) < 0.1f)
        {
            m_Animator.SetBool("isFalling", false);
            m_Animator.SetBool("isJumping", false);
        }
        desiredVelocity = new Vector3(Gamepad.current.leftStick.ReadValue().x, 0f, Gamepad.current.leftStick.ReadValue().y) * maxSpeed;

        //Add in camera orientation
        Vector3 m_CamForward_test = Vector3.Scale(m_CamForward, new Vector3(1, 0, 1)).normalized;
        desiredVelocity = desiredVelocity.x * m_CamRight + desiredVelocity.z * m_CamForward_test;



        velocity = m_Rigidbody.velocity;

        AdjustVelocity(); //Changes air acceleration based on m_IsGrounded and adjusts velocity based on ground slope

        SnapToGround(); //returns a bool but really snaps player to ground to prevent sliding ridiculously off the edge of ramps

        //To make the fall part of the jump faster, affect gravity
        //Including points just before the 0-derivative point helps w precision issues
        if (m_Rigidbody.velocity.y < 0.1f)
        {
            velocity += Vector3.up * Physics.gravity.y * FallRateModifier * Time.deltaTime;
        }

        //Do movement and rotation
        m_Rigidbody.velocity = velocity;
        m_Animator.SetFloat("moveSpeed", velocity.magnitude/maxSpeed);

        DoRotation();

        //If player is not grounded, is near a wall, and is pressing wallrun button, let them wall run
        if (m_canWallRun && m_IsWallRunning)
        {
            m_Animator.SetBool("isFalling", false);
            m_Animator.SetBool("isJumping", false);
          
            if (isVerticalWallRun)
            {
                transform.forward = transform.up;
                //transform.up = -transform.forward;
                velocity.y = 0;
            }
            if (m_IsGrounded || m_IsNearWall)
            {
                velocity.y = wallRunForce;
            }
            Vector3 wallRunDirection = new Vector3(desiredVelocity.x * m_WallRunMultiplier, velocity.normalized.y, desiredVelocity.z * m_WallRunMultiplier);
            m_Animator.SetFloat("moveSpeed", velocity.magnitude * 0.05f);
            m_Rigidbody.velocity = wallRunDirection;
            //m_Rigidbody.MovePosition(transform.position + wallRunDirection * Time.deltaTime);
        }


    }

    /// <summary>
    /// TODO:
    /// 1. Once you understand quaternions properly, fix the m_RigidBody.MoveRotation() line
    /// 1a. The reason shit gets fucked is bc Quaternion.LookRotation * tiltTarget evaluates to Quaternion.identity when
    /// 1b. lookDir is too far away from transform.forward. Too far being maybe >20 degrees
    /// </summary>
    private void DoRotation()
    {
        //convert to local-relative for tiltTarget
        Vector3 moveDirectionLocal = transform.InverseTransformDirection(desiredVelocity);
        Vector3 lookDir;
        float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, Vector3.Magnitude(desiredVelocity));

        //Let player move backwards if moveDirection is pointed backwards
        if (Vector3.Dot(transform.forward, desiredVelocity.normalized) > -0.85f)
        {
            lookDir = Vector3.RotateTowards(transform.forward, desiredVelocity.normalized, turnSpeed * Time.deltaTime, 0.0f);
        }
        else
        {
            lookDir = transform.forward;
        }
        //For regular movemnt
        if (m_Rigidbody.velocity.magnitude > 0.2f)
        {
            tiltDir = new Vector3(8f * moveDirectionLocal.normalized.z, 0f, 8f * -moveDirectionLocal.normalized.x);
            tiltDir.x = Mathf.Clamp(tiltDir.x, 0f, 10f);
            tiltDir.z = Mathf.Clamp(tiltDir.z, -10f, 10f);
            tiltDir.y = 0f;
            tiltTarget = Quaternion.Euler(tiltDir);
        }
        //When we've fallen
        else if (m_Rigidbody.velocity.magnitude <= 0.2f && Vector3.Dot(Vector3.up, transform.up) < 0.966f)
        {
            tiltTarget = Quaternion.Euler(-transform.rotation.x * 20f, 0f, -transform.rotation.z * 20f);
        }
        //When we're still, don't add any rotation to the lookdir, which will be close to the current transform.forward if the code enters here
        else
        {
            tiltTarget = Quaternion.Euler(0f, 0f, 0f);
        }


        Debug.DrawRay(m_Rigidbody.position + Vector3.up, desiredVelocity / maxSpeed, Color.green);
        Debug.DrawRay(m_Rigidbody.position + (Vector3.up * 0.9f), lookDir.normalized, Color.blue);
        Vector3 lookDirAdusted = Quaternion.LookRotation(lookDir, transform.up).eulerAngles + tiltDir;

        //m_Rigidbody.MoveRotation(Quaternion.LookRotation(lookDir, transform.up) * tiltTarget);
        m_Rigidbody.MoveRotation(Quaternion.LookRotation(lookDir, transform.up));
    }

    /// <summary>
    /// ToDo:
    /// 1. Resolve issue b/w snapDistance and jump steps. Currently snaps too fast or not at all based on snap distance *SPECIFICALLY* when jumping! Will let jump for now and leave snapping for later
    /// </summary>
    /// <returns></returns>
    //Snaps player to ground to prevent being launched off the top of ramps
    private bool SnapToGround()
    {
        //Don't snap to ground if we've been not-grounded for 2 or more steps  OR if we've jumped within the last 2 steps
        if (stepsSinceLastGrounded >= 2 || stepsSinceLastJump <= 2)
        {
            return false;
        }
        if (m_JumpCount > 0)
        {
            return false;
        }
        float speed = m_Rigidbody.velocity.magnitude;
        if (speed > maxGroundSnapSpeed)
        {
            return false;
        }
        Debug.DrawLine(m_Rigidbody.position + Vector3.up, m_Rigidbody.position - (Vector3.up * snapDistance), Color.red);
        if (!Physics.Raycast(m_Rigidbody.position + Vector3.up, -transform.up, out RaycastHit hit, snapDistance, snapMask))
        {
            return false;
        }
        if (hit.normal.y < Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad))
        {
            return false;
        }

        if (hit.normal != null)
        {
            m_GroundNormal = hit.normal;
        }
        float dot = Vector3.Dot(m_Rigidbody.velocity, hit.normal);
        if (dot > 0.1f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }

        return true;
    }

    private Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        //Remove some of the movement in the groundNormal direction by taking the projection of velocity onto ground normal. This is so we don't lose contact too much at higher slope values
        //Why aren't we using Vector3.ProjectOnPlane() here? 
        return vector - m_GroundNormal * Vector3.Dot(vector, m_GroundNormal);
    }

    /// <summary>
    /// TODO:
    /// 1. xAxis and zAxis equations might not work at different rotations (if gravity = (-9.81,0,0) for example). Must make more robust 
    /// when the time comes
    /// 2. If you make the ProjectOnContactPlane transform.right/forward, rotation gets fucked up since rotation is in local axes (i think)
    /// </summary>
    //Uses ProjectOnContactPlane to adjust velocity based on ground angle
    private void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right);
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward);

        float currentXVelocity = Vector3.Dot(velocity, xAxis);
        float currentZVelocity = Vector3.Dot(velocity, zAxis);

        float acceleration = (m_IsGrounded) ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentXVelocity, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZVelocity, desiredVelocity.z, maxSpeedChange);

        velocity += xAxis * (newX - currentXVelocity) + zAxis * (newZ - currentZVelocity);
    }

    #endregion
}

public class LockOnEventArgs : EventArgs
{
    public bool lockedOn { get; set; }
    public Transform lockedEnemy { get; set; }
}