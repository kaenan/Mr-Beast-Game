using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TorchWeapon : NetworkBehaviour
{
    [Header("Torch Settings")]
    [SerializeField] GameObject lightObj;
    [SerializeField] float torchDamage;
    [SerializeField] private float coolDownTime;
    [SerializeField] private float torchUseTime;
    [SerializeField] private float timeBetweenUse;
    private float torchUseTimer;
    private float betweenUseTimer = 0f;

    private PauseMenu pauseMenu;
    private HUDReferences HUDReferences;
    private Slider torchSlider;

    // Animator
    private Animator animator;

    NetworkVariable<bool> switchedOn = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    NetworkVariable<int> animationWeight = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        pauseMenu = FindObjectOfType<PauseMenu>();

        HUDReferences= GameObject.FindGameObjectWithTag("PlayerHUD").GetComponent<HUDReferences>();
        torchSlider= HUDReferences.GetLightSlider();
        torchSlider.maxValue = torchUseTime;
        torchSlider.minValue = 0f;
        torchSlider.value = torchUseTime;
        torchSlider.gameObject.SetActive(true);
        torchUseTimer = torchUseTime;
    }

    void Update()
    {
        if (lightObj.activeSelf != switchedOn.Value)
        {
            lightObj.SetActive(switchedOn.Value);
        }

        if (animator != null)
        {
            if (animator.GetLayerWeight(2) != animationWeight.Value)
            {
                animator.SetLayerWeight(2, animationWeight.Value);
            }
        } 
        else
        {
            animator = FindObjectOfType<MrBeastPlayer>().gameObject.GetComponent<Animator>();
        }

        if (!IsOwner) return;
        if (pauseMenu.IsPaused()) return;

        TorchControls();
    }

    private void TorchControls()
    {
        if (Input.GetMouseButtonDown(1))
        {
            animationWeight.Value = 1;
        }

        if (Input.GetMouseButtonUp(1)) 
        {
            switchedOn.Value = false;

            if (betweenUseTimer <= 0)
            {
                betweenUseTimer = timeBetweenUse;
            }
            animationWeight.Value = 0;
        }

        if (Input.GetMouseButton(1))
        {
            if (torchUseTimer > 0f && betweenUseTimer <= 0)
            {
                if (!switchedOn.Value) TorchOn();

                torchSlider.value -= Time.deltaTime;
                torchUseTimer = torchSlider.value;
            } 
            else
            {
                if (switchedOn.Value) TorchOff();

                if (betweenUseTimer <= 0) betweenUseTimer = torchUseTime / 2;
                ResetTorchSlider();
            }
        } 
        else
        {
            ResetTorchSlider();
        }

        if (betweenUseTimer >= 0f)
        {
            betweenUseTimer -= Time.deltaTime;
        }
    }

    private void TorchOn()
    {
        switchedOn.Value = true;
    }

    private void TorchOff()
    {
        switchedOn.Value = false;
    }

    public bool IsSwitchOn()
    {
        return switchedOn.Value;
    }

    private void ResetTorchSlider()
    {
        if (torchUseTimer < torchUseTime)
        {
            torchSlider.value += Time.deltaTime / coolDownTime;
            torchUseTimer = torchSlider.value;
        }
        else
        {
            torchSlider.value = torchUseTime;
            torchUseTimer = torchUseTime;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Person")
        {
            if (switchedOn.Value)
            {
                Health otherHealth = other.GetComponent<Health>();
                otherHealth.TakingDamage(true, transform.position);

                if (IsOwner)
                {
                    otherHealth.TakeHealthServerRpc(torchDamage);
                }
            }
            else
            {
                other.GetComponent<Health>().TakingDamage(false, Vector3.zero);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Person")
        {
            other.GetComponent<Health>().TakingDamage(false, Vector3.zero);
        }
    }
}
