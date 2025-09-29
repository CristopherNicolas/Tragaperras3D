using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class PanelApuesta : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public static PanelApuesta instance;
    public TMP_Text statsText, contadorIncremento;
    public TMP_Text apuestaText; //para mostrar los puntos ganados al terminar.
    public TMP_Text apuestaTextARealizar; //para mostrar los puntos antes de tirar.

    public void UpdateUI()
    {
        string stats = MachineController.instance.GetStats();
        statsText.text = stats;
    }
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public int apuestaActual;
    public void AumentarApuesta(int incremento)
    {
        if (incremento > MachineController.instance.puntos) return;


        Debug.Log($"Apuesta actual: {apuestaActual} - Incremento: {incremento}");
        apuestaActual += incremento;
        if (apuestaActual < 0) apuestaActual = 0;
        apuestaTextARealizar.text = $"Apuesta: {apuestaActual}$";
        apuestaTextARealizar.transform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 4, 0.8f)
        .OnComplete(() => apuestaTextARealizar.transform.localScale = Vector3.one);
        UpdateUI();
        //despues al realizar el spin se restara de los puntos del jugador.
    }


    public void MostrarEsconderPanel(bool mostrar)
    {
        UpdateUI();
        canvasGroup.DOFade(mostrar ? 1 : 0, .35f);
        canvasGroup.interactable = mostrar;
        canvasGroup.blocksRaycasts = mostrar;


    }
     /// <summary>
    /// Anima el texto para contar hasta el nuevo valor de puntos con corutina.
    /// </summary>
    /// <param name="puntos">El valor final al que debe llegar el contador.</param>
    /// <param name="valorActual">El valor actual antes de la animación.</param>
    /// <param name="duracion">Duración de la animación en segundos.</param>
    public void AnimarPuntos(int valorInicial, int valorFinal, float duracion = 1.2f)
    {
        if (contadorIncremento == null)
        {
            Debug.LogWarning("ContadorIncremento no está asignado.");
            return;
        }

        // Cancelar cualquier animación previa
        StopAllCoroutines();

        // Comienza la corutina de animación
        StartCoroutine(IncrementarPuntosCoroutine(valorInicial, valorFinal, duracion));
    }

    /// <summary>
    /// Corutina para incrementar (o decrementar) los puntos de forma gradual.
    /// </summary>
    /// <param name="valorInicial">Valor inicial</param>
    /// <param name="valorFinal">Valor final al que se quiere llegar</param>
    /// <param name="duracion">Duración de la animación</param>
    private IEnumerator IncrementarPuntosCoroutine(int valorInicial, int valorFinal, float duracion)
    {
        float tiempoTranscurrido = 0f;
        int valorActual = valorInicial;

        // Asegúrate de que el texto esté activo
        contadorIncremento.gameObject.SetActive(true);

        while (tiempoTranscurrido < duracion)
        {
            // Incremento o decremento gradual
            tiempoTranscurrido += Time.deltaTime;

            // Use Mathf.Lerp para suavizar el cambio de valor
            valorActual = Mathf.RoundToInt(Mathf.Lerp(valorInicial, valorFinal, tiempoTranscurrido / duracion));

            // Actualizar el texto en cada iteración
            contadorIncremento.text = valorActual.ToString("N0");

            // Esperar el siguiente frame
            yield return null;
        }

        // Asegurarse de que el valor final sea exacto al terminar
        contadorIncremento.text = valorFinal.ToString("N0");

        // Efecto visual de "punch" al final de la animación
        contadorIncremento.transform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 4, 0.8f);
    }


}
