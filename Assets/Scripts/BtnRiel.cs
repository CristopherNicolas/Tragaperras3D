using DG.Tweening;
using UnityEngine;

public class BtnRiel : MonoBehaviour
{
    public GameObject worldCanvas;
    public AudioSource source;
    private float startYpos;
    public int indexRiel;
    public bool btnPresionado = false, estaJugando = false;

    private BtnRiel[] allButtons; // cache de todos los botones

    void Awake()
    {
        startYpos = transform.position.y;
        // obtener todos los botones desde el padre
        allButtons = transform.parent.GetComponentsInChildren<BtnRiel>();
    }

    public void OnMouseDown()
    {
        if (estaJugando) return; // solo permitir si no se esta jugando

       if (btnPresionado)
        {
            DesactivarBtn();

            // desactivar a la derecha
            for (int i = indexRiel + 1; i < allButtons.Length; i++)
            {
                allButtons[i].DesactivarBtn();
            }

            // ahora recalculamos cuántos están activos
            int count = 0;
            foreach (var btn in allButtons)
            {
                if (btn.btnPresionado) count++;
            }
            MachineController.instance.cantidadDeRielesSeleccionados = count;
        }

        else
        {
            ActivarBtn();

            // al activar el btn, activar los botones a la izquierda
            for (int i = 0; i < indexRiel; i++)
            {
                allButtons[i].ActivarBtn();
            }
            MachineController.instance.cantidadDeRielesSeleccionados = indexRiel + 1;
        }

        // sonido btn
        if (source != null) source.Play();
    }

    public void ActivarBtn()
    {
        if (btnPresionado) return;

        btnPresionado = true;
        transform.DOMoveY(-2.811f, .85f);
        Debug.Log("Btn activado → riel " + indexRiel);
        MachineController.instance.rielesDisponibles[indexRiel].gameObject.SetActive(true);
    }

    public void DesactivarBtn()
    {
        if (!btnPresionado) return;

        btnPresionado = false;
        transform.DOMoveY(startYpos, .35f);
        Debug.Log("Btn desactivado → riel " + indexRiel);
        MachineController.instance.rielesDisponibles[indexRiel].gameObject.SetActive(false);
    }
}
