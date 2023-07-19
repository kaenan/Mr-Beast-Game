using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameVariables : NetworkBehaviour
{
    [Header("Win Messages")]
    [SerializeField] private string mrBeastWin;
    [SerializeField] private string personWin;
    [SerializeField] private string mrBeastDeadWin;

    [SerializeField] private int minAmountToWin;
    [SerializeField] private int maxAmountToWin;

    private GameOverUI gameOverUI;

    NetworkVariable<int> connectClientsNum = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    NetworkVariable<int> collectedMoney = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    NetworkVariable<int> amountToWin = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    NetworkVariable<int> playersDead = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    NetworkVariable<bool> mrBeastDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    NetworkVariable<bool> gameOver = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public ulong mrBeastID { get; set; }
    public ulong hostID { get; set; }

    private Dictionary<ulong, GameObject> destroyObjects;

    public override void OnNetworkSpawn()
    {
        gameOverUI = FindObjectOfType<GameOverUI>();
        destroyObjects = new Dictionary<ulong, GameObject>();

        if (!IsOwner) return;
        SetConnectedClientsServerRpc();
        SetAmountToWinServerRpc();
    }

    private void Update()
    {
        if (gameOver.Value)
        {
            gameOverUI.GetgameOverCanvas().SetActive(true);
            if (playersDead.Value == connectClientsNum.Value - 1)
            {
                gameOverUI.GetgameOverText().text = mrBeastWin;
            }
            else if (collectedMoney.Value >= amountToWin.Value)
            {
                gameOverUI.GetgameOverText().text = personWin;
            }
            else if (mrBeastDead.Value)
            {
                gameOverUI.GetgameOverText().text = mrBeastDeadWin;
            }
            Time.timeScale = 0;
        }

        if (!IsHost) return;

        if (!gameOver.Value)
        {
            if (playersDead.Value == connectClientsNum.Value - 1)
            {
                gameOver.Value = true;
            }
            else if (collectedMoney.Value >= amountToWin.Value)
            {
                gameOver.Value = true;
            }
            else if (mrBeastDead.Value)
            {
                gameOver.Value = true;
            }
        }

        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
    }

    private void Singleton_OnClientDisconnectCallback(ulong obj)
    {
        if (obj == mrBeastID || obj == hostID)
        {
            SetMrBeastDeadServerRpc();
        }
        else
        {
            GameObject[] people = GameObject.FindGameObjectsWithTag("Person");
            ResetPlayersDeadServerRpc();

            foreach (GameObject go in people)
            {
                if (go.TryGetComponent(out Health health))
                {
                    if (health.GetHealth() <= 0)
                    {
                        AddPlayerDeadServerRpc();
                    }
                }
            }
        }
        StartCoroutine(WaitSetConnectedClients());
    }

    IEnumerator WaitSetConnectedClients()
    {
        yield return new WaitForSeconds(2);

        SetConnectedClientsServerRpc();
    }

    [ServerRpc (RequireOwnership = false)]
    private void SetConnectedClientsServerRpc()
    {
        connectClientsNum.Value = NetworkManager.ConnectedClients.Count;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetAmountToWinServerRpc()
    {
        amountToWin.Value = Random.Range(minAmountToWin, maxAmountToWin);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddMoneyCollectedServerRpc()
    {
        collectedMoney.Value++;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerDeadServerRpc()
    {
        playersDead.Value++;
    }

    [ServerRpc (RequireOwnership = false)]
    public void RemovePlayerDeadServerRpc()
    {
        playersDead.Value--;
    }

    [ServerRpc (RequireOwnership = false)]
    private void ResetPlayersDeadServerRpc()
    {
        playersDead.Value = 0;
    }

    [ServerRpc (RequireOwnership = false)]
    public void SetMrBeastDeadServerRpc()
    {
        mrBeastDead.Value = true;
    }

    public int GetConnectedClients() { return connectClientsNum.Value; }
    public int GetAmountToWin() { return amountToWin.Value; }
    public int GetAmountCollected() { return collectedMoney.Value; }
    public bool IsGameOver() { return gameOver.Value; }


    public void AddObjectToDestroy(ulong id, GameObject obj)
    {
        destroyObjects.Add(id, obj);
        DestroyObjectServerRpc(id);
    }

    [ServerRpc (RequireOwnership =false)]
    private void DestroyObjectServerRpc(ulong id)
    {
        destroyObjects[id].GetComponent<NetworkObject>().Despawn();
    }
}
