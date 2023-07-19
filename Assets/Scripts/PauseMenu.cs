using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject settingMenu;

    [Header("Pause Menu Buttons")]
    [SerializeField] Button resume;
    [SerializeField] Button settings;
    [SerializeField] Button quit;

    [Header("Settings Menu Buttons")]
    [SerializeField] Button backButton;
    [SerializeField] Slider musicVolume;
    [SerializeField] Slider sfxVolume;

    [Header ("Settings Sensitivity Sliders")]
    [SerializeField] Slider rX;
    [SerializeField] Slider rY;
    [SerializeField] Slider zX;
    [SerializeField] Slider zY;

    private bool paused = false;

    void Start()
    {
        pauseMenu.SetActive(false);
        settingMenu.SetActive(false);

        PauseMenuInit();
        SettingsMenuInit();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !paused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            pauseMenu.SetActive(true);
            paused = true;
        }
    }

    public bool IsPaused()
    {
        return paused;
    }

    private void PauseMenuInit()
    {
        resume.onClick.AddListener(() =>
        {
            pauseMenu.SetActive(false);
            paused = false;
        });

        settings.onClick.AddListener(() => {
            settingMenu.SetActive(true);
            pauseMenu.SetActive(false);
        });

        quit.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton);
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        });
    }

    private void SettingsMenuInit()
    {
        backButton.onClick.AddListener(() =>
        {
            settingMenu.SetActive(false);
            pauseMenu.SetActive(true);
        });

        musicVolume.value = PlayerPrefs.GetFloat("Music");
        sfxVolume.value = PlayerPrefs.GetFloat("SFX");

        rX.value = PlayerPrefs.GetFloat("r_sensitivityX") / 100;
        rY.value = PlayerPrefs.GetFloat("r_sensitivityY");
        zX.value = PlayerPrefs.GetFloat("z_sensitivityX") / 100;
        zY.value = PlayerPrefs.GetFloat("z_sensitivityY");

        musicVolume.onValueChanged.AddListener(delegate {
            OnValueChange("Music", musicVolume.value);
        });

        sfxVolume.onValueChanged.AddListener(delegate {
            OnValueChange("SFX", sfxVolume.value);
        });
        
        rX.onValueChanged.AddListener(delegate {
            OnValueChange("r_sensitivityX", rX.value * 100);
        });
        
        rY.onValueChanged.AddListener(delegate {
            OnValueChange("r_sensitivityY", rY.value);
        });

        zX.onValueChanged.AddListener(delegate {
            OnValueChange("z_sensitivityX", zX.value * 100);
        });

        zY.onValueChanged.AddListener(delegate {
            OnValueChange("z_sensitivityY", zY.value);
        });
    }

    private void OnValueChange(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }
}
