using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class SlotEvaluator : MonoBehaviour
{
    public CameraController cameraController;
    public PiezaEnEscena[,] piezasCreadas;
    public List<Pieza> piezasPrefab;
    public float destroyAnimDuration = 0.45f;
    public float fallDuration = 0.25f;
    public float punchScale = 1.3f;
    public float punchRotation = 30f;

    public float firstPieceY = -11.9f;
    public float offsetY = 1.5f;
    public int totalPoints { get; private set; }

    public void Evaluate()
    {
        StartCoroutine(EvaluateRoutine());
    }

    private IEnumerator EvaluateRoutine()
    {
        bool anyMatch = true;
        int cols = piezasCreadas.GetLength(0);
        int rows = piezasCreadas.GetLength(1);

        while (anyMatch)
        {
            anyMatch = false;
            List<PiezaEnEscena> toDestroy = new List<PiezaEnEscena>();

            // --- Evaluar columnas ---
            for (int x = 0; x < cols; x++)
            {
                int count = 1;
                for (int y = 1; y < rows; y++)
                {
                    if (piezasCreadas[x, y] != null && piezasCreadas[x, y - 1] != null &&
                        piezasCreadas[x, y].pieza.nombre == piezasCreadas[x, y - 1].pieza.nombre)
                    {
                        count++;
                        if (y == rows - 1 && count >= 3)
                        {
                            for (int k = y - count + 1; k <= y; k++)
                                toDestroy.Add(piezasCreadas[x, k]);
                            anyMatch = true;
                        }
                    }
                    else
                    {
                        if (count >= 3)
                        {
                            for (int k = y - count; k < y; k++)
                                toDestroy.Add(piezasCreadas[x, k]);
                            anyMatch = true;
                        }
                        count = 1;
                    }
                }
            }

            // --- Evaluar filas ---
            for (int y = 0; y < rows; y++)
            {
                int count = 1;
                for (int x = 1; x < cols; x++)
                {
                    if (piezasCreadas[x, y] != null && piezasCreadas[x - 1, y] != null &&
                        piezasCreadas[x, y].pieza.nombre == piezasCreadas[x - 1, y].pieza.nombre)
                    {
                        count++;
                        if (x == cols - 1 && count >= 3)
                        {
                            for (int k = x - count + 1; k <= x; k++)
                                if (!toDestroy.Contains(piezasCreadas[k, y]))
                                    toDestroy.Add(piezasCreadas[k, y]);
                            anyMatch = true;
                        }
                    }
                    else
                    {
                        if (count >= 3)
                        {
                            for (int k = x - count; k < x; k++)
                                if (!toDestroy.Contains(piezasCreadas[k, y]))
                                    toDestroy.Add(piezasCreadas[k, y]);
                            anyMatch = true;
                        }
                        count = 1;
                    }
                }
            }

            // --- Evaluar diagonales ---
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (piezasCreadas[x, y] == null) continue;

                    // ↘ diagonal (derecha-abajo)
                    List<PiezaEnEscena> diag1 = new List<PiezaEnEscena>();
                    int dx = x, dy = y;
                    string nombre = piezasCreadas[x, y].pieza.nombre;
                    while (dx < cols && dy < rows && piezasCreadas[dx, dy] != null &&
                           piezasCreadas[dx, dy].pieza.nombre == nombre)
                    {
                        diag1.Add(piezasCreadas[dx, dy]);
                        dx++;
                        dy++;
                    }
                    if (diag1.Count >= 3)
                    {
                        foreach (var p in diag1)
                            if (!toDestroy.Contains(p)) toDestroy.Add(p);
                        anyMatch = true;
                    }

                    // ↙ diagonal (izquierda-abajo)
                    List<PiezaEnEscena> diag2 = new List<PiezaEnEscena>();
                    dx = x; dy = y;
                    while (dx >= 0 && dy < rows && piezasCreadas[dx, dy] != null &&
                           piezasCreadas[dx, dy].pieza.nombre == nombre)
                    {
                        diag2.Add(piezasCreadas[dx, dy]);
                        dx--;
                        dy++;
                    }
                    if (diag2.Count >= 3)
                    {
                        foreach (var p in diag2)
                            if (!toDestroy.Contains(p)) toDestroy.Add(p);
                        anyMatch = true;
                    }
                }
            }

            // --- Animar destrucción ---
            foreach (var pieza in toDestroy)
            {
                if (pieza == null) continue;
                totalPoints += Mathf.RoundToInt(pieza.pieza.recompensa);

                float animacionDuracion = pieza.Animacion();
                pieza.transform.DOPunchScale(Vector3.one * punchScale, animacionDuracion, 1, 0.5f)
                    .OnComplete(() => Destroy(pieza.gameObject));


                // Limpiar referencia
                for (int x = 0; x < cols; x++)
                    for (int y = 0; y < rows; y++)
                        if (piezasCreadas[x, y] == pieza)
                            piezasCreadas[x, y] = null;
            }

            yield return new WaitForSeconds(destroyAnimDuration);
            cameraController.SetImpulseCamera(toDestroy.Count);

            // --- Hacer caer piezas existentes y generar nuevas ---
            for (int x = 0; x < cols; x++)
            {
                Transform parentRiel = null;

                // Encontrar el riel de referencia (primer child no nulo)
                for (int y = 0; y < rows; y++)
                    if (piezasCreadas[x, y] != null)
                    {
                        parentRiel = piezasCreadas[x, y].transform.parent;
                        break;
                    }

                if (parentRiel == null && rows > 0)
                    parentRiel = transform; // fallback

                for (int y = 0; y < rows; y++)
                {
                    if (piezasCreadas[x, y] == null)
                    {
                        int fallFrom = y + 1;
                        while (fallFrom < rows && piezasCreadas[x, fallFrom] == null) fallFrom++;

                        if (fallFrom < rows && piezasCreadas[x, fallFrom] != null)
                        {
                            // Hacer caer pieza existente
                            piezasCreadas[x, y] = piezasCreadas[x, fallFrom];
                            piezasCreadas[x, fallFrom] = null;

                            Vector3 targetPos = piezasCreadas[x, y].transform.localPosition;
                            targetPos.y = firstPieceY + y * offsetY;
                            piezasCreadas[x, y].transform.DOLocalMoveY(targetPos.y, fallDuration)
                                .SetEase(Ease.OutBounce);
                        }
                        else
                        {
                            // Crear pieza nueva
                            Pieza randomPieza = piezasPrefab[Random.Range(0, piezasPrefab.Count)];

                            GameObject obj = Instantiate(randomPieza.prefab,
                                Vector3.zero,
                                Quaternion.Euler(randomPieza.startRotation),
                                parentRiel);

                            PiezaEnEscena nueva = obj.GetComponent<PiezaEnEscena>();
                            nueva.pieza = randomPieza;
                            piezasCreadas[x, y] = nueva;

                            // Posición inicial (arriba del riel)
                            Vector3 spawnPos = new Vector3(0, firstPieceY + rows * offsetY, 0);
                            nueva.transform.localPosition = spawnPos;

                            // Posición final
                            Vector3 targetPos = new Vector3(0, firstPieceY + y * offsetY, 0);
                            nueva.transform.DOLocalMoveY(targetPos.y, fallDuration).SetEase(Ease.OutBounce);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(fallDuration + 0.05f);
        }

        cameraController.SetCamera("idle");
        Debug.Log("Evaluación completa! Puntos obtenidos: " + totalPoints);
        MachineController.instance.puntos += totalPoints;
        MachineController.instance.DestruirPiezas();
    }
}
