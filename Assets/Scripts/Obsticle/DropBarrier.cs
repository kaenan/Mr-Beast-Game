using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;

public class DropBarrier : NetworkBehaviour
{
    [SerializeField] private Collider barrierCollider;
    [SerializeField] private GameObject mesh;
    [SerializeField] private GameObject endPos;
    [SerializeField] private GameObject originalPos;

    [Header("Reset")]
    [SerializeField] private float resetTime;
    private float resetTimer;
    private float animateTime = 1f;
    private float animateTimer = 0f;
    NetworkVariable<bool> activated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Audio")]
    [SerializeField] private AudioSource audio_source;
    private GameVolume gameVolume;

    public override void OnNetworkSpawn()
    {
        gameVolume = FindObjectOfType<GameVolume>();

        barrierCollider.enabled = activated.Value;
    }

    private void Update()
    {
        UpdateVolume();

        if (activated.Value && !barrierCollider.enabled) 
        {
            barrierCollider.enabled = activated.Value;
            resetTimer = resetTime;
            animateTimer = 0f;
            audio_source.Play();
        }

        if (activated.Value && (mesh.transform.position != endPos.transform.position || mesh.transform.rotation != endPos.transform.rotation))
        {
            mesh.transform.position = Vector3.Lerp(mesh.transform.position, endPos.transform.position, Time.deltaTime * 5);
            mesh.transform.rotation = Quaternion.Lerp(mesh.transform.rotation, endPos.transform.rotation, Time.deltaTime * 5);
            animateTimer += Time.deltaTime;

            if (animateTimer >= animateTime) 
            {
                mesh.transform.position = endPos.transform.position;
                mesh.transform.rotation = endPos.transform.rotation;
                animateTimer = 0f;
            }
        }

        if (activated.Value)
        {
            resetTimer -= Time.deltaTime;
        }

        if (resetTimer <= 0)
        {
            if (IsOwner) ResetBarrierServerRpc();

            barrierCollider.enabled = activated.Value;
            animateTimer = 0f;

            if (mesh.transform.position != originalPos.transform.position || mesh.transform.rotation != originalPos.transform.rotation)
            {
                mesh.transform.position = Vector3.Lerp(mesh.transform.position, originalPos.transform.position, Time.deltaTime * 5);
                mesh.transform.rotation = Quaternion.Lerp(mesh.transform.rotation, originalPos.transform.rotation, Time.deltaTime * 5);
                animateTimer += Time.deltaTime;

                if (animateTimer >= animateTime)
                {
                    mesh.transform.position = originalPos.transform.position;
                    mesh.transform.rotation = originalPos.transform.rotation;
                }
            } else
            {
                resetTimer = resetTime;
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void ActivateBarrierServerRpc()
    {
        activated.Value = true;
    }
    
    [ServerRpc (RequireOwnership = false)]
    public void ResetBarrierServerRpc()
    {
        activated.Value = false;
    }

    public bool IsActivated()
    {
        return activated.Value;
    }

    private void UpdateVolume()
    {
        if (audio_source.volume != gameVolume.sfxVolume) audio_source.volume = gameVolume.sfxVolume;
    }
}
