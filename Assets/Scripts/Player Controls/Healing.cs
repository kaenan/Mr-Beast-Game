using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class Healing : NetworkBehaviour
{
    private HUDReferences HUDReferences;
    private Slider healingBar;
    private GameObject healText;

    [SerializeField] Health myHealth;
    [SerializeField] float healthToAdd;
    [SerializeField] float healTime;
    private float healTimer;
    private bool isHealing = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        HUDReferences= FindObjectOfType<HUDReferences>();
        healText = HUDReferences.GetHealText();
        healingBar = HUDReferences.GetHealingSlider();
    }

    public float GetHealth()
    {
        return myHealth.GetHealth();
    }

    public void HealPlayer(float health)
    {
        myHealth.AddHealthServerRpc(health);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsOwner) return;

        if (other.TryGetComponent(out Healing healing))
        {
            if (healing.GetHealth() <= 0)
            {
                if (!healText.activeSelf) healText.SetActive(true);

                if (Input.GetKeyDown(KeyCode.F))
                {
                    isHealing = true;
                    healTimer = 0f;
                    healingBar.minValue = 0f;
                    healingBar.maxValue = healTime;
                    healingBar.gameObject.SetActive(true);
                }
                
                if (Input.GetKeyUp(KeyCode.F))
                {
                    isHealing = false;
                    healingBar.gameObject.SetActive(false);
                }

                if (Input.GetKey(KeyCode.F))
                {
                    healTimer += Time.deltaTime;
                    healingBar.value = healTimer;
                    if (healTimer >= healTime && isHealing)
                    {
                        isHealing = false;
                        healing.HealPlayer(healthToAdd);
                        healingBar.gameObject.SetActive(false);
                    }
                }
            } else
            {
                if (healText.activeSelf) healText.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;

        if (healingBar.gameObject.activeSelf) healingBar.gameObject.SetActive(false);
        if (healText.activeSelf) healText.SetActive(false);
    }
}
