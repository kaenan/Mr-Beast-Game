using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ParticlesScript : NetworkBehaviour
{
    [SerializeField] ParticleSystem particle;
    [SerializeField] float particleDuration;
    [SerializeField] NetworkObject networkObject;
    private GameVariables gameVariables;

    public override void OnNetworkSpawn()
    {
        gameVariables = FindObjectOfType<GameVariables>();
        StartCoroutine(RemoveParticleTimer());
    }

    IEnumerator RemoveParticleTimer()
    {
        yield return new WaitForSeconds(particleDuration);

        gameVariables.AddObjectToDestroy(NetworkObjectId, gameObject);
    }
}
