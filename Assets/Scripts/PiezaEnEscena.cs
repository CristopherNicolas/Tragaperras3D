using DG.Tweening;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class PiezaEnEscena : MonoBehaviour
{
    public AudioSource source;
    public Pieza pieza;
    //retorna el tiempo de animacion, para imlementar una animacion custom por cada pieza
    //el valor de retorno no debe superrar . los 0.45f
    public virtual float Animacion()
    {
        source.Play();
        transform.DOPunchRotation(Vector3.forward * 180, 0.45f, 1, 0.5f);
        return 1.25f;
    }    
}