using UnityEngine;
namespace ProceduralLight
{
    /// <summary>CPU visibility queries refresh a small world-space grid. Original, single-bounce approximation; no hardware ray tracing.</summary>
    public sealed class IrradianceField : MonoBehaviour
    {
        [Range(3,15)] public int resolution=7;
        [Min(.5f)] public float spacing=3;
        [Range(1,32)] public int probesPerFrame=3;
        [Range(8,96)] public int raysPerProbe=24;
        [Min(1)] public float rayDistance=24;
        [Range(0,1)] public float history=.65f;
        public LayerMask geometry=~0;
        public Transform follow;
        public Light sun;
        public Vector3 skyUp=Vector3.up;
        public float bounceStrength=.7f;
        public int UpdatedProbes {get;private set;}
        public int LastRayCount {get;private set;}
        public int ReadyProbes {get;private set;}
        Texture3D[] textures;Color[][] pixels;DiffuseLobe[] lobes;bool[] ready;Vector3 origin;int cursor,n;float cell;
        static readonly int Active=Shader.PropertyToID("_PLActive");
        public Vector3 Origin=>origin;
        public bool Contains(Vector3 point){var p=(point-origin)/cell;return p.x>=0&&p.y>=0&&p.z>=0&&p.x<n-1&&p.y<n-1&&p.z<n-1;}
        void OnEnable(){Allocate();}
        void Allocate()
        {
            Release();n=Mathf.Clamp(resolution,3,15);cell=Mathf.Max(.5f,spacing);lobes=new DiffuseLobe[n*n*n];ready=new bool[lobes.Length];pixels=new Color[4][];textures=new Texture3D[4];
            for(int i=0;i<4;i++){pixels[i]=new Color[lobes.Length];textures[i]=new Texture3D(n,n,n,TextureFormat.RGBAHalf,false){name="Local irradiance "+i,wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};}
            origin=SnappedOrigin(follow?follow.position:transform.position);ReadyProbes=cursor=0;
        }
        Vector3 SnappedOrigin(Vector3 p)=>new Vector3(Mathf.Floor(p.x/cell),Mathf.Floor(p.y/cell),Mathf.Floor(p.z/cell))*cell-Vector3.one*((n-1)/2*cell);
        int Index(int x,int y,int z)=>x+n*(y+n*z);
        void Recenter(Vector3 next)
        {
            var delta=Vector3Int.RoundToInt((next-origin)/cell);if(delta==Vector3Int.zero)return;
            var old=lobes;var oldReady=ready;lobes=new DiffuseLobe[old.Length];ready=new bool[old.Length];ReadyProbes=0;
            for(int z=0;z<n;z++)for(int y=0;y<n;y++)for(int x=0;x<n;x++){
                int a=x+delta.x,b=y+delta.y,c=z+delta.z;if(a<0||b<0||c<0||a>=n||b>=n||c>=n)continue;
                int i=Index(x,y,z),j=Index(a,b,c);lobes[i]=old[j];ready[i]=oldReady[j];if(ready[i])ReadyProbes++;
            }
            origin=next;
        }
        void LateUpdate()
        {
            if(textures==null||n!=resolution||Mathf.Abs(cell-spacing)>.001f)Allocate();
            Recenter(SnappedOrigin(follow?follow.position:transform.position));LastRayCount=0;
            for(int k=0;k<Mathf.Clamp(probesPerFrame,1,32);k++){
                int i=cursor++%lobes.Length;var p=origin+new Vector3(i%n,(i/n)%n,i/(n*n))*cell;
                var sample=Trace(p);if(!ready[i]){ReadyProbes++;ready[i]=true;lobes[i]=sample;}else lobes[i]=DiffuseLobe.Lerp(sample,lobes[i],history);
                UpdatedProbes++;
            }
            Upload();
        }
        static Vector3 RGB(Color c)=>new Vector3(c.r,c.g,c.b);
        bool Ray(Vector3 p,Vector3 d,out RaycastHit hit,float distance){LastRayCount++;return Physics.Raycast(p,d,out hit,distance,geometry,QueryTriggerInteraction.Ignore);}
        public DiffuseLobe Trace(Vector3 position)
        {
            // Rays originating inside solids do not report their enclosing collider.
            // Treat these cells as dark rather than inventing an unobstructed sky view.
            if(Physics.CheckSphere(position,.035f,geometry,QueryTriggerInteraction.Ignore))return default;
            DiffuseLobe result=default;int rays=Mathf.Clamp(raysPerProbe,8,96);
            for(int j=0;j<rays;j++){
                float y=1-2*(j+.5f)/rays,angle=j*2.39996323f,r=Mathf.Sqrt(1-y*y);var direction=new Vector3(r*Mathf.Cos(angle),y,r*Mathf.Sin(angle));Vector3 radiance;
                if(!Ray(position,direction,out var hit,rayDistance)){
                    float vertical=Vector3.Dot(direction,skyUp);radiance=RGB(Color.Lerp(RenderSettings.ambientEquatorColor,vertical>0?RenderSettings.ambientSkyColor:RenderSettings.ambientGroundColor,Mathf.Abs(vertical)));
                }else{
                    var renderer=hit.collider.GetComponent<Renderer>();var material=renderer?renderer.sharedMaterial:null;
                    Color albedo=material&&material.HasProperty("_BaseColor")?material.GetColor("_BaseColor"):material&&material.HasProperty("_Color")?material.color:Color.gray;
                    radiance=Vector3.zero;var offset=hit.point+hit.normal*.035f;
                    if(sun&&sun.enabled){var lightDirection=-sun.transform.forward;float cosine=Mathf.Max(0,Vector3.Dot(hit.normal,lightDirection));if(cosine>0&&!Ray(offset,lightDirection,out _,rayDistance*2))radiance=Vector3.Scale(RGB(albedo),RGB(sun.color))*(sun.intensity*cosine*bounceStrength);}
                    if(material&&material.HasProperty("_EmissionColor")&&material.IsKeywordEnabled("_EMISSION"))radiance+=RGB(material.GetColor("_EmissionColor"));
                }
                result.AddRadiance(direction,radiance,1f/rays);
            }
            int lights=0;
            foreach(var emitter in LightEmitter.Active){if(!emitter||!emitter.isActiveAndEnabled)continue;var delta=emitter.transform.position-position;float distance=delta.magnitude;if(distance<.01f||distance>emitter.range)continue;if(lights++>=16)break;
                var direction=delta/distance;if(Ray(position,direction,out _,Mathf.Max(0,distance-emitter.radius)))continue;
                result.AddRadiance(direction,RGB(emitter.color)*emitter.Attenuation(distance),.25f);
            }
            return result;
        }
        void Upload()
        {
            for(int i=0;i<lobes.Length;i++)for(int c=0;c<4;c++){var v=c==0?lobes[i].mean:c==1?lobes[i].x:c==2?lobes[i].y:lobes[i].z;pixels[c][i]=new Color(v.x,v.y,v.z,ready[i]?1:0);}
            for(int c=0;c<4;c++){textures[c].SetPixels(pixels[c]);textures[c].Apply(false,false);Shader.SetGlobalTexture("_PLIrradiance"+c,textures[c]);}
            Shader.SetGlobalVector("_PLOrigin",origin);Shader.SetGlobalVector("_PLGrid",new Vector4(cell,n,1/(cell*n),0));Shader.SetGlobalFloat(Active,1);
        }
        void OnDisable(){Shader.SetGlobalFloat(Active,0);Release();}
        void Release(){if(textures!=null)foreach(var texture in textures)if(texture){if(Application.isPlaying)Destroy(texture);else DestroyImmediate(texture);}textures=null;}
    }
}
