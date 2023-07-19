using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayButtonAreaEffect : NetworkBehaviour
{
    [SerializeField] float damage;
    [SerializeField] float slowEffectLength;
    [SerializeField] SphereCollider s_collider;

    private float timer;

    public override void OnNetworkSpawn()
    {
        timer = 1.2f;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f) s_collider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.TryGetComponent(out Health health))
        {
            health.HitByObject(damage);
        }

        if (other.TryGetComponent(out PlayerEffects effects))
        {
            effects.SlowDownEffect(slowEffectLength);
        }

        KillSloth(other);
    }

    private void KillSloth(Collider other)
    {
        if (other.TryGetComponent(out SlothMovement slothMovement))
        {
            slothMovement.KillSloth();
        }
    }
}
