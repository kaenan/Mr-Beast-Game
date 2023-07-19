using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LobbyScript : NetworkBehaviour
{
    NetworkVariable<string> playerNames = new NetworkVariable<string>("reger", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    NetworkVariable<bool> namesUpdated = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private TextMeshProUGUI playerNamesText;

    private void Update()
    {
        if (!namesUpdated.Value) return;

        playerNamesText.text = playerNames.Value;

        namesUpdated.Value = false;

        Debug.Log("Player Count Updated " + playerNames.Value);
    }

    public void AddNickName(string name)
    {
        playerNames.Value = name;

        namesUpdated.Value = true;

        Debug.Log("Player Sent Name To Lobby");
    }
}
