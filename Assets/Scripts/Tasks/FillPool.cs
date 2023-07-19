using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FillPool : NetworkBehaviour
{
    [SerializeField] private FillPoolHelpText helpText;
    private int objectsInPool = 0;
    NetworkVariable<int> objectsForRewards = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    NetworkVariable<bool> objectAdded = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Pool Objects")]
    [SerializeField] private GameObject poolObjects;
    [SerializeField] private List<GameObject> spawnLocations;

    [Header("Reward")]
    [SerializeField] private int minToWin;
    [SerializeField] private int maxToWin;
    [SerializeField] private int minReward;
    [SerializeField] private int maxReward;
    [SerializeField] private GameObject[] rewardLocations;
    [SerializeField] private GameObject reward;

    [Header("Audio")]
    [SerializeField] private AudioSource audio_source;
    [SerializeField] private AudioClip audio_clip;
    private GameVolume gameVolume;

    public override void OnNetworkSpawn()
    {
        helpText.taskDone= false;

        gameVolume = FindObjectOfType<GameVolume>();

        if (IsOwner)
        {
            SetForRewardServerRpc();
            for (int i = 0; i <= objectsForRewards.Value; i++)
            {
                int k = Random.Range(0, spawnLocations.Count);
                GameObject j = Instantiate(poolObjects, spawnLocations[k].transform.position, Quaternion.identity);
                j.GetComponent<NetworkObject>().Spawn();
                spawnLocations.RemoveAt(k);
            }
        }

        helpText.helpText = "Find " + objectsForRewards.Value.ToString() + " buckets of orbeez and fill the pool.";
    }

    [ServerRpc (RequireOwnership = false)]
    private void SetForRewardServerRpc()
    {
        objectsForRewards.Value = Random.Range(minToWin, maxToWin);
    }

    void Update()
    {
        UpdateVolume();

        if (objectAdded.Value)
        {
            audio_source.PlayOneShot(audio_clip);

            if (IsOwner) objectAdded.Value = false;
        }

        if (!IsOwner) return;

        if (objectsInPool >= objectsForRewards.Value && !helpText.taskDone)
        {
            helpText.taskDone = true;

            int rewardNum = Random.Range(minReward, maxReward);

            for (int i = 0; i < rewardNum; i++)
            {
                Vector3 pos = new(Random.Range(rewardLocations[0].transform.position.x, rewardLocations[1].transform.position.x), rewardLocations[0].transform.position.y,
                    Random.Range(rewardLocations[0].transform.position.z, rewardLocations[1].transform.position.z));

                GameObject r = Instantiate(reward, pos, Quaternion.identity);
                r.GetComponent<NetworkObject>().Spawn();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pool Object") && other.transform.parent.TryGetComponent(out ItemPickUp itemPickUp))
        {
            if (!itemPickUp.GetIsPickedUp() && !itemPickUp.GetIsThrown())
            {
                if (IsOwner)
                {
                    other.GetComponentInParent<NetworkObject>().Despawn();
                    objectsInPool++;
                    objectAdded.Value = true;
                }
                //audio_source.PlayOneShot(audio_clip);
            }
        }
    }

    public void SetBucketSpawnLocations(GameObject[] locations)
    {
        spawnLocations = new List<GameObject>();
        spawnLocations.AddRange(locations);
    }

    private void UpdateVolume()
    {
        if (audio_source.volume != gameVolume.sfxVolume) audio_source.volume = gameVolume.sfxVolume;
    }
}
