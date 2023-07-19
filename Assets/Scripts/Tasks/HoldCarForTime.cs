using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HoldCarForTime : NetworkBehaviour
{

    [Header("Task Settings")]
    [SerializeField] private string help;
    [SerializeField] private int progressTime;

    [Header("Reward Settings")]
    [SerializeField] private GameObject rewardObject;
    [SerializeField] private int maxReward;
    [SerializeField] private int minReward;
    [SerializeField] private GameObject[] rewardSpawnRegion;

    [Header("Audio")]
    [SerializeField] private AudioSource audio_source;
    private GameVolume gameVolume;

    NetworkVariable<float> timer = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    NetworkVariable<bool> inUse = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    NetworkVariable<bool> rewarded = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        gameVolume = FindObjectOfType<GameVolume>();
    }

    void Update()
    {
        UpdateVolume();

        if (!inUse.Value || rewarded.Value)
        {
            if (audio_source.isPlaying) audio_source.Stop();
        }
        else if (inUse.Value)
        {
            if (!audio_source.isPlaying) audio_source.Play();
        }

        if (!IsHost) return;

        if (inUse.Value && !rewarded.Value)
        {
            if (timer.Value < progressTime)
            {
                AddToTimerServerRpc();
            }
            else if (!rewarded.Value)
            {
                SetRewardedServerRpc(true);

                int reward = Random.Range(minReward, maxReward);
                Vector3 spawn;

                for (int i = 0; i < reward; i++)
                {
                    spawn = new(Random.Range(rewardSpawnRegion[0].transform.position.x, rewardSpawnRegion[1].transform.position.x), rewardSpawnRegion[0].transform.position.y,
                        Random.Range(rewardSpawnRegion[0].transform.position.z, rewardSpawnRegion[1].transform.position.z));

                    var r = Instantiate(rewardObject, spawn, Quaternion.identity);
                    r.GetComponent<NetworkObject>().Spawn();
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddToTimerServerRpc()
    {
        timer.Value += Time.deltaTime;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetTimerServerRpc()
    {
        timer.Value = 0f;
    }

    [ServerRpc (RequireOwnership = false)]
    public void SetInUseServerRpc(bool use)
    {
        inUse.Value= use;
    }
    
    [ServerRpc (RequireOwnership = false)]
    public void SetRewardedServerRpc(bool r)
    {
        rewarded.Value= r;
    }
    
    public bool GetRewarded()
    {
        return rewarded.Value;
    }

    public bool GetInUse()
    {
        return inUse.Value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Person") && !rewarded.Value)
        {
            if (other.TryGetComponent(out PlayerTasks tasks))
            {
                tasks.SetInCarSphere(true, this);

                if (!tasks.GetHelpActive()) tasks.SetHelpText(true, help);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Person"))
        {
            if (other.TryGetComponent(out PlayerTasks tasks))
            {
                tasks.SetInCarSphere(false, this);

                if (tasks.GetHelpActive()) tasks.SetHelpText(false, help);
                if (tasks.GetProgressBarActive()) tasks.SetProgressBar(false, progressTime);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Person"))
        {
            if (other.TryGetComponent(out PlayerTasks tasks))
            {
                if (inUse.Value && !rewarded.Value)
                {
                    if (!tasks.GetProgressBarActive()) tasks.SetProgressBar(true, progressTime);
                    tasks.AddToProgressBar(timer.Value);
                } 
                else if (!inUse.Value && !rewarded.Value)
                {
                    if (tasks.GetProgressBarActive()) tasks.SetProgressBar(false, progressTime);
                    if (timer.Value != 0) ResetTimerServerRpc();
                } 
                else
                {
                    if (tasks.GetProgressBarActive()) tasks.SetProgressBar(false, progressTime);
                    if (tasks.GetHelpActive()) tasks.SetHelpText(false, help);
                }
            }
        }
    }

    private void UpdateVolume()
    {
        if (audio_source.volume != gameVolume.sfxVolume) audio_source.volume = gameVolume.sfxVolume;
    }
}
