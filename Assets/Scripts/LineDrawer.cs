using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LineDrawer : MonoBehaviour
{
    [SerializeField] private GameObject linePrefab; // Prefab con LineRenderer
    private List<LineRenderer> activeLines = new List<LineRenderer>();

    /// <summary>
    /// Dibuja una línea conectando todas las piezas en orden.
    /// </summary>
    public void DrawLine(List<PiezaEnEscena> piezas)
    {
        if (piezas == null || piezas.Count < 2) return;

        GameObject lineObj = Instantiate(linePrefab, transform);
        LineRenderer lr = lineObj.GetComponent<LineRenderer>();

        // Configuración inicial
        lr.positionCount = piezas.Count;
        for (int i = 0; i < piezas.Count; i++)
        {
            Vector3 targetPos  = new()
            {
                x = piezas[i].transform.position.x,
                y = piezas[i].transform.position.y,  // Ajuste vertical para que la línea no toque la pieza
                z = piezas[i].transform.position.z + .45f
            };
            lr.SetPosition(i,targetPos);
        }

        // Opcional: animar aparición (alpha 0 → 1)
        if (lr.material != null)
        {
            Color c = lr.material.color;
            c.a = 0f;
            lr.material.color = c;
            lr.material.DOFade(1f, 0.25f);
        }

        activeLines.Add(lr);
    }

    /// <summary>
    /// Limpia todas las líneas activas.
    /// </summary>
    public void ClearLines()
    {
        foreach (var line in activeLines)
        {
            if (line != null)
            {
                // Animar desaparición antes de destruir
                if (line.material != null)
                {
                    line.material.DOFade(0f, 0.25f)
                        .OnComplete(() => Destroy(line.gameObject));
                }
                else
                {
                    Destroy(line.gameObject);
                }
            }
        }
        activeLines.Clear();
    }
}
