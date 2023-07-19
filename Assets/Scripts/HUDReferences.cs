using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDReferences : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider lightSlider;
    [SerializeField] private Slider grappleSlider;
    [SerializeField] private Slider healingSlider;
    [SerializeField] private Image flashOverlay;
    [SerializeField] private GameObject healText;

    [Header("Stamina Bar")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Image staminaFill;
    [SerializeField] private Color staminaActive;
    [SerializeField] private Color staminaInactive;

    [Header("Hot Bar")]
    [SerializeField] private GameObject hotBarObject;
    [SerializeField] private GameObject[] hotbarImages;
    [SerializeField] private Image[] hotbarImagesImage;
    [SerializeField] private GameObject highlight;
    private float highlightPosition = 98f;
    private int currentPosition = 1;

    [Header("Progress Stuff")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI helpText;

    [Header("Mr Beast Ability Icons")]
    [Header("Karl Ability")]
    [SerializeField] private GameObject karlIcon;
    [SerializeField] private Slider karlLoadingSlider;
    [SerializeField] private Color karlReadyColour;
    [SerializeField] private Color karlNotReadyColour;

    // Get Functions

    // Health
    public Slider GetHealthSlider() { return healthSlider; }
    public GameObject GetHealText() { return healText; }
    public Slider GetHealingSlider() { return healingSlider; }

    // Stamina
    public Slider GetStaminaSlider() { return staminaSlider; }
    public Color GetStaminaActive() { return staminaActive; }
    public Color GetStaminaInactive() { return staminaInactive; }
    public Image GetStaminaFill() { return staminaFill; }

    // Light Slider
    public Slider GetLightSlider() { return lightSlider; }

    // Blind Overlay
    public Image GetFlashOverlay() { return flashOverlay; }

    // Hot Bar Stuff
    public GameObject GetHotBarObject() { return hotBarObject; }
    public Image[] GetHotbarImages() { return hotbarImagesImage; }

    // Task Stuff
    public Slider GetProgressBar() { return progressBar; }
    public TextMeshProUGUI GetHelpText() { return helpText; }

    // Grapple
    public Slider GetGrappleSlider() { return grappleSlider; }

    // Mr Beast Abilities
    // Karl jacobs Icon
    public GameObject GetKarlIcon() { return karlIcon; }


    // Set Functions
    public void SetHotBar(bool active) { hotBarObject.SetActive(active); }
    public void SetHightlightPosition(int position)
    {
        if (position == 1)
        {
            if (currentPosition == 3)
            {
                currentPosition = 1;
                highlight.transform.localPosition = new(-98f, 0, 0);
            }else
            {
                currentPosition += 1;
                highlight.transform.localPosition = new(highlight.transform.localPosition.x + highlightPosition, 0, 0);
            }
        } 
        else if (position == 0)
        {
            if (currentPosition == 1)
            {
                currentPosition = 3;
                highlight.transform.localPosition = new(98f, 0, 0);
            }
            else
            {
                currentPosition -= 1;
                highlight.transform.localPosition = new(highlight.transform.localPosition.x - highlightPosition, 0, 0);
            }
        }
    }
    public void SetHotBarImage(int image, Sprite sprite)
    {
        hotbarImagesImage[image].sprite= sprite;
    }
    private void UpdateHotBar()
    {
        if (!hotBarObject.activeSelf) return;

        // Image 1
        if (hotbarImages[0].activeSelf && hotbarImagesImage[0].sprite == null)
        {
            hotbarImages[0].SetActive(false);
        } 
        else if (!hotbarImages[0].activeSelf && hotbarImagesImage[0].sprite != null)
        {
            hotbarImages[0].SetActive(true);
        }
        
        // Image 2
        if (hotbarImages[1].activeSelf && hotbarImagesImage[1].sprite == null)
        {
            hotbarImages[1].SetActive(false);
        } 
        else if (!hotbarImages[1].activeSelf && hotbarImagesImage[1].sprite != null)
        {
            hotbarImages[1].SetActive(true);
        }
        
        // Image 3
        if (hotbarImages[2].activeSelf && hotbarImagesImage[2].sprite == null)
        {
            hotbarImages[2].SetActive(false);
        } 
        else if (!hotbarImages[2].activeSelf && hotbarImagesImage[2].sprite != null)
        {
            hotbarImages[2].SetActive(true);
        }
    }

    // Karl Icon Set Functions
    public void SetKarlNotReady()
    {
        karlIcon.GetComponent<Image>().color = karlNotReadyColour;
    }
    public void SetKarlReady()
    {
        karlIcon.GetComponent<Image>().color = karlReadyColour;
    }
    public void InitKarlLoadingSlider(float min, float max, float value)
    {
        karlLoadingSlider.value = value;
        karlLoadingSlider.minValue = min;
        karlLoadingSlider.maxValue = max;
    }
    public void SetKarlSlider(float value)
    {
        karlLoadingSlider.value = value;
    }

    private void Update()
    {
        UpdateHotBar();
    }
}
