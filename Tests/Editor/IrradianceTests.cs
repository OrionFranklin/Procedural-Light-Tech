using NUnit.Framework;
using UnityEngine;
namespace ProceduralLight.Tests
{
    public sealed class IrradianceTests
    {
        [Test] public void AmbientRadianceMatchesProjectColorSpace()
        {
            var sky=RenderSettings.ambientSkyColor;var equator=RenderSettings.ambientEquatorColor;var ground=RenderSettings.ambientGroundColor;var obj=new GameObject("color space probe");
            try {
                var color=new Color(.2f,.3f,.4f);RenderSettings.ambientSkyColor=RenderSettings.ambientEquatorColor=RenderSettings.ambientGroundColor=color;
                var grid=obj.AddComponent<IrradianceField>();grid.geometry=0;grid.raysPerProbe=96;
                var result=grid.Trace(new Vector3(10000,10000,10000)).Evaluate(Vector3.up);var expected=QualitySettings.activeColorSpace==ColorSpace.Linear?color.linear:color;
                Assert.That(Vector3.Distance(result,new Vector3(expected.r,expected.g,expected.b)),Is.LessThan(.003f));
            }finally {Object.DestroyImmediate(obj);RenderSettings.ambientSkyColor=sky;RenderSettings.ambientEquatorColor=equator;RenderSettings.ambientGroundColor=ground;}
        }
        [Test] public void UniformRadianceRemainsUniformUnderDiffuseConvolution(){DiffuseLobe lobe=default;foreach(var d in new[]{Vector3.up,Vector3.down,Vector3.left,Vector3.right,Vector3.forward,Vector3.back})lobe.AddRadiance(d,new Vector3(.2f,.5f,.8f),1f/6);Assert.That(Vector3.Distance(lobe.Evaluate(new Vector3(1,2,3).normalized),new Vector3(.2f,.5f,.8f)),Is.LessThan(.0001f));}
        [Test] public void DirectionalRadianceLightsFacingNormals(){DiffuseLobe lobe=default;lobe.AddRadiance(Vector3.right,Vector3.one,1);Assert.That(lobe.Evaluate(Vector3.right).x,Is.GreaterThan(lobe.Evaluate(Vector3.left).x));Assert.That(lobe.Evaluate(Vector3.left).x,Is.Zero);}
        [Test] public void HistoryBlendHasNoOvershoot(){var a=new DiffuseLobe{mean=Vector3.one};var b=new DiffuseLobe{mean=Vector3.zero};Assert.That(DiffuseLobe.Lerp(a,b,.75f).Evaluate(Vector3.up).x,Is.EqualTo(.25f).Within(.0001f));}
        [Test] public void OccluderBlocksEmissiveContribution(){var previousSky=RenderSettings.ambientSkyColor;var previousEquator=RenderSettings.ambientEquatorColor;var previousGround=RenderSettings.ambientGroundColor;GameObject field=null,emitter=null,wall=null;try{RenderSettings.ambientSkyColor=RenderSettings.ambientEquatorColor=RenderSettings.ambientGroundColor=Color.black;field=new GameObject("field");var grid=field.AddComponent<IrradianceField>();emitter=new GameObject("emitter");emitter.transform.position=Vector3.right*4;var light=emitter.AddComponent<LightEmitter>();light.color=Color.red;light.radius=1;light.intensity=10;var lit=grid.Trace(Vector3.zero).Evaluate(Vector3.right);wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=Vector3.right*2;wall.transform.localScale=new Vector3(.5f,8,8);Physics.SyncTransforms();var blocked=grid.Trace(Vector3.zero).Evaluate(Vector3.right);Assert.That(lit.x,Is.GreaterThan(.1f));Assert.That(blocked.x,Is.LessThan(.001f));}finally{if(wall)Object.DestroyImmediate(wall);if(emitter)Object.DestroyImmediate(emitter);if(field)Object.DestroyImmediate(field);RenderSettings.ambientSkyColor=previousSky;RenderSettings.ambientEquatorColor=previousEquator;RenderSettings.ambientGroundColor=previousGround;}}
    }
}
