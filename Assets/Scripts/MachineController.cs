using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class MachineController : MonoBehaviour
{
    //stats para usar la maquina
    [FoldoutGroup("Stats jugador"), SerializeField] int tiradasDisponibles = 3;
    [FoldoutGroup("Stats jugador")] public int puntos = 0;
    [FoldoutGroup("Stats jugador"), SerializeField] int vecesQueSeHaTirado = 0;

    public static MachineController instance;
    public List<RielAnimator> rieles, rielesDisponibles; //rieles seleccionados para juego - disponibles
    [TableList] public List<Pieza> piezas;
    public int cantidadDePiezasPorRiel = 7;
    public float offsetY = 1.5f;
    public PiezaEnEscena[,] piezasCreadas;
    public Button throwButton;
    public SlotEvaluator evaluator;
    public CameraController cameraController;
    private int rielesEnMovimiento;
    public int cantidadDeRielesSeleccionados;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    IEnumerator Start()
    {
        //Deberia de modificarse esto para que cambie con respecto a los botones presionados
        // como todo el sistema de la maquina trabaja con la lista rieles, tener vacia y al presionar el 
        //boton del riel agregar a la lista
        yield return new WaitUntil(() => !throwButton.interactable); // podria dar fallos
       
    }

    [Button("Girar Reels")]
    public void SpinReels()
    {
        if (rielesEnMovimiento > 0) return;
        
        rieles = rielesDisponibles.Take(cantidadDeRielesSeleccionados).ToList();
        Debug.Log("Rieles a girar: " + rieles.Count);   

            piezasCreadas = new PiezaEnEscena[rieles.Count, cantidadDePiezasPorRiel];
            GenerarPiezas();

        throwButton.interactable = false;
        throwButton.transform.DOScale(Vector3.zero, .5f);  
        cameraController.SetCamera("spin");
        tiradasDisponibles--;   
        vecesQueSeHaTirado++;
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
                    pos, Quaternion.Euler(randomTarget.startRotation), rielT);

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

    internal void DestruirPiezas()
    {
        int totalPorDestruir = piezasCreadas.Length;
        int destruidas = 0;

        for (int i = 0; i < piezasCreadas.GetLength(0); i++)
        {
            for (int j = 0; j < piezasCreadas.GetLength(1); j++)
            {
                PiezaEnEscena pieza = piezasCreadas[i, j];
                if (pieza == null)
                {
                    totalPorDestruir--;
                    continue;
                }

                pieza.transform.DOPunchRotation(Vector3.left * 25, 1);
                pieza.transform.DOScale(Vector3.zero, 1.35f)
                    .OnComplete(() =>
                    {
                        Destroy(pieza.gameObject);  
                        destruidas++;

                        if (destruidas >= totalPorDestruir)
                        {
                            // Ahora sí regeneramos
                            piezasCreadas = new PiezaEnEscena[rieles.Count, cantidadDePiezasPorRiel];
                            //GenerarPiezas();
                        }
                    });
            }
        }
        throwButton.transform.DOScale(Vector3.one, .5f).SetDelay(1.45f).OnComplete(()=> throwButton.interactable = true);
    }


}

[System.Serializable]
public class Pieza
{
    public Vector3 startRotation;
    public string nombre;
    public float recompensa;
    public GameObject prefab;
}
