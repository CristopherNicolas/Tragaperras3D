using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using TMPro;

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
    public GameObject btnsWorldContainer;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        PanelApuesta.instance.MostrarEsconderPanel(true);//mostrar
    }
    public string GetStats()=> $"Tiradas: {tiradasDisponibles} - Puntos: {puntos} - Veces tirado: {vecesQueSeHaTirado}";


    [Button("Girar Reels")]
    public void SpinReels()
    {
        if (rielesEnMovimiento > 0 || tiradasDisponibles<=0 || cantidadDeRielesSeleccionados ==0 ) return;

        //desactivar texto btns
        int bCC = btnsWorldContainer.transform.childCount;
        for (int i = 0; i < bCC; i++)
        {
            btnsWorldContainer.transform.GetChild(i).transform
            .GetComponentInChildren<TMP_Text>()
            .DOFade(0, .75f);
        }
        PanelApuesta.instance.MostrarEsconderPanel(false);//esconder

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
        throwButton.transform.DOScale(Vector3.one, .5f).SetDelay(1.45f).OnComplete(() => throwButton.interactable = true);
        //desactivar texto btns
        int bCC = btnsWorldContainer.transform.childCount;
        for (int i = 0; i < bCC; i++)
        {
            btnsWorldContainer.transform.GetChild(i).transform
            .GetComponentInChildren<TMP_Text>()
            .DOFade(1, .75f);
        }
        PanelApuesta.instance.MostrarEsconderPanel(true);//mostrar
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
