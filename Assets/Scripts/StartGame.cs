using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class StartGame : NetworkBehaviour
{
    [SerializeField] private GameObject mrBeast;
    [SerializeField] private GameObject player;

    [Header("Game Task Objects")]
    [SerializeField] private GameObject gameVariables;
    [SerializeField] private GameObject[] taskObjects;

    [Header("Game Items")]
    [SerializeField] private GameObject playerButton;
    [SerializeField] private GameObject money;
    [SerializeField] private int moneyMinSpawn;
    [SerializeField] private int moneyMaxSpawn;
    [SerializeField] private GameObject health;
    [SerializeField] private int healthMinSpawn;

    private SpawnLocations spawnLocations;
    private List<GameObject> locations;

    public override void OnNetworkSpawn()
    {
        if (!IsHost) return;

        List<CharacterSelection> characterSelections = new List<CharacterSelection>();
        characterSelections.AddRange(FindObjectsOfType<CharacterSelection>());

        foreach (CharacterSelection cs in characterSelections)
        {
            cs.gameObject.GetComponent<NetworkObject>().Despawn();
        }

        spawnLocations = FindObjectOfType<SpawnLocations>();
        locations = new List<GameObject>();
        locations.AddRange(spawnLocations.GetPlayerLocations());

        ulong mrbeastClient = NetworkManager.ConnectedClientsIds[Random.Range(0, NetworkManager.ConnectedClients.Count)];
        StartGameServer(mrbeastClient);
    }

    public void StartGameServer(ulong mrbeastClient)
    {
        SpawnGameVariables(mrbeastClient);

        int location;

        for (int i = 0; i < NetworkManager.ConnectedClients.Count; i++)
        {
            if (mrbeastClient == NetworkManager.ConnectedClientsIds[i])
            {
                GameObject p1 = Instantiate(mrBeast, spawnLocations.GetMrBeastLocations().transform.position, Quaternion.identity);
                p1.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.ConnectedClientsIds[i]);
            }
            else
            {
                location = Random.Range(0, locations.Count);
                GameObject p1 = Instantiate(player, locations[location].transform.position, Quaternion.identity);
                p1.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.ConnectedClientsIds[i]);
                locations.RemoveAt(location);
            }
        }
        SpawnTasks();
        SpawnItems();
    }

    private void SpawnGameVariables(ulong mrbeastID)
    {
        GameObject vars = Instantiate(gameVariables, Vector3.zero, Quaternion.identity);
        vars.GetComponent<NetworkObject>().Spawn();
        vars.GetComponent<GameVariables>().mrBeastID = mrbeastID;
        vars.GetComponent<GameVariables>().hostID = OwnerClientId;
    }

    private void SpawnTasks()
    {
        List<GameObject> taskLocations = new List<GameObject>();
        taskLocations.AddRange(spawnLocations.GetTaskLocations());

        int i;

        foreach (GameObject t in taskObjects)
        {
            i = Random.Range(0, taskLocations.Count - 1);
            GameObject t2 = Instantiate(t, taskLocations[i].transform.position, Quaternion.identity);

            if (t2.TryGetComponent(out FillPool pool))
            {
                pool.SetBucketSpawnLocations(taskLocations[i].GetComponent<TaskSpawnInfo>().GetBucketSpawnLocations());
            }

            t2.GetComponent<NetworkObject>().Spawn();
            taskLocations.RemoveAt(i);
        }
    }

    private void SpawnItems()
    {
        // Spawn Play Buttons

        List<GameObject> playButtons = new List<GameObject>();
        playButtons.AddRange(spawnLocations.GetPlayButtonSpawns());

        int randomSpawn, numMoneyLocations, numSpawn = Random.Range(3, playButtons.Count);

        for (int j = 0; j < numSpawn; j++)
        {
            randomSpawn = Random.Range(0, playButtons.Count);
            GameObject pb = Instantiate(playerButton, playButtons[randomSpawn].transform.position, Quaternion.identity);
            pb.GetComponent<NetworkObject>().Spawn();
            playButtons.RemoveAt(randomSpawn);
        }

        // Spawn Money at random locations around map.

        List<int> moneyList = new List<int>();
        GameObject moneyLocations = spawnLocations.GetMoneySpawns();
        numMoneyLocations = moneyLocations.transform.childCount;
        numSpawn = Random.Range(moneyMinSpawn, moneyMaxSpawn);

        for (int j = 0; j < numSpawn; j++)
        {
            randomSpawn = Random.Range(0, numMoneyLocations);

            if (moneyList.Contains(randomSpawn))
            {
                j--;
            }
            else
            {
                GameObject m = Instantiate(money, moneyLocations.transform.GetChild(randomSpawn).transform.position, Quaternion.identity);
                m.GetComponent<NetworkObject>().Spawn();
                moneyList.Add(randomSpawn);
            }
        }

        // Spawn Health Kits

        playButtons = new List<GameObject>();
        playButtons.AddRange(spawnLocations.GetHealthSpawns());
        numSpawn = Random.Range(healthMinSpawn, playButtons.Count);

        for (int j = 0; j < numSpawn; j++)
        {
            randomSpawn = Random.Range(0, playButtons.Count - 1);
            GameObject pb = Instantiate(health, playButtons[randomSpawn].transform.position, Quaternion.identity);
            pb.GetComponent<NetworkObject>().Spawn();
            playButtons.RemoveAt(randomSpawn);
        }
    }
}
