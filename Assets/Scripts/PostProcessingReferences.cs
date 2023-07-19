using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessingReferences : MonoBehaviour
{
    [SerializeField] private PostProcessVolume blurVolume;

    public PostProcessVolume GetBlurVolume() { return blurVolume; }
}
