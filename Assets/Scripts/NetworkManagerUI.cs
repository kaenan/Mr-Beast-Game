using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [Header("Home Screen")]
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private TMP_InputField codeInput;

    [Header("Settings Screen")]
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider sfxVolume;

    [Header("Screens")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settingsMenu;

    private void Awake()
    {
        Time.timeScale = 1;

        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);

        hostBtn.onClick.AddListener(() =>
        {
            PlayerPrefs.SetString("Load Type", "Host");
            SceneManager.LoadScene(2, LoadSceneMode.Single);
        });

        clientBtn.onClick.AddListener(() =>
        {
            PlayerPrefs.SetString("Load Type", "Client");
            PlayerPrefs.SetString("Room Code", codeInput.text.ToUpper());
            SceneManager.LoadScene(2, LoadSceneMode.Single);
        });

        settingsBtn.onClick.AddListener(() =>
        {
            mainMenu.SetActive(false);
            settingsMenu.SetActive(true);

            settingsBackButton.onClick.AddListener(() =>
            {
                mainMenu.SetActive(true);
                settingsMenu.SetActive(false);
            });
        });

        CheckPlayerPrefs();
    }

    private void Update()
    {
        if (codeInput.text.Length <= 0)
        {
            clientBtn.interactable = false;
        } else
        {
            clientBtn.interactable = true;
        }
    }

    private void CheckPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey("Music"))
        {
            PlayerPrefs.SetFloat("Music", 1);
        }
        
        if (!PlayerPrefs.HasKey("SFX"))
        {
            PlayerPrefs.SetFloat("SFX", 1);
        }

        if (!PlayerPrefs.HasKey("r_sensitivityX"))
        {
            PlayerPrefs.SetFloat("r_sensitivityX", 500);
        }
        
        if (!PlayerPrefs.HasKey("r_sensitivityY"))
        {
            PlayerPrefs.SetFloat("r_sensitivityY", 5);
        }
        
        if (!PlayerPrefs.HasKey("z_sensitivityX"))
        {
            PlayerPrefs.SetFloat("z_sensitivityX", 100);
        }
        
        if (!PlayerPrefs.HasKey("z_sensitivityY"))
        {
            PlayerPrefs.SetFloat("z_sensitivityY", 1);
        }

        musicVolume.value = PlayerPrefs.GetFloat("Music");
        sfxVolume.value = PlayerPrefs.GetFloat("SFX");

        musicVolume.onValueChanged.AddListener(delegate { 
            OnValueChange("Music", musicVolume); 
        });
        
        sfxVolume.onValueChanged.AddListener(delegate { 
            OnValueChange("SFX", sfxVolume); 
        });
    }

    private void OnValueChange(string key, Slider slider)
    {
        PlayerPrefs.SetFloat(key, slider.value);
    }
}
