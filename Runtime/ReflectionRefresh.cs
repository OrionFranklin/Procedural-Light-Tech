using UnityEngine;
using UnityEngine.Rendering;
namespace ProceduralLight
{
    /// <summary>Captures offscreen geometry in a bounded, time-sliced cubemap. Not a mirror or ray-traced reflection.</summary>
    [RequireComponent(typeof(ReflectionProbe))]
    public sealed class ReflectionRefresh:MonoBehaviour
    {
        public Transform follow;
        [Min(.5f)] public float interval=2;
        [Min(1)] public float extent=50;
        public LayerMask geometry=~0;
        ReflectionProbe probe;int request=-1;float next;
        void OnEnable(){probe=GetComponent<ReflectionProbe>();probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.ViaScripting;probe.timeSlicingMode=ReflectionProbeTimeSlicingMode.IndividualFaces;probe.resolution=128;probe.hdr=true;probe.boxProjection=false;probe.clearFlags=ReflectionProbeClearFlags.Skybox;probe.nearClipPlane=.2f;probe.farClipPlane=120;probe.intensity=.7f;request=-1;next=0;}
        void LateUpdate(){if(Time.unscaledTime<next||request>=0&&!probe.IsFinishedRendering(request))return;if(follow)transform.position=follow.position;probe.size=Vector3.one*extent;probe.cullingMask=geometry;request=probe.RenderProbe();next=Time.unscaledTime+Mathf.Max(.5f,interval);}
    }
}
