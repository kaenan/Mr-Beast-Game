using Unity.Netcode;
using UnityEngine;

public class ItemPickUp : NetworkBehaviour
{
    [SerializeField] private NetworkObject networkObject;
    [SerializeField] private Sprite imageIcon;
    [SerializeField] private GameObject mesh;

    [Header("Thrown Object")]
    [SerializeField] private bool throwable;
    [SerializeField] private SphereCollider sphereCollider;
    [SerializeField] private GameObject particle;
    private GameObject spawnedParticle;

    NetworkVariable<bool> pickedUp = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    NetworkVariable<bool> visible = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    NetworkVariable<bool> thrown = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [ServerRpc (RequireOwnership = false)]
    public void ChangeItemOwnershipServerRpc(ulong clientID)
    {
        if (clientID == OwnerClientId) return;
        networkObject.ChangeOwnership(clientID);
    }

    [ServerRpc (RequireOwnership = false)]
    public void SetPickUpServerRpc(bool isPickedUp)
    {
        pickedUp.Value= isPickedUp;
    }

    [ServerRpc (RequireOwnership = false)]
    public void SetItemActiveServerRpc(bool active)
    {
        visible.Value= active;
        mesh.SetActive(visible.Value);
    }

    [ServerRpc (RequireOwnership = false)]
    public void ThrownObjectServerRpc(bool isThrown)
    {
        thrown.Value= isThrown;
        sphereCollider.enabled = thrown.Value;
    }

    [ServerRpc (RequireOwnership = false)]
    private void SpawnParticleServerRpc()
    {
        SpawnParticle();
    }

    [ServerRpc (RequireOwnership = false)]
    private void DespawnThisObjectServerRpc()
    {
        DespawnThisObject();
    }

    public bool GetIsPickedUp()
    {
        return pickedUp.Value;
    }

    public bool GetIsThrown()
    {
        return thrown.Value;
    }

    public bool IsThrowable()
    {
        return throwable;
    }

    public Sprite GetImageIcon()
    {
        return imageIcon;
    }

    private void Update()
    {
        if (visible.Value != mesh.activeSelf)
        {
            mesh.SetActive(visible.Value);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!thrown.Value) return;
        if (other.CompareTag("Hand")) return;

        if (particle != null)
        {
            SpawnParticleServerRpc();
        }

        DespawnThisObjectServerRpc();
    }

    private void SpawnParticle()
    {
        spawnedParticle = Instantiate(particle, transform.position, Quaternion.identity);
        spawnedParticle.GetComponent<NetworkObject>().Spawn();
    }

    private void DespawnThisObject()
    {
        networkObject.Despawn(true);
    }
}
