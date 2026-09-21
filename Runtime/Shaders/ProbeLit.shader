Shader "Procedural Light/Lit" {
    Properties { _BaseColor("Albedo",Color)=(.6,.6,.6,1) _EmissionColor("Emission",Color)=(0,0,0,1) }
    SubShader { Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
        Pass {Tags {"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Irradiance.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor,_EmissionColor;
            CBUFFER_END
            struct Attributes {float4 position:POSITION;float3 normal:NORMAL;};
            struct Varyings {float4 position:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;};
            Varyings Vertex(Attributes i){Varyings o;o.world=TransformObjectToWorld(i.position.xyz);o.position=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(i.normal);return o;}
            half4 Fragment(Varyings i):SV_Target {float3 n=normalize(i.normal);Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));float3 diffuse=SampleProceduralIrradiance(i.world,n,SampleSH(n));diffuse+=sun.color*(saturate(dot(n,sun.direction))*sun.shadowAttenuation);return half4(_BaseColor.rgb*diffuse+_EmissionColor.rgb,1);}
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
