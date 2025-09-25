using DG.Tweening;
using TMPro;
using UnityEngine;

public class PanelApuesta : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public static PanelApuesta instance;
    public TMP_Text statsText;

    public void UpdateUI()
    {
        string stats = MachineController.instance.GetStats();
        statsText.text = stats;
    }
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void MostrarEsconderPanel(bool mostrar)
    {
        UpdateUI();
        canvasGroup.DOFade(mostrar ? 1 : 0, .35f);
        canvasGroup.interactable = mostrar;
        canvasGroup.blocksRaycasts = mostrar;

    }
}
