using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [System.Serializable]
    public class CamSetup
    {
        public string name;                   // Identificador ("Idle", "Spin", "Win", "Jackpot")
        public CinemachineCamera cam;  // Referencia a la cámara
    }

    [Header("Cámaras configuradas")]
    public List<CamSetup> cameras;

    [Header("Prioridades")]
    public int activePriority = 20;
    public int inactivePriority = 5;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        // Iniciar todas en inactivo
        foreach (var c in cameras)
        {
            if (c.cam != null) c.cam.Priority = inactivePriority;
        }

        // Cámara Idle por defecto
        SetCamera("Idle");
    }

    [Button]
    /// <summary>
    /// Cambia la cámara activa por nombre
    /// </summary>
    public void SetCamera(string name)
    {
        foreach (var c in cameras)
        {
            if (c.cam == null) continue;

            if (c.name == name)
                c.cam.Priority = activePriority;
            else
                c.cam.Priority = inactivePriority;
        }
    }

    /// <summary>
    /// Devuelve la cámara actual activa (por si quieres saberlo)
    /// </summary>
    public CinemachineCamera GetActiveCamera()
    {
        foreach (var c in cameras)
        {
            if (c.cam != null && c.cam.Priority == activePriority)
                return c.cam;
        }
        return null;
    }
    [SerializeField] CinemachineImpulseSource cinemachineImpulseSource;
    public void SetImpulseCamera(float multiplayer)
    {
        cinemachineImpulseSource.GenerateImpulse(multiplayer * .15f);
        Debug.LogFormat($" impulso camara : {multiplayer * .15f}");
    }
}
