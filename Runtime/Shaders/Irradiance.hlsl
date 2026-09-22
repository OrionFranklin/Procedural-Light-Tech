#ifndef PROCEDURAL_LIGHT_IRRADIANCE
#define PROCEDURAL_LIGHT_IRRADIANCE
TEXTURE3D(_PLIrradiance0);SAMPLER(sampler_PLIrradiance0);
TEXTURE3D(_PLIrradiance1);TEXTURE3D(_PLIrradiance2);TEXTURE3D(_PLIrradiance3);
float3 _PLOrigin;float4 _PLGrid;float _PLActive;
float3 SampleProceduralIrradiance(float3 position,float3 normal,float3 fallback)
{
    if(_PLActive<.5)return fallback;
    float3 cell=(position-_PLOrigin)/_PLGrid.x;
    float3 edge=min(cell,_PLGrid.y-1-cell);
    float blend=saturate(min(edge.x,min(edge.y,edge.z)));
    if(blend<=0)return fallback;
    float3 uv=(cell+.5)/_PLGrid.y;
    float4 c=SAMPLE_TEXTURE3D_LOD(_PLIrradiance0,sampler_PLIrradiance0,uv,0);
    float3 x=SAMPLE_TEXTURE3D_LOD(_PLIrradiance1,sampler_PLIrradiance0,uv,0).rgb;
    float3 y=SAMPLE_TEXTURE3D_LOD(_PLIrradiance2,sampler_PLIrradiance0,uv,0).rgb;
    float3 z=SAMPLE_TEXTURE3D_LOD(_PLIrradiance3,sampler_PLIrradiance0,uv,0).rgb;
    float3 irradiance=max(0,(c.rgb+x*normal.x+y*normal.y+z*normal.z)/max(c.a,.0001));
    return lerp(fallback,irradiance,blend*c.a);
}
#endif
