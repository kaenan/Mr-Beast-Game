using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : NetworkBehaviour
{
    public bool dead { get; set; }

    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject lightObj;
    [SerializeField] private GameObject lightHolder;

    [Header("Movement")]
    [SerializeField] int speed;
    [SerializeField] int sprintSpeed;
    [SerializeField] int crouchSpeed;
    [SerializeField] int jumpForce;
    [SerializeField] int climbSpeed;
    NetworkVariable<int> currentSpeed = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    NetworkVariable<float> velocity = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private Vector3 limitedVelocity;
    private Vector3 horizontalVelocity;

    [Header("Boot Prints")]
    [SerializeField] private GameObject[] bootPrintObj;
    [SerializeField] private GameObject[] feetObjs;
    private bool leftPrint = false;
    private float printTimer = 0;
    private float printTime = 0.7f;
    private int boots = -1;
    private Dictionary<int, GameObject> bootPrints;

    // Crouching
    private float colliderOrgHeight;
    private float colliderOrgY;
    private float colliderCrouchHeight;
    private float colliderCrouchY;
    private bool canStand = true;

    [Header("Stamina Settings")]
    [SerializeField] private float runTime;
    [SerializeField] private float staminaCooldown;
    private float runTimer;
    private float staminaCooldownTimer;

    [Header("Ground Check")]
    [SerializeField] LayerMask ground;
    [SerializeField] float groundDrag;
    private bool grounded;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private ClientNetworkAnimator c_animator;
    private bool grappledAnimation = false;
    private bool deathAnimation = false;

    private Transform cam;
    private Vector3 camForward;
    private Vector3 camRight;
    private Vector3 forward;

    [Header("Ladder Climbing")]
    [SerializeField] private CapsuleCollider playerCollider;
    [SerializeField] private PhysicMaterial normalMat;
    [SerializeField] private PhysicMaterial climbingMat;
    private bool touchingLadder = false;
    private Vector3 ladderPos;

    //HUD References
    private PauseMenu pauseMenu;
    private HUDReferences HUDReferences;
    private Slider staminaBar;
    private Image staminaFill;

    [Header("Effects")]
    [SerializeField] private int slowDownSpeed;
    NetworkVariable<bool> slowDown = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    NetworkVariable<bool> grappled = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Audio")]
    [SerializeField] private AudioSource audio_source;
    [SerializeField] private AudioClip walkingFX;
    [SerializeField] private AudioClip runningFX;
    private GameVolume gameVolume;

    public override void OnNetworkSpawn()
    {
        bootPrints = new Dictionary<int, GameObject>();
        gameVolume = FindObjectOfType<GameVolume>();

        if (!IsOwner) return;
        dead = false;

        pauseMenu = FindObjectOfType<PauseMenu>();

        colliderOrgHeight = playerCollider.height;
        colliderOrgY = playerCollider.center.y;
        colliderCrouchHeight = colliderOrgHeight / 2;
        colliderCrouchY = colliderOrgY / 2;

        cam = Camera.main.transform;

        HUDReferences = GameObject.FindGameObjectWithTag("PlayerHUD").GetComponent<HUDReferences>();
        staminaBar = HUDReferences.GetStaminaSlider();
        staminaFill = HUDReferences.GetStaminaFill();
        staminaFill.color = HUDReferences.GetStaminaActive();
        staminaBar.minValue= 0;
        staminaBar.maxValue = runTime;
        staminaBar.value = runTime;
        staminaCooldownTimer = 0;

        SetCurrentSpeedServerRpc(speed);

        Instantiate(lightObj, lightHolder.transform.position, Quaternion.identity).transform.SetParent(lightHolder.transform);
    }

    void Update()
    {
        SoundEffects();
        UpdateVolume();

        if (!IsOwner) return;

        if (dead)
        {
            if (!deathAnimation)
            {
                c_animator.SetTrigger("Dead");
                animator.SetBool("Is Dead", true);
            }
            deathAnimation= true;
            return;
        } 
        else if (deathAnimation)
        {
            animator.SetBool("Is Dead", false);
            deathAnimation = false;
        }

        if (grappled.Value)
        {
            if (!grappledAnimation)
            {
                c_animator.SetTrigger("Grappled");
                animator.SetBool("Is Grappled", true);
                grappledAnimation = true;
            }
        }
        else
        {
            if (grappledAnimation)
            {
                animator.SetBool("Is Grappled", false);
                grappledAnimation = false;
            }
        }

        GroundCheck();
        LadderCheck();
        CrouchCheck();
        SpeedControl();

        if (pauseMenu.IsPaused()) return;

        Movement();
        Jump();

        if (Input.GetKeyDown(KeyCode.V))
        {
            c_animator.SetTrigger("Emote");
        }
    }

    private void Movement()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) 
        {
            camForward = new(cam.forward.x, 0f, cam.forward.z);

            if (touchingLadder)
            {
                forward = transform.up * Input.GetAxis("Vertical") * 3f + camForward * Input.GetAxis("Vertical");
                rb.AddForce(forward * climbSpeed * Time.deltaTime * 8 * rb.mass, ForceMode.Impulse);
            }
            else if (grounded)
            {
                camRight = new(cam.right.x, 0f, cam.right.z);

                forward = camForward * Input.GetAxis("Vertical") + camRight * Input.GetAxis("Horizontal");
                rb.AddForce(forward * currentSpeed.Value * Time.deltaTime * 100 * rb.mass, ForceMode.Force);

                rb.rotation = Quaternion.Lerp(rb.rotation, Quaternion.LookRotation(Vector3.Normalize(forward)), Time.deltaTime * 5);
            }
            BootPrints();
        }

        SprintAndCrouch();
    }

    private void SprintAndCrouch()
    {
        if (!slowDown.Value && !grappled.Value)
        {
            if (touchingLadder) return;

            if (Input.GetKey(KeyCode.LeftShift) && runTimer > 0 && staminaCooldownTimer <= 0)
            {
                if (currentSpeed.Value != sprintSpeed) SetCurrentSpeedServerRpc(sprintSpeed);
                staminaBar.value -= Time.deltaTime;
                runTimer = staminaBar.value;
            }
            else if (Input.GetKey(KeyCode.C))
            {
                if (currentSpeed.Value != crouchSpeed) SetCurrentSpeedServerRpc(crouchSpeed);

                if (playerCollider.height != colliderCrouchHeight)
                {
                    playerCollider.height = colliderCrouchHeight;
                    playerCollider.center = new(playerCollider.center.x, colliderCrouchY, playerCollider.center.z);
                }
            }
            else
            {
                if (currentSpeed.Value != speed) SetCurrentSpeedServerRpc(speed);
            }

            if (Input.GetKeyUp(KeyCode.LeftShift) || runTimer <= 0)
            {
                staminaCooldownTimer = staminaCooldown;
            }
        }
        else if (grappled.Value)
        {
            if (currentSpeed.Value != 0) SetCurrentSpeedServerRpc(0);
        }
        else if (slowDown.Value)
        {
            if (currentSpeed.Value != slowDownSpeed) SetCurrentSpeedServerRpc(slowDownSpeed);
        } 

        if (staminaCooldownTimer > 0)
        {
            if (staminaFill.color != HUDReferences.GetStaminaInactive()) staminaFill.color = HUDReferences.GetStaminaInactive();
            staminaCooldownTimer -= Time.deltaTime;
        }
        else
        {
            if (staminaFill.color != HUDReferences.GetStaminaActive()) staminaFill.color = HUDReferences.GetStaminaActive();
        }

        if (!Input.GetKey(KeyCode.LeftShift) || staminaCooldownTimer > 0)
        {
            staminaBar.value += Time.deltaTime;
            runTimer = staminaBar.value;
        }

        if (currentSpeed.Value != crouchSpeed && playerCollider.height != colliderOrgHeight && canStand)
        {
            playerCollider.height = colliderOrgHeight;
            playerCollider.center = new(playerCollider.center.x, colliderOrgY, playerCollider.center.z);
        }
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (touchingLadder)
            {
                rb.AddForce(-transform.forward * jumpForce * rb.mass, ForceMode.Impulse);
                touchingLadder = false;
            }
            else if (grounded)
            {
                rb.AddForce(Vector3.up * jumpForce * rb.mass, ForceMode.Impulse);
            }
        }
    }

    private void GroundCheck()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, 0.3f);

        if(grounded)
        {
            rb.drag = groundDrag;
        } else
        {
            rb.drag = 0;
        }
    }

    private void SpeedControl()
    {
        horizontalVelocity = new(rb.velocity.x, 0 , rb.velocity.z);

        if (touchingLadder)
        {
            if (rb.velocity.magnitude > climbSpeed)
            {
                limitedVelocity = rb.velocity.normalized * climbSpeed;
                rb.velocity = new(limitedVelocity.x, limitedVelocity.y, limitedVelocity.z);
            }
        }
        else
        {
            if (rb.velocity.magnitude > currentSpeed.Value)
            {
                limitedVelocity = horizontalVelocity.normalized * currentSpeed.Value;
                rb.velocity = new(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
            }
        }

        velocity.Value = rb.velocity.magnitude;
        animator.SetFloat("Speed", rb.velocity.magnitude);
        animator.SetInteger("Set Speed", currentSpeed.Value);
    }

    private void LadderCheck()
    {
        ladderPos = new(transform.position.x, transform.position.y + 0.2f, transform.position.z);
        if (Physics.Raycast(ladderPos, transform.forward, out RaycastHit hit, 0.5f))
        {
            if (hit.transform.CompareTag("Ladder"))
            {
                touchingLadder = true;
                rb.useGravity = false;
                playerCollider.material = climbingMat;
            } 
            else
            {
                touchingLadder = false;
                rb.useGravity = true;
                playerCollider.material = normalMat;
            }
        } else
        {
            touchingLadder = false;
            rb.useGravity = true;
            playerCollider.material = normalMat;
        }
    }

    private void CrouchCheck()
    {
        canStand = !Physics.Raycast(transform.position, Vector3.up, colliderOrgHeight + 0.2f);
    }

    /// <summary>
    /// 
    /// \/ \/ \/ \/ Slow Effect Stuff \/ \/ \/ \/
    /// 
    /// </summary>
    /// 

    public void SlowDownEffect(float effectLength)
    {
        SetSlowDownValueServerRpc(true);
        StartCoroutine(SlowDownEffectTimer(effectLength));
    }

    public void SlowDownEffectNoTimer(bool active)
    {
        SetSlowDownValueServerRpc(active);
    }

    IEnumerator SlowDownEffectTimer(float length)
    {
        yield return new WaitForSeconds(length);

        SetSlowDownValueServerRpc(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetSlowDownValueServerRpc(bool value)
    {
        slowDown.Value = value;
    }

    /// <summary>
    /// 
    /// /\ /\ /\ /\ Slow Down Stuff /\ /\ /\ /\
    /// 
    /// </summary>

    [ServerRpc(RequireOwnership = false)]
    private void SetCurrentSpeedServerRpc(int newSpeed)
    {
        currentSpeed.Value = newSpeed;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetGrappledServerRpc(bool isGrappled)
    {
        grappled.Value = isGrappled;
    }

    /// <summary>
    /// 
    /// \/ \/ \/ \/ Boot Prints \/ \/ \/ \/
    /// 
    /// </summary>

    private void BootPrints()
    {
        if (bootPrintObj.Length != 2) return;
        if (!grounded) return;

        if (leftPrint && printTimer <= 0)
        {
            printTimer = printTime;
            leftPrint = !leftPrint;
            boots++;
            SpawnBootPrintServerRpc(0);
        }
        else if (!leftPrint && printTimer <= 0)
        {
            printTimer = printTime;
            leftPrint = !leftPrint;
            SpawnBootPrintServerRpc(1);
        }

        printTimer -= Time.deltaTime;
    }

    private void SpawnBootPrints(int foot)
    {
        GameObject b = Instantiate(bootPrintObj[foot], feetObjs[foot].transform.position, Quaternion.LookRotation(transform.forward));
        b.GetComponent<NetworkObject>().Spawn();

        boots++;
        bootPrints.Add(boots, b);
        StartCoroutine(DelayDespawnBootPrint(boots));
    }

    IEnumerator DelayDespawnBootPrint(int key)
    {
        yield return new WaitForSeconds(10);

        DespawnBootPrintServerRpc(key);
    }

    private void DespawnBootPrints(int key)
    {
        bootPrints[key].GetComponent<NetworkObject>().Despawn();
        bootPrints.Remove(key);
    }

    [ServerRpc (RequireOwnership = false)]
    private void SpawnBootPrintServerRpc(int foot)
    {
        SpawnBootPrints(foot);
    }

    [ServerRpc (RequireOwnership = false)]
    private void DespawnBootPrintServerRpc(int key)
    {
        DespawnBootPrints(key);
    }

    /// <summary>
    /// 
    /// /\ /\ /\ /\ Boot Prints /\ /\ /\ /\
    /// 
    /// </summary>

    [ClientRpc]
    public void MovePlayerClientRpc(Vector3 position)
    {
        rb.position = position;
    }

    private void SoundEffects()
    {
        if (velocity.Value < 1)
        {
            if (audio_source.isPlaying) audio_source.Stop();
        }
        else
        {
            if (currentSpeed.Value <= 12)
            {
                audio_source.clip = walkingFX;
                if (!audio_source.isPlaying) audio_source.Play();
            }
            else
            {
                audio_source.clip = runningFX;
                if (!audio_source.isPlaying) audio_source.Play();
            }
        }
    }

    private void UpdateVolume()
    {
        if (audio_source.volume != gameVolume.sfxVolume) audio_source.volume = gameVolume.sfxVolume;
    }
}
