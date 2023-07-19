using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class Health : NetworkBehaviour
{
    [SerializeField] private PlayerMovement movementScript;
    [SerializeField] private bool mrBeast;

    //HUD References
    private HUDReferences HUDReferences;
    private Slider healthBar;
    private Image flashOverlay;
    private Color flashOverlayColor;

    //Post Processing
    private PostProcessingReferences PostProcessingReferences;
    private PostProcessVolume blurVolume;

    private bool hit = false;
    private Vector3 torchPosition;
    private float distance;

    private GameVariables gameVars;
    private bool dead = false;

    //Network Variables
    NetworkVariable<float> health = new NetworkVariable<float>(30, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    NetworkVariable<bool> takingDamage = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    NetworkVariable<bool> torchDamage = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        HUDReferences= GameObject.FindGameObjectWithTag("PlayerHUD").GetComponent<HUDReferences>();
        PostProcessingReferences= GameObject.FindGameObjectWithTag("Post Process").GetComponent<PostProcessingReferences>();

        gameVars = FindObjectOfType<GameVariables>();

        HUDReferences.transform.GetChild(0).gameObject.SetActive(true);

        blurVolume = PostProcessingReferences.GetBlurVolume();

        healthBar = HUDReferences.GetHealthSlider();
        healthBar.maxValue = health.Value;
        healthBar.value = health.Value;

        flashOverlay = HUDReferences.GetFlashOverlay();
        flashOverlayColor = flashOverlay.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        healthBar.value = health.Value;

        TorchBlind();
        Blinded();

        if (mrBeast)
        {
            if (health.Value <= 0 && !dead)
            {
                dead = true;
                movementScript.dead = true;
                gameVars.SetMrBeastDeadServerRpc();
            }
        }
        else
        {
            if (health.Value <= 0 && !dead)
            {
                dead = true;
                movementScript.dead = true;
                gameVars.AddPlayerDeadServerRpc();
            }
            else if (health.Value > 0 && dead)
            {
                dead = false;
                movementScript.dead = false;
                gameVars.RemovePlayerDeadServerRpc();
            }
        }
    }

    private void TorchBlind()
    {
        if (takingDamage.Value && torchDamage.Value)
        {
            distance = Vector3.Distance(transform.position, torchPosition);
            float i = Mathf.Clamp(1 / (distance - 2), 0, 100);

            flashOverlayColor.a = Mathf.Lerp(flashOverlayColor.a, i, Time.deltaTime * 10);
            blurVolume.weight = flashOverlayColor.a;

            flashOverlay.color = flashOverlayColor;
        }
        else if (!takingDamage.Value && !torchDamage.Value)
        {
            flashOverlayColor.a = Mathf.Lerp(flashOverlayColor.a, 0, Time.deltaTime * 5);
            blurVolume.weight = Mathf.Lerp(flashOverlayColor.a, 0, Time.deltaTime);

            if (blurVolume.weight <= 0.1 && blurVolume.weight != 0) blurVolume.weight = 0;

            flashOverlay.color = flashOverlayColor;
        }
    }

    private void Blinded()
    {
        if (takingDamage.Value && !torchDamage.Value)
        {
            if (!hit)
            {
                flashOverlayColor.a = 1;
                blurVolume.weight = 1;
                flashOverlay.color = flashOverlayColor;
                hit = true;
            }

            flashOverlayColor.a = Mathf.Lerp(flashOverlayColor.a, 0, Time.deltaTime * 5);
            blurVolume.weight = Mathf.Lerp(flashOverlayColor.a, 0, Time.deltaTime);

            if (blurVolume.weight <= 0.1 && blurVolume.weight != 0) blurVolume.weight = 0;

            flashOverlay.color = flashOverlayColor;

            if (blurVolume.weight == 0)
            {
                TakingDamageServerRpc(false, false);
                hit = false;
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void TakeHealthServerRpc(float take)
    {
        health.Value -= take;
    }

    [ServerRpc (RequireOwnership = false)]
    public void AddHealthServerRpc(float add)
    {
        health.Value = Mathf.Clamp(health.Value + add, 0, 30);
    }

    public float GetHealth()
    {
        return health.Value;
    }

    public void TakingDamage(bool hitting, Vector3 tp)
    {
        TakingDamageServerRpc(hitting, hitting);
        torchPosition = tp;
    }
    public void HitByObject(float damage)
    {
        TakeHealthServerRpc(damage);
        TakingDamageServerRpc(true, false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakingDamageServerRpc(bool takingDamage2, bool torchDamage2)
    {
        takingDamage.Value = takingDamage2;
        torchDamage.Value = torchDamage2;
    }
}
