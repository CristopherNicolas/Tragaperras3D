using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

public class Corazon : PiezaEnEscena
{
    public override float Animacion()
    {
        transform.DORotate(new(transform.localRotation.x, transform.localRotation.y, transform.localRotation.z),1 );
        return 0.4f;
    }
}