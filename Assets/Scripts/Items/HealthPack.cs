using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HealthPack : NetworkBehaviour
{
    [SerializeField] private float health;
    [SerializeField] private float timeToHeal;

    public float GetHealth() { return health; }
    public float GetTimeToHeal() { return timeToHeal; }
}
