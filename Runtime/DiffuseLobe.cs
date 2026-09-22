using UnityEngine;
namespace ProceduralLight
{
    /// <summary>First-order spherical harmonics after Lambertian cosine convolution (irradiance divided by pi).</summary>
    public struct DiffuseLobe
    {
        public Vector3 mean,x,y,z;
        public void AddRadiance(Vector3 direction,Vector3 radiance,float weight)
        {mean+=radiance*weight;x+=radiance*(2*direction.x*weight);y+=radiance*(2*direction.y*weight);z+=radiance*(2*direction.z*weight);}
        public Vector3 Evaluate(Vector3 normal) => Vector3.Max(Vector3.zero,mean+x*normal.x+y*normal.y+z*normal.z);
        public static DiffuseLobe Lerp(DiffuseLobe a,DiffuseLobe b,float t)=>new DiffuseLobe{mean=Vector3.Lerp(a.mean,b.mean,t),x=Vector3.Lerp(a.x,b.x,t),y=Vector3.Lerp(a.y,b.y,t),z=Vector3.Lerp(a.z,b.z,t)};
    }
}
