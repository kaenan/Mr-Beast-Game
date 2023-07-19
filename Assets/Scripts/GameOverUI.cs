using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button mainMenuButton;

    void Start()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();

            if (NetworkManager.Singleton != null) Destroy(NetworkManager.Singleton);

            SceneManager.LoadScene(0, LoadSceneMode.Single);
        });
    }

    public GameObject GetgameOverCanvas() { return gameOverCanvas; }
    public TextMeshProUGUI GetgameOverText() { return gameOverText; }
    public Button GetmainMenuButton() { return mainMenuButton; }
}
