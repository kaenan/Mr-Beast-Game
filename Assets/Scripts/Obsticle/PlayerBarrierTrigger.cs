using Unity.Netcode;
using UnityEngine;

public class PlayerBarrierTrigger : NetworkBehaviour
{
    private DropBarrier barrier;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Barrier"))
        {
            if (other.TryGetComponent(out DropBarrier barrier))
            {
                if (barrier.IsActivated()) return;
                this.barrier = barrier;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Barrier"))
        {
            barrier = null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && barrier != null)
        {
            barrier.ActivateBarrierServerRpc();
            barrier = null;
        }
    }
}
