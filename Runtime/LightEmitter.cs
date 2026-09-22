using System.Collections.Generic;
using UnityEngine;
namespace ProceduralLight
{
    [ExecuteAlways,DisallowMultipleComponent]
    public sealed class LightEmitter : MonoBehaviour
    {
        internal static readonly HashSet<LightEmitter> Active=new HashSet<LightEmitter>();
        [ColorUsage(false,true)] public Color color=new Color(.3f,1,.7f);
        [Min(0)] public float intensity=4;
        [Min(.01f)] public float radius=.4f;
        [Min(.1f)] public float range=10;
        void OnEnable()=>Active.Add(this);
        void OnDisable()=>Active.Remove(this);
        public float Attenuation(float distance){float fade=Mathf.Clamp01(1-distance/Mathf.Max(.1f,range));return intensity*radius*radius/(radius*radius+distance*distance)*fade*fade;}
    }
}
