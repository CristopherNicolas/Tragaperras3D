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
        //source.Play();
        GameObject audio = new GameObject();
        AudioSource s =  audio.AddComponent<AudioSource>();
        s.clip = source.clip;   
        s.volume = source.volume;   
        s.Play();
        Destroy(audio, source.clip.length); 


        transform.DOPunchRotation(Vector3.forward * 180, 0.45f, 1, 0.5f);
        return 1.25f;
    }    
}