using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] GameObject camHolder;
    private CinemachineFreeLook cam;
    //[SerializeField] float sensitivityX;
    //[SerializeField] float sensitivityY;

    [Header("Regular Settings")]
    [SerializeField] float r_topRadius;
    [SerializeField] float r_midRadius;
    [SerializeField] float r_botRadius;
    private float r_xSensitivity;
    private float r_ySensitivity;

    [Header("Zoom Settings")]
    [SerializeField] float z_topRadius;
    [SerializeField] float z_midRadius;
    [SerializeField] float z_botRadius;
    private float z_xSensitivity;
    private float z_ySensitivity;

    private GameVariables gameVars;
    private PauseMenu paused;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CinemachineFreeLook>();
        cam.Follow = camHolder.transform;
        cam.LookAt = camHolder.transform;

        gameVars = FindObjectOfType<GameVariables>();
        paused = FindObjectOfType<PauseMenu>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!IsOwner) return;

        CameraZoom();
        LockMouse();
        Sensitivity();
    }

    private void CameraZoom()
    {
        if (Input.GetMouseButton(1) && !TryGetComponent(out MrBeastPlayer _))
        {
            if (cam.m_YAxis.m_MaxSpeed != z_ySensitivity) cam.m_YAxis.m_MaxSpeed = z_ySensitivity;
            if (cam.m_XAxis.m_MaxSpeed != z_xSensitivity) cam.m_XAxis.m_MaxSpeed = z_xSensitivity;

            cam.m_Orbits[0].m_Radius = Mathf.Lerp(cam.m_Orbits[0].m_Radius, z_topRadius, Time.deltaTime * 6);
            cam.m_Orbits[1].m_Radius = Mathf.Lerp(cam.m_Orbits[1].m_Radius, z_midRadius, Time.deltaTime * 6);
            cam.m_Orbits[2].m_Radius = Mathf.Lerp(cam.m_Orbits[2].m_Radius, z_botRadius, Time.deltaTime * 6);
        } 
        else if (cam.m_Orbits[0].m_Radius != r_topRadius || cam.m_Orbits[1].m_Radius != r_midRadius || cam.m_Orbits[2].m_Radius != r_botRadius)
        {
            if (cam.m_YAxis.m_MaxSpeed != r_ySensitivity) cam.m_YAxis.m_MaxSpeed = r_ySensitivity;
            if (cam.m_XAxis.m_MaxSpeed != r_xSensitivity) cam.m_XAxis.m_MaxSpeed = r_xSensitivity;

            cam.m_Orbits[0].m_Radius = Mathf.Lerp(cam.m_Orbits[0].m_Radius, r_topRadius, Time.deltaTime * 6);
            cam.m_Orbits[1].m_Radius = Mathf.Lerp(cam.m_Orbits[1].m_Radius, r_midRadius, Time.deltaTime * 6);
            cam.m_Orbits[2].m_Radius = Mathf.Lerp(cam.m_Orbits[2].m_Radius, r_botRadius, Time.deltaTime * 6);
        }
    }

    private void LockMouse()
    {
        if (gameVars.IsGameOver())
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (paused.IsPaused())
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void Sensitivity()
    {
        if (PlayerPrefs.GetFloat("r_sensitivityX") != r_xSensitivity)
        {
            r_xSensitivity = PlayerPrefs.GetFloat("r_sensitivityX");
        }
        
        if (PlayerPrefs.GetFloat("r_sensitivityY") != r_ySensitivity)
        {
            r_ySensitivity = PlayerPrefs.GetFloat("r_sensitivityY");
        }
        
        if (PlayerPrefs.GetFloat("z_sensitivityX") != z_xSensitivity)
        {
            z_xSensitivity = PlayerPrefs.GetFloat("z_sensitivityX");
        }
        
        if (PlayerPrefs.GetFloat("z_sensitivityY") != z_ySensitivity)
        {
            z_ySensitivity = PlayerPrefs.GetFloat("z_sensitivityY");
        }
    }
}
