using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPickUp : NetworkBehaviour
{
    private HUDReferences HUDReferences;
    private PauseMenu pauseMenu;

    [SerializeField] private PlayerMovement movement;

    [Header("Hold Items")]
    [SerializeField] private GameObject hand;
    [SerializeField] private LayerMask layer;

    [Header("Healing")]
    [SerializeField] private Health health;
    private Slider healingBar;
    private bool usingHealthPack = false;
    private float healingTimer;

    private Camera cam;

    private GameObject[] inventory;
    private int currentIventory = 0;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        inventory = new GameObject[3];

        pauseMenu = FindObjectOfType<PauseMenu>();

        HUDReferences = GameObject.FindGameObjectWithTag("PlayerHUD").GetComponent<HUDReferences>();
        HUDReferences.SetHotBar(true);
        healingBar = HUDReferences.GetHealingSlider();
        cam = Camera.main;
    }

    void Update()
    {
        if (!IsOwner) return;

        HoldItem();

        if (pauseMenu.IsPaused()) return;

        if (!usingHealthPack)
        {
            PickUpItem();
            ThrowItem();
            DropItem();
            Inventory();
        }
        HealthItem();
    }

    private void PickUpItem()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, 12, layer))
        {
            if (hit.transform.CompareTag("PickUp"))
            {
                if (Input.GetKeyDown(KeyCode.E) && inventory[currentIventory] == null)
                {
                    if (hit.transform.TryGetComponent(out ItemPickUp itemPickUp))
                    {
                        if (!itemPickUp.GetIsPickedUp())
                        {
                            hit.transform.GetComponentInChildren<BoxCollider>().enabled = false;

                            HUDReferences.SetHotBarImage(currentIventory, itemPickUp.GetImageIcon());
                            itemPickUp.ChangeItemOwnershipServerRpc(OwnerClientId);
                            itemPickUp.SetPickUpServerRpc(true);
                            hit.transform.GetComponent<Rigidbody>().useGravity = false;
                            inventory[currentIventory] = hit.transform.gameObject;
                        }
                    }
                }
            }
        }
    }

    private void ThrowItem()
    {
        if (Input.GetMouseButtonDown(0) && inventory[currentIventory] != null)
        {
            if (!inventory[currentIventory].GetComponent<ItemPickUp>().IsThrowable()) return;

            HUDReferences.SetHotBarImage(currentIventory, null);

            ItemPickUp thisObj = inventory[currentIventory].transform.GetComponent<ItemPickUp>();
            thisObj.SetPickUpServerRpc(false);

            Rigidbody rb = inventory[currentIventory].transform.GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.AddForce(cam.transform.forward * 70, ForceMode.Impulse);

            inventory[currentIventory] = null;
            thisObj.ThrownObjectServerRpc(true);
        }
    }

    private void DropItem()
    {
        if (Input.GetKeyDown(KeyCode.Q) && inventory[currentIventory] != null)
        {
            HUDReferences.SetHotBarImage(currentIventory, null);

            ItemPickUp thisObj = inventory[currentIventory].transform.GetComponent<ItemPickUp>();
            thisObj.SetPickUpServerRpc(false);
            Rigidbody rb = inventory[currentIventory].transform.GetComponent<Rigidbody>();
            rb.AddForce(cam.transform.forward * 7, ForceMode.Impulse);
            rb.useGravity = true;
            inventory[currentIventory].transform.GetComponentInChildren<BoxCollider>().enabled = true;
            inventory[currentIventory] = null;
        }
    }

    private void HoldItem()
    {
        if (inventory[currentIventory] == null) return;

        inventory[currentIventory].transform.position= hand.transform.position;
    }

    private void Inventory()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            SetInventorySlotActive(false);

            if (currentIventory == 2)
            {
                currentIventory = 0;
            }
            else
            {
                currentIventory++;
            }
            SetInventorySlotActive(true);
            HUDReferences.SetHightlightPosition(1);
        } 
        else if (Input.mouseScrollDelta.y < 0)
        {
            SetInventorySlotActive(false);
            if (currentIventory == 0)
            {
                currentIventory = 2;
            }
            else
            {
                currentIventory--;
            }
            SetInventorySlotActive(true);
            HUDReferences.SetHightlightPosition(0);
        }
    }

    private void SetInventorySlotActive(bool active)
    {
        if (inventory[currentIventory] != null)
        {
            inventory[currentIventory].GetComponent<ItemPickUp>().SetItemActiveServerRpc(active);
        }
    }

    private void HealthItem()
    {
        if (Input.GetKey(KeyCode.F) && inventory[currentIventory] != null)
        {
            if (inventory[currentIventory].TryGetComponent(out HealthPack healthPack))
            {
                if (!usingHealthPack)
                {
                    usingHealthPack = true;
                    healingTimer = 0;
                    healingBar.minValue = 0f;
                    healingBar.maxValue = healthPack.GetTimeToHeal();
                    healingBar.gameObject.SetActive(true);
                    movement.SlowDownEffectNoTimer(true);
                }

                if (healingTimer < healthPack.GetTimeToHeal())
                {
                    healingTimer += Time.deltaTime;
                    healingBar.value = healingTimer;
                }
                else
                {
                    usingHealthPack = false;
                    movement.SlowDownEffectNoTimer(false);
                    healingBar.gameObject.SetActive(false);
                    HUDReferences.SetHotBarImage(currentIventory, null);
                    health.AddHealthServerRpc(healthPack.GetHealth());
                    HealthItemDespawnServerRpc();
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.F) && usingHealthPack)
        {
            usingHealthPack = false;
            movement.SlowDownEffectNoTimer(false);
            healingBar.gameObject.SetActive(false);
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void HealthItemDespawnServerRpc()
    {
        inventory[currentIventory].GetComponent<NetworkObject>().Despawn();
        inventory[currentIventory] = null;
    }
}
