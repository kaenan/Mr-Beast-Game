using UnityEngine;

public class TaskSpawnInfo : MonoBehaviour
{
    [SerializeField] GameObject[] bucketSpawnLocations;

    public GameObject[] GetBucketSpawnLocations() { return bucketSpawnLocations; }
}
