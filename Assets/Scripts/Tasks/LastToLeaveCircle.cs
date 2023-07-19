using UnityEngine;
using Unity.Netcode;

public class LastToLeaveCircle : NetworkBehaviour
{
    private GameVariables gameVars;
    public int numClients { get; set; }
    private bool rewarded = false;

    [Header ("Task Settings")]
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

    NetworkVariable<int> playersInCircle = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        gameVars = FindObjectOfType<GameVariables>();
        gameVolume = FindObjectOfType<GameVolume>();
    }

    void Update()
    {
        UpdateVolume();

        if (numClients != gameVars.GetConnectedClients() - 1) numClients = gameVars.GetConnectedClients() - 1;

        if (playersInCircle.Value == numClients)
        {
            if (!audio_source.isPlaying) audio_source.Play();
        } 
        else
        {
            if (audio_source.isPlaying) audio_source.Stop();
        }

        if (!IsHost) return;

        if (playersInCircle.Value == numClients)
        {
            if (timer.Value < progressTime)
            {
                AddToTimerServerRpc();
            }
            else if (!rewarded)
            {
                rewarded = true;
                
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
    private void SetPlayersInCircleServerRpc(int i)
    {
        playersInCircle.Value += i;
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

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.CompareTag("Person"))
        {
            SetPlayersInCircleServerRpc(1);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Person"))
        {
            if (other.TryGetComponent(out PlayerTasks tasks))
            {
                if (tasks.GetProgressBarActive()) tasks.SetProgressBar(false, progressTime);
                if (tasks.GetHelpActive()) tasks.SetHelpText(false, help);
            }

            if (!IsOwner) return;
            SetPlayersInCircleServerRpc(-1);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Person"))
        {
            if (other.TryGetComponent(out PlayerTasks tasks))
            {
                if (playersInCircle.Value != numClients && timer.Value < progressTime)
                {
                    if (!tasks.GetHelpActive()) tasks.SetHelpText(true, help);
                    if (tasks.GetProgressBarActive()) tasks.SetProgressBar(false, progressTime);
                    if (timer.Value != 0) ResetTimerServerRpc();
                }
                else if (playersInCircle.Value == numClients && timer.Value < progressTime)
                {
                    if (!tasks.GetProgressBarActive()) tasks.SetProgressBar(true, progressTime);
                    if (tasks.GetHelpActive()) tasks.SetHelpText(false, help);
                    tasks.AddToProgressBar(timer.Value);
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
