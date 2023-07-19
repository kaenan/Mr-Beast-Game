using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerEffects : NetworkBehaviour
{
    [SerializeField] PlayerMovement movementScript;

    public void SlowDownEffect(float effectLength)
    {
        movementScript.SlowDownEffect(effectLength);
    }

    public void Grappled (bool grappled)
    {
        movementScript.SetGrappledServerRpc(grappled);
    }
}
