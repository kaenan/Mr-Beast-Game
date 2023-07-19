using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class FillPoolHelpText : NetworkBehaviour
{
    public string helpText { get; set; }
    public bool taskDone { get; set; }

    private void OnTriggerEnter(Collider other)
    {
        if (taskDone) return;

        if (other.CompareTag("Person"))
        {
            if (other.TryGetComponent(out PlayerTasks tasks))
            {
                if (!tasks.GetHelpActive()) tasks.SetHelpText(true, helpText);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Person"))
        {
            if (other.TryGetComponent(out PlayerTasks tasks))
            {
                if (tasks.GetHelpActive()) tasks.SetHelpText(false, helpText);
            }
        }
    }
}
