using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartTypeOnSceneLoad : MonoBehaviour
{
    [SerializeField] private bool RelayTransport = false;

    void Start()
    {
        Time.timeScale = 1.0f;

        if (RelayTransport)
        {
            RelayTransportOption();
        }
        else
        {
            if (PlayerPrefs.GetString("Load Type") == "Host")
            {
                NetworkManager.Singleton.StartHost();
            }
            else if (PlayerPrefs.GetString("Load Type") == "Client")
            {
                NetworkManager.Singleton.StartClient();
            }
        }
    }

    private async void RelayTransportOption()
    {
        await UnityServices.InitializeAsync();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        if (PlayerPrefs.GetString("Load Type") == "Host")
        {
            CreateRelay();
        }
        else if (PlayerPrefs.GetString("Load Type") == "Client")
        {
            JoinRelay();
        }
        else
        {
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }
    }

    private async void CreateRelay()
    {
        try
        {
            Allocation allocation =  await RelayService.Instance.CreateAllocationAsync(4);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            PlayerPrefs.SetString("Room Code", joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }

    private async void JoinRelay()
    {
        try
        {
            string code = PlayerPrefs.GetString("Room Code");
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(code);

            RelayServerData relayServerData = new RelayServerData(joinAlloc, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }
}
