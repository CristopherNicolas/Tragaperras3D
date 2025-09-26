using UnityEngine;
using DG.Tweening;
using System;

/// <summary>
/// Controla la animacion de un riel (reel) en la maquina de tragaperras.
/// deberia estar desactivado y activarse antes de iniciar el giro.
/// </summary>
public class RielAnimator : MonoBehaviour
{

    public AudioSource audioSource;
    public Transform riel;
    public float spinDuration = 1f;
    public float bounceStrength = 0.3f;

    public Action OnReelStop; // Callback al terminar el movimiento

    void Awake()
    {
        riel = transform;
    }

    public void MovePiecesToTargetY(float firstPieceY, float offsetY)
    {
        if (riel == null) return;
        audioSource.Play();
        int totalPiezas = riel.childCount;
        int completed = 0;

        for (int i = 0; i < totalPiezas; i++)
        {
            Transform pieza = riel.GetChild(i);
            float targetY = firstPieceY + i * offsetY;
            int indexLocal = i; // Captura local

            pieza.DOLocalMoveY(targetY, spinDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    // Rebote al final
                    pieza.DOLocalMoveY(targetY - bounceStrength, 0.15f)
                        .SetEase(Ease.OutBounce)
                        .SetLoops(1, LoopType.Yoyo)
                        .OnComplete(() =>
                        {
                            completed++;
                            if (completed >= totalPiezas)
                                OnReelStop?.Invoke();
                        });
                });
        }
    }
}
