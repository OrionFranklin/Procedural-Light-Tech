Shader "Hidden/Procedural Light/Probe Lit Haze" {
    SubShader { Tags {"RenderPipeline"="UniversalPipeline"} Pass {ZTest Always ZWrite Off Cull Off
        HLSLPROGRAM
        #pragma vertex Vert
        #pragma fragment Frag
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Irradiance.hlsl"
        float4 _PLHaze;
        half4 Frag(Varyings input):SV_Target {
            half4 color=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,input.texcoord);
            if(_PLActive<.5)return color;
            float depth=SampleSceneDepth(input.texcoord);
            #if !UNITY_REVERSED_Z
            depth=lerp(UNITY_NEAR_CLIP_VALUE,1,depth);
            #endif
            float3 origin=GetCameraPositionWS();float3 hit=ComputeWorldSpacePosition(input.texcoord,depth,UNITY_MATRIX_I_VP);
            float3 delta=hit-origin,dir=normalize(delta);float stepSize=min(length(delta),_PLHaze.z)/_PLHaze.y;
            float transmittance=1;float3 haze=0;float opacity=1-exp(-stepSize*_PLHaze.x);
            for(int i=0;i<(int)_PLHaze.y;i++){
                float3 position=origin+dir*((i+.5)*stepSize);float3 light=SampleProceduralIrradiance(position,-dir,0);
                // Empty/uninitialized cells contribute no haze, avoiding bright startup flashes.
                float amount=saturate(dot(light,float3(.2126,.7152,.0722))*4);float alpha=opacity*amount;
                haze+=transmittance*alpha*light;transmittance*=1-alpha;
            }
            return half4(color.rgb*transmittance+haze,color.a);
        }
        ENDHLSL
    }}
}
