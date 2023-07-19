using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class BreafCase : NetworkBehaviour
{
    private GameVariables gameVars;
    private int amountLastChecked = 0;

    [SerializeField] private TextMeshProUGUI amountDue;

    public override void OnNetworkSpawn()
    {
        gameVars = FindObjectOfType<GameVariables>();
        amountDue.text = "Amount To Win:\n$" + (gameVars.GetAmountCollected() * 10000).ToString() + " / $" + (gameVars.GetAmountToWin() * 10000).ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Money Stack"))
        {
            if (IsOwner)
            {
                ItemPickUp item = other.GetComponentInParent<ItemPickUp>();

                if (item.GetIsThrown()) return;

                gameVars.AddMoneyCollectedServerRpc();

                item.SetPickUpServerRpc(true);
            }
        }
    }

    private void Update()
    {
        if (gameVars.GetAmountCollected() != amountLastChecked)
        {
            amountDue.text = "Amount To Win:\n$" + (gameVars.GetAmountCollected() * 10000).ToString() + " / $" + (gameVars.GetAmountToWin() * 10000).ToString();
            amountLastChecked= gameVars.GetAmountCollected();
        }
    }
}
