using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MrBeastPlayer : NetworkBehaviour
{
    [SerializeField] private PlayerMovement movementScript;

    [Header ("Mr Beast Player Model Stuff")]
    [SerializeField] private GameObject torch;
    [SerializeField] private GameObject rightHand;
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject torchLookAt;
    private GameObject torchGO;
    private TorchWeapon switchOn;

    [Header("Grapple")]
    [SerializeField] private GameObject grappleObj;
    [SerializeField] private LineRenderer grappleLine;
    [SerializeField] private float grappleTime;
    [SerializeField] private float grappleCooldownTime;
    [SerializeField] private int quality;
    [SerializeField] private float waveHeight;
    [SerializeField] private float waveCount;
    [SerializeField] private float damper;
    [SerializeField] private float strength;
    [SerializeField] private float velocity;
    [SerializeField] private AnimationCurve affectCurve;
    private float grappleTimer;
    private float grappleCooldownTimer = 0;
    private Vector3 startPoint;
    private PlayerEffects effects;
    private Spring spring;

    [Header("Sloth Ability")]
    [SerializeField] private Transform slothSpawnPos;
    [SerializeField] private GameObject slothObj;
    [SerializeField] private float timeBeforeSloth;
    private float slothTimer;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioSource audio_source;

    private Transform cam;
    private HUDReferences HUDReferences;
    private Slider grappleSlider;
    private PauseMenu pauseMenu;
    private GameVolume gameVolume;

    NetworkVariable<bool> grappling = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    NetworkVariable<GrappleLine> lineInfo = new NetworkVariable<GrappleLine>(new GrappleLine
    {
        hitPoint = Vector3.zero
    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct GrappleLine : INetworkSerializable
    {
        public Vector3 hitPoint;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref hitPoint);
        }
    }

    public override void OnNetworkSpawn()
    {
        animator.SetLayerWeight(1, 0);
        spring = new Spring();
        spring.SetTarget(0);

        gameVolume = FindObjectOfType<GameVolume>();

        if (!IsOwner) return;

        pauseMenu = FindObjectOfType<PauseMenu>();

        SpawnTorchServerRpc();
        cam = Camera.main.transform;

        HUDReferences = FindObjectOfType<HUDReferences>();
        grappleSlider = HUDReferences.GetGrappleSlider();
        grappleSlider.minValue = 0;
        grappleSlider.maxValue = grappleTime;
        grappleSlider.value = grappleTime;
        grappleSlider.gameObject.SetActive(true);
        grappleTimer = grappleTime;

        HUDReferences.GetKarlIcon().SetActive(true);
        HUDReferences.SetKarlNotReady();
        HUDReferences.InitKarlLoadingSlider(0, timeBeforeSloth, slothTimer);

        slothTimer = timeBeforeSloth;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnTorchServerRpc()
    {
        SpawnTorch();
    }

    private void SpawnTorch()
    {
        torchGO = Instantiate(torch, rightHand.transform.position, Quaternion.identity);
        torchGO.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
    }

    private void Update()
    {
        UpdateVolume();

        if (grappling.Value)
        {
            if (grappleLine.positionCount== 0)
            {
                spring.SetVelocity(velocity);
                grappleLine.positionCount = quality + 1;
                audio_source.Play();
            }
            animator.SetLayerWeight(1, 1);
            DrawGrappleLine();
            grappleObj.SetActive(false);
        } else
        {
            spring.Reset();
            startPoint = leftHand.transform.position;
            animator.SetLayerWeight(1, 0);
            grappleLine.positionCount = 0;
            grappleObj.SetActive(true);
        }

        if (!IsOwner) return;
        if (pauseMenu.IsPaused()) return;
        MoveTorch();
        StartGrapple();
        SpawnSloth();

        if (switchOn == null)
        {
            switchOn = FindObjectOfType<TorchWeapon>();
        }
        else
        {
            movementScript.SlowDownEffectNoTimer(switchOn.IsSwitchOn());
        }
    }

    private void MoveTorch()
    {
        if (torchGO == null)
        {
            if (FindObjectOfType<TorchWeapon>())
            {
                torchGO = FindObjectOfType<TorchWeapon>().gameObject;
                return;
            }
            return;
        }

        torchGO.transform.position = rightHand.transform.position;
        torchGO.transform.rotation = Quaternion.LookRotation(Vector3.Normalize(torchLookAt.transform.position - torchGO.transform.position));
    }

    private void StartGrapple()
    {
        if (Input.GetMouseButtonDown(0) && grappleCooldownTimer >= grappleCooldownTime)
        {
            if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, 30, 1 | 3 | 1))
            {
                grappling.Value = true;
                animator.Play("Grapple", 1, 0);

                lineInfo.Value = new GrappleLine
                {
                    hitPoint = hit.point,
                };

                if (hit.transform.CompareTag("Person"))
                {
                    effects = hit.transform.GetComponentInChildren<PlayerEffects>();
                    
                    effects.Grappled(true);
                    
                }
            }
        }

        if (Input.GetMouseButton(0) && grappleCooldownTimer >= grappleCooldownTime && grappling.Value)
        {
            grappleTimer -= Time.deltaTime;
            grappleTimer = Mathf.Clamp(grappleTimer -= Time.deltaTime, 0, grappleTime);
            grappleSlider.value = grappleTimer;
        }

        if ((Input.GetMouseButtonUp(0) || grappleTimer <= 0) && grappleCooldownTimer >= grappleCooldownTime)
        {
            StopGrapple();
        }

        if (grappleCooldownTimer <= grappleCooldownTime)
        {
            grappleCooldownTimer += Time.deltaTime;
            grappleSlider.value = grappleCooldownTimer;
        }
    }

    private void StopGrapple()
    {
        grappling.Value = false;
        grappleCooldownTimer = 0;
        grappleTimer = grappleTime;
        if (effects != null) effects.Grappled(false);
        effects = null;
    }

    private void DrawGrappleLine()
    {
        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        Vector3 up = Quaternion.LookRotation((leftHand.transform.position - lineInfo.Value.hitPoint).normalized) * Vector3.up;
        startPoint = Vector3.Lerp(startPoint, lineInfo.Value.hitPoint, Time.deltaTime * 12f);

        for (int i = 0; i < quality + 1; i++)
        {
            float delta = i / (float) quality;
            Vector3 offset = affectCurve.Evaluate(delta) * Mathf.Sin(delta * waveCount * Mathf.PI) * waveHeight * up * spring.Value;

            grappleLine.SetPosition(i, Vector3.Lerp(leftHand.transform.position, startPoint, delta) + offset);
        }
    }

    private void SpawnSloth()
    {
        if (slothTimer <= 0)
        {
            HUDReferences.SetKarlReady();
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SpawnSlothServerRpc();
                slothTimer = timeBeforeSloth;
                HUDReferences.SetKarlNotReady();
            }
        }
        else
        {
            slothTimer -= Time.deltaTime;
            HUDReferences.SetKarlSlider(slothTimer);
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void SpawnSlothServerRpc()
    {
        GameObject sloth = Instantiate(slothObj, slothSpawnPos.position, Quaternion.identity);
        sloth.GetComponent<NetworkObject>().Spawn();
    }

    private void UpdateVolume()
    {
        if (audio_source.volume != gameVolume.sfxVolume) audio_source.volume = gameVolume.sfxVolume;
    }
}
