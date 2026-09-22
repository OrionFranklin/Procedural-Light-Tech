using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
namespace ProceduralLight
{
    public sealed class ProbeLitHazeFeature:ScriptableRendererFeature
    {
        public Shader shader;
        [Range(0,.1f)] public float density=.008f;
        [Range(4,32)] public int samples=12;
        [Min(1)] public float distance=24;
        Material material;HazePass pass;
        public override void Create(){CoreUtils.Destroy(material);if(!shader)shader=Shader.Find("Hidden/Procedural Light/Probe Lit Haze");if(shader)material=CoreUtils.CreateEngineMaterial(shader);pass=new HazePass(material);}
        public override void AddRenderPasses(ScriptableRenderer renderer,ref RenderingData data){if(!material||data.cameraData.cameraType!=CameraType.Game||density<=0)return;material.SetVector("_PLHaze",new Vector4(density,samples,distance,0));renderer.EnqueuePass(pass);}
        protected override void Dispose(bool disposing)=>CoreUtils.Destroy(material);
        sealed class HazePass:ScriptableRenderPass
        {
            readonly Material material;
            public HazePass(Material value){material=value;renderPassEvent=RenderPassEvent.BeforeRenderingTransparents;ConfigureInput(ScriptableRenderPassInput.Depth);requiresIntermediateTexture=true;}
            public override void RecordRenderGraph(RenderGraph graph,ContextContainer frame){var resources=frame.Get<UniversalResourceData>();if(!material||resources.isActiveTargetBackBuffer)return;var desc=graph.GetTextureDesc(resources.activeColorTexture);desc.name="Probe-lit haze";desc.clearBuffer=false;var output=graph.CreateTexture(desc);graph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(resources.activeColorTexture,output,material,0),"Probe-lit haze");resources.cameraColor=output;}
        }
    }
}
