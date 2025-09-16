using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class MachineController : MonoBehaviour
{
    public List<RielAnimator> rieles; 
    [TableList] public List<Pieza> piezas;
    public int cantidadDePiezasPorRiel = 7;
    public float offsetY = 1.5f;
    public PiezaEnEscena[,] piezasCreadas;

    public SlotEvaluator evaluator;

    private int rielesEnMovimiento;

    void Awake()
    {
        piezasCreadas = new PiezaEnEscena[rieles.Count, cantidadDePiezasPorRiel];
        GenerarPiezas();
    }

    [Button("Girar Reels")]
    public void SpinReels()
    {
        rielesEnMovimiento = rieles.Count;
    

        for (int i = 0; i < rieles.Count; i++)
        {
            int rielIndex = i; // Captura de variable para callbacks
            rieles[i].OnReelStop = () =>
            {
                rielesEnMovimiento--;
                if (rielesEnMovimiento <= 0)
                {
                    // Todos los rieles terminaron → evaluar
                    EvaluateReels();
                }
            };

            rieles[i].MovePiecesToTargetY(-11.9f, offsetY);
        }
    }

    [Button]
    void GenerarPiezas()
    {
        for (int i = 0; i < rieles.Count; i++)
        {
            Transform rielT = rieles[i].transform;
            for (int j = 0; j < cantidadDePiezasPorRiel; j++)
            {
                Vector3 pos = new Vector3(
                    rielT.position.x,
                    rielT.position.y + (j * offsetY),
                    rielT.position.z
                );

                Pieza randomTarget = piezas[Random.Range(0, piezas.Count)];
                GameObject obj = Instantiate(randomTarget.prefab,
                    pos, Quaternion.Euler(90, 0, 0), rielT);

                PiezaEnEscena piezaEnEscena = obj.GetComponent<PiezaEnEscena>();
                piezaEnEscena.pieza = randomTarget;

                piezasCreadas[i, j] = piezaEnEscena;
            }
        }
    }

    void EvaluateReels()
    {
        evaluator.offsetY = offsetY;
        evaluator.piezasPrefab = piezas;
        evaluator.piezasCreadas = piezasCreadas;
        evaluator.Evaluate();
    }
}

[System.Serializable]
public class Pieza
{
    public string nombre;
    public float recompensa;
    public GameObject prefab;
}
