using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace ProceduralLight.Editor
{
    public static class LightingLab
    {
        [MenuItem("Procedural Light/Create lighting laboratory")]
        public static void Create()
        {
            // Additive scene leaves the user's current work intact.
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);EditorSceneManager.SetActiveScene(scene);
            var root=new GameObject("Procedural lighting laboratory");
            Material Surface(Color color,Color emission){var m=new Material(Shader.Find("Procedural Light/Lit"));m.SetColor("_BaseColor",color);m.SetColor("_EmissionColor",emission);return m;}
            GameObject Box(string name,Vector3 at,Vector3 scale,Color color){var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(root.transform);o.transform.position=at;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=Surface(color,Color.black);return o;}
            Box("Floor",new Vector3(0,-.25f,0),new Vector3(14,.5f,14),Color.gray);
            Box("Red bounce wall",new Vector3(-4,2,0),new Vector3(.3f,4,8),new Color(.8f,.04f,.02f));
            Box("Blue bounce wall",new Vector3(4,2,0),new Vector3(.3f,4,8),new Color(.025f,.09f,.8f));
            var blocker=Box("Move me to test occlusion",new Vector3(0,1.5f,0),new Vector3(.3f,3,2),Color.gray);
            for(int i=0;i<2;i++){var sphere=GameObject.CreatePrimitive(PrimitiveType.Sphere);sphere.name="Neutral receiver";sphere.transform.SetParent(root.transform);sphere.transform.position=new Vector3(i==0?-2:2,1,0);sphere.GetComponent<Renderer>().sharedMaterial=Surface(Color.white,Color.black);}
            var emitter=Box("Animated colored emitter",new Vector3(-2,1.5f,2),Vector3.one*.4f,Color.black);emitter.GetComponent<Renderer>().sharedMaterial=Surface(Color.black,new Color(.1f,1,.3f)*3);var lamp=emitter.AddComponent<LightEmitter>();lamp.color=new Color(.1f,1,.3f);lamp.radius=.5f;lamp.intensity=12;
            var sun=new GameObject("Sun").AddComponent<Light>();sun.transform.SetParent(root.transform);sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(40,-30,0);sun.shadows=LightShadows.Soft;
            var field=root.AddComponent<IrradianceField>();field.resolution=9;field.spacing=1.5f;field.probesPerFrame=8;field.sun=sun;
            var camera=new GameObject("Laboratory camera").AddComponent<Camera>();camera.transform.SetParent(root.transform);camera.transform.position=new Vector3(0,3,-9);camera.transform.LookAt(new Vector3(0,1.5f,0));camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;camera.tag="MainCamera";
            root.AddComponent<ReflectionRefresh>();Selection.activeGameObject=root;
            Debug.Log("Lighting laboratory created in an additive, unsaved scene. Use a URP renderer, add ProbeLitHazeFeature for haze, and enter Play mode. Move the blocker or emitter to inspect lighting updates.");
        }
    }
}
