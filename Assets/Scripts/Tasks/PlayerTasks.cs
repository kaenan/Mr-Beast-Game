using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTasks : NetworkBehaviour
{
    private HUDReferences HUDReferences;
    public Slider progressBar { get; set; }
    public TextMeshProUGUI helpText { get; set; }

    // Tasks
    private bool inHoldCarSphere = false;
    private bool holding = false;
    private Vector3 positionWhenHolding;
    private HoldCarForTime holdCarScript;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        HUDReferences = FindObjectOfType<HUDReferences>();

        progressBar = HUDReferences.GetProgressBar();
        helpText = HUDReferences.GetHelpText();
    }

    private void Update()
    {
        if (!IsOwner) return;

        HoldCar();
    }

    public bool GetProgressBarActive()
    {
        if (!IsOwner) return false;

        return progressBar.gameObject.activeSelf;
    }
    public bool GetHelpActive()
    {
        if (!IsOwner) return false;

        return helpText.gameObject.activeSelf;
    }

    public void SetProgressBar(bool active, int barMaxValue)
    {
        if (!IsOwner) return;

        progressBar.gameObject.SetActive(active);

        if (active)
        {
            progressBar.value = 0f;
            progressBar.maxValue = barMaxValue;
            progressBar.minValue= 0f;
        }
    }

    public void AddToProgressBar(float i)
    {
        if (!IsOwner) return;

        progressBar.value = i;
    }

    public void SetHelpText(bool active, string text)
    {
        if (!IsOwner) return;

        helpText.gameObject.SetActive(active);

        if (active)
        {
            helpText.text = text;
        }
    }

    public void SetInCarSphere(bool active, HoldCarForTime holdCarForTime)
    {
        if (!IsOwner) return;

        inHoldCarSphere = active;
        holdCarScript = holdCarForTime;
    }
     private void HoldCar()
    {
        if (!inHoldCarSphere) return;

        if (Input.GetKeyDown(KeyCode.E) && !holdCarScript.GetRewarded() && !holdCarScript.GetInUse())
        {
            holding = true;
            positionWhenHolding = new(transform.position.x, transform.position.y, transform.position.z);
            holdCarScript.SetInUseServerRpc(true);
        }

        if (holding)
        {
            if (transform.position.x > positionWhenHolding.x + 0.5f || transform.position.x < positionWhenHolding.x - 0.5f || 
                transform.position.z > positionWhenHolding.z + 0.5f || transform.position.z < positionWhenHolding.z - 0.5f)
            {
                holding = false;
                holdCarScript.SetInUseServerRpc(false);
            }

            if (progressBar.value >= progressBar.maxValue)
            {
                holding = false;
            }
        }
    }
}
