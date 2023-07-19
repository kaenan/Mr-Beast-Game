using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameVolume : MonoBehaviour
{
    [SerializeField] private AudioSource backgroundSFX;

    public float musicVolume { get; private set; }
    public float sfxVolume { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        musicVolume = PlayerPrefs.GetFloat("Music");
        sfxVolume = PlayerPrefs.GetFloat("SFX");
    }

    // Update is called once per frame
    void Update()
    {
        if (musicVolume != PlayerPrefs.GetFloat("Music")) musicVolume = PlayerPrefs.GetFloat("Music");
        if (sfxVolume != PlayerPrefs.GetFloat("SFX"))
        {
            sfxVolume = PlayerPrefs.GetFloat("SFX");
            backgroundSFX.volume = sfxVolume;
        }
    }
}
