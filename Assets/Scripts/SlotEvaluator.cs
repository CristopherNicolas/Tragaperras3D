using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class SlotEvaluator : MonoBehaviour
{
    public LineDrawer lineDrawer;
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

            // 🔹 limpiar líneas anteriores
            lineDrawer.ClearLines();

            // --- Evaluar columnas ---
            for (int x = 0; x < cols; x++)
            {
                int count = 1;
                List<PiezaEnEscena> currentMatch = new List<PiezaEnEscena>();
                currentMatch.Add(piezasCreadas[x, 0]);

                for (int y = 1; y < rows; y++)
                {
                    if (piezasCreadas[x, y] != null && piezasCreadas[x, y - 1] != null &&
                        piezasCreadas[x, y].pieza.nombre == piezasCreadas[x, y - 1].pieza.nombre)
                    {
                        count++;
                        currentMatch.Add(piezasCreadas[x, y]);

                        if (y == rows - 1 && count >= 3)
                        {
                            toDestroy.AddRange(currentMatch);
                            lineDrawer.DrawLine(new List<PiezaEnEscena>(currentMatch));
                            anyMatch = true;
                        }
                    }
                    else
                    {
                        if (count >= 3)
                        {
                            toDestroy.AddRange(currentMatch);
                            lineDrawer.DrawLine(new List<PiezaEnEscena>(currentMatch));
                            anyMatch = true;
                        }
                        count = 1;
                        currentMatch.Clear();
                        currentMatch.Add(piezasCreadas[x, y]);
                    }
                }
            }

            // --- Evaluar filas ---
            for (int y = 0; y < rows; y++)
            {
                int count = 1;
                List<PiezaEnEscena> currentMatch = new List<PiezaEnEscena>();
                currentMatch.Add(piezasCreadas[0, y]);

                for (int x = 1; x < cols; x++)
                {
                    if (piezasCreadas[x, y] != null && piezasCreadas[x - 1, y] != null &&
                        piezasCreadas[x, y].pieza.nombre == piezasCreadas[x - 1, y].pieza.nombre)
                    {
                        count++;
                        currentMatch.Add(piezasCreadas[x, y]);

                        if (x == cols - 1 && count >= 3)
                        {
                            foreach (var p in currentMatch)
                                if (!toDestroy.Contains(p)) toDestroy.Add(p);

                            lineDrawer.DrawLine(new List<PiezaEnEscena>(currentMatch));
                            anyMatch = true;
                        }
                    }
                    else
                    {
                        if (count >= 3)
                        {
                            foreach (var p in currentMatch)
                                if (!toDestroy.Contains(p)) toDestroy.Add(p);

                            lineDrawer.DrawLine(new List<PiezaEnEscena>(currentMatch));
                            anyMatch = true;
                        }
                        count = 1;
                        currentMatch.Clear();
                        currentMatch.Add(piezasCreadas[x, y]);
                    }
                }
            }

            // --- Evaluar diagonales ---
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (piezasCreadas[x, y] == null) continue;

                    string nombre = piezasCreadas[x, y].pieza.nombre;

                    // ↘ diagonal (derecha-abajo)
                    List<PiezaEnEscena> diag1 = new List<PiezaEnEscena>();
                    int dx = x, dy = y;
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

                        lineDrawer.DrawLine(diag1);
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

                        lineDrawer.DrawLine(diag2);
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

            // 🔹 limpiar líneas después de destruir
            lineDrawer.ClearLines();

            // --- Hacer caer piezas existentes y generar nuevas ---
for (int x = 0; x < cols; x++)
{
    Transform parentRiel = MachineController.instance.rieles[x].transform; // siempre el riel correcto

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
                    parentRiel); // 👈 padre correcto

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
        int valorInicial = MachineController.instance.puntos, valorFinal = MachineController.instance.puntos + totalPoints;
        MachineController.instance.puntos += totalPoints;
        PanelApuesta.instance.AnimarPuntos(valorInicial, valorFinal, 5);
        totalPoints = 0;
        MachineController.instance.DestruirPiezas();
    }
}
