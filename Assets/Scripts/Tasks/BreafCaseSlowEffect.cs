using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BreafCaseSlowEffect : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.CompareTag("MrBeast") && other.transform.parent.TryGetComponent(out PlayerMovement movement))
        {
            movement.SlowDownEffectNoTimer(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("MrBeast") && other.TryGetComponent(out Health health))
        {
            health.TakingDamage(true, transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;

        if (other.CompareTag("MrBeast") && other.transform.parent.TryGetComponent(out PlayerMovement movement) && other.TryGetComponent(out Health health))
        {
            movement.SlowDownEffectNoTimer(false);
            health.TakingDamage(false, transform.position);
        }
    }
}
