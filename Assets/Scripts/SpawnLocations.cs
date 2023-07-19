using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnLocations : MonoBehaviour
{
    [Header("Player Locations")]
    [SerializeField] GameObject mrBeastLocation;
    [SerializeField] GameObject[] playerLocations;

    [Header("Task Locations")]
    [SerializeField] GameObject[] taskLocations;

    [Header("Items")]
    [SerializeField] private GameObject[] playButtonsSpawns;
    [SerializeField] private GameObject moneySpawns;
    [SerializeField] private GameObject[] healthSpawns;
    
    public GameObject GetMrBeastLocations () { return mrBeastLocation; }
    public GameObject[] GetPlayerLocations () { return playerLocations; }
    public GameObject[] GetTaskLocations () { return taskLocations; }
    public GameObject[] GetPlayButtonSpawns() { return playButtonsSpawns; }
    public GameObject GetMoneySpawns() { return moneySpawns; }
    public GameObject[] GetHealthSpawns() { return healthSpawns; }
}
