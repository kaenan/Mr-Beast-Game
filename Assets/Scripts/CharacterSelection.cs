using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelection : NetworkBehaviour
{
    [SerializeField] GameObject startGameObj;

    [Header("Lobby Stuff")]
    [SerializeField] private GameObject lobby;
    [SerializeField] private TextMeshProUGUI playerNames;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private GameObject refreshButton;

    [Header("Game Items")]
    [SerializeField] private GameObject playerButton;
    [SerializeField] private GameObject money;
    [SerializeField] private int moneyMinSpawn;
    [SerializeField] private int moneyMaxSpawn;

    private int playersNum = 0;
    private int connectedClients = 1;

    NetworkVariable<int> playersLoaded = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    NetworkVariable<LobbyNames> lobbyNames = new NetworkVariable<LobbyNames>(new LobbyNames { names = ""}, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct LobbyNames : INetworkSerializable
    {
        public string names;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref names);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            startGameButton.gameObject.SetActive(true);
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(() => StartGame());

            refreshButton.SetActive(true);

            roomCodeText.gameObject.SetActive(true);
            roomCodeText.text = "Room Code: " + PlayerPrefs.GetString("Room Code");
        } 
        
        if ((!IsHost && IsOwner) || (IsHost && !IsOwner))
        {
            lobby.SetActive(false);
            return;
        }

        GetConnectedPlayers();
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (connectedClients < 2)
            {
                startGameButton.enabled = false;
            }
            else
            {
                startGameButton.enabled = true;
            }

            GetConnectClientsServerRpc();
            if (playersNum != connectedClients)
            {
                StartCoroutine(DelayGetPlayers());
            }
        }
        else
        {
            playerNames.text = lobbyNames.Value.names;
        }
    }

    public void GetConnectedPlayers()
    {
        GetConnectClientsServerRpc();
        playersNum = connectedClients;

        string name = "";

        for (int i = 0; i < connectedClients; i++)
        {
            name = name + "Player " + i + "\n";
        }

        lobbyNames.Value = new LobbyNames { names= name };

        playerNames.text= lobbyNames.Value.names;
    }

    IEnumerator DelayGetPlayers()
    {
        yield return new WaitForSeconds(1);

        GetConnectedPlayers();
    }

    private void StartGame()
    {
        lobby.SetActive(false);
        var status = NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);

        NetworkManager.Singleton.SceneManager.OnLoadComplete += SceneManager_OnLoadComplete;
    }

    private void SceneManager_OnLoadComplete(ulong clientId, string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
    {
        PlayerLoadedServerRpc();

        if (playersLoaded.Value == NetworkManager.ConnectedClients.Count)
        {
            StartCoroutine(DelaySpawnStart());
        }
    }

    IEnumerator DelaySpawnStart()
    {
        yield return new WaitForSeconds(3);

        GameObject s = Instantiate(startGameObj);
        s.GetComponent<NetworkObject>().Spawn();
    }

    // Server Rpc's

    [ServerRpc(RequireOwnership = false)]
    private void GetConnectClientsServerRpc()
    {
        connectedClients = NetworkManager.ConnectedClients.Count;
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerLoadedServerRpc()
    {
        playersLoaded.Value++;
    }
}
