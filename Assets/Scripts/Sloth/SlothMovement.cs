using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class SlothMovement : NetworkBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform playerHoldPosition;

    [Header("Movement")]
    [SerializeField] private float acceleration;
    [SerializeField] private float runningSpeed;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float attackStopDistance;

    [Header("Distance Controls")]
    [SerializeField] private float minDistanceFromAttackingPlayer;

    [Header("Time Active")]
    [SerializeField] private float maxTimeAliveOnceAttacking;
    private float aliveTimer;

    [Header("Animation")]
    [SerializeField] private ClientNetworkAnimator c_animator;

    [Header("Sound")]
    [SerializeField] private AudioSource audio_source;
    private GameVolume gameVolume;

    private Transform mrBeast;
    private GameVariables gameVariables;

    private Transform attackPlayer;
    private PlayerMovement attackPlayerMovement;
    private List<PlayerPickUp> players;
    private MovementState state;

    private enum MovementState
    {
        running,
        attack,
        die
    }

    public override void OnNetworkSpawn()
    {
        gameVolume = FindObjectOfType<GameVolume>();

        if (!IsOwner) return;

        gameVariables = FindObjectOfType<GameVariables>();

        agent.acceleration = acceleration;
        agent.speed = runningSpeed;
        agent.stoppingDistance = 0;
        aliveTimer = 0;

        mrBeast = FindObjectOfType<MrBeastPlayer>().transform;

        players = new List<PlayerPickUp>();
        players.AddRange(FindObjectsOfType<PlayerPickUp>());

        ChoosePlayerToAttack();

        state = MovementState.running;
    }

    private void ChoosePlayerToAttack()
    {
        if (players.Count == 0)
        {
            state = MovementState.die;
            return;
        }

        int i;

        if (players.Count == 1)
        {
            i = 0;
        } else
        {
            i = Random.Range(0, players.Count);
        }

        if (players[i].transform.GetComponentInChildren<Health>().GetHealth() <= 0)
        {
            players.RemoveAt(i);
            ChoosePlayerToAttack();
            return;
        }

        attackPlayer = players[i].transform;
    }

    void Update()
    {
        UpdateVolume();

        if (!IsOwner) return;

        if (state != MovementState.die)
        {
            if (Vector3.Distance(transform.position, attackPlayer.position) <= minDistanceFromAttackingPlayer && state != MovementState.attack)
            {
                state = MovementState.attack;

                agent.stoppingDistance = attackStopDistance;
                agent.speed = attackSpeed;

                attackPlayer.GetComponent<PlayerMovement>().SetGrappledServerRpc(true);
                attackPlayerMovement = attackPlayer.GetComponent<PlayerMovement>();
                c_animator.SetTrigger("Push");
            }

            if (state == MovementState.attack)
            {
                aliveTimer += Time.deltaTime;

                if (aliveTimer >= maxTimeAliveOnceAttacking && state != MovementState.die)
                {
                    attackPlayer.GetComponent<PlayerMovement>().SetGrappledServerRpc(false);
                    state = MovementState.die;
                    c_animator.SetTrigger("Die");
                    StartCoroutine(DelayDespawn());
                }
            }
        }

        RunningState();
        AttackState();
        DieState();
    }

    private void RunningState()
    {
        if (state != MovementState.running) return;

        agent.destination = attackPlayer.position;
    }

    private void AttackState()
    {
        if (state != MovementState.attack) return;

        agent.destination = mrBeast.position;
        attackPlayerMovement.MovePlayerClientRpc(playerHoldPosition.position);
    }

    private void DieState()
    {
        if (state != MovementState.die) return;

        agent.isStopped= true;
    }

    IEnumerator DelayDespawn()
    {
        yield return new WaitForSeconds(3);

        gameVariables.AddObjectToDestroy(NetworkObjectId, gameObject);
    }

    public void KillSloth()
    {
        attackPlayer.GetComponent<PlayerMovement>().SetGrappledServerRpc(false);
        state = MovementState.die;
        c_animator.SetTrigger("Die");
        StartCoroutine(DelayDespawn());
    }

    private void UpdateVolume()
    {
        if (audio_source.volume != gameVolume.sfxVolume) audio_source.volume = gameVolume.sfxVolume;
    }
}
