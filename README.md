# Procedural Light Tech

An original Unity 6 / URP 17.6 lighting prototype for Astromancer. Inspired by the discussion of irradiance probes, emissive bounce, haze and offscreen reflections in [VFX Artists Breakdown GTA 6's IMPOSSIBLE Graphics](https://www.youtube.com/watch?v=ULM2OIj5o0I). The speakers' account is inspiration, not a specification of Rockstar's implementation.

## Implemented

- A local, world-aligned irradiance grid follows a target. Grid shifts retain overlapping samples rather than resetting the whole field.
- Budgeted physics rays estimate sky visibility and one diffuse sun bounce from collider surfaces. Colored emitters contribute with line-of-sight occlusion and finite range.
- Four half-float 3D textures hold first-order spherical harmonics after diffuse cosine convolution. Trilinear sampling, temporal blending, validity and edge fades reduce visible transitions.
- A URP Render Graph haze pass samples that same irradiance field along depth-limited camera rays.
- Time-sliced realtime reflection probes capture geometry outside the camera view.
- An asset-free lighting laboratory and EditMode tests cover directional response, energy preservation, history blending and emitter occlusion.

## Install and use

Add this repository through Unity Package Manager's Git URL option, or embed the directory under `Packages/com.orion.procedural-light-tech`. Requires Unity 6000.6 and URP 17.6.0 for the included Render Graph feature.

1. Add `IrradianceField`, assign the target transform and directional sun. A single active field publishes global textures for the main camera. For spherical worlds, update `skyUp` to the local surface normal.
2. Add `LightEmitter` to glowing sources. `color` is linear HDR radiance; `intensity`, `radius` and `range` control the approximation.
3. In a lit shader, include `Runtime/Shaders/Irradiance.hlsl` and use `SampleProceduralIrradiance(positionWS, normalWS, fallbackGI)` for diffuse baked GI. The supplied `Procedural Light/Lit` shader demonstrates integration.
4. Add `ProbeLitHazeFeature` to the URP Renderer and retain its shader reference in the asset. Use `ReflectionRefresh` on a reflection probe for offscreen captures.
5. Use **Procedural Light → Create lighting laboratory**, then enter Play mode. The lab opens additively and is unsaved; keep one main camera enabled. Move its wall or emitter to inspect visibility changes. Save the generated materials if saving the lab permanently.

Tests run from the Unity Test Runner after adding `com.orion.procedural-light-tech` to the project manifest's `testables` array.

## Cost and limits

Defaults: 7³ probes, three probes refreshed per frame, 24 directions per probe. At most two visibility rays per direction plus 16 emitter rays per probe (192 raycasts/frame), and three small overlap checks. One complete refresh takes 115 frames. Texture uploads total about 11 KB/frame; the haze defaults to 12 samples/pixel. Reflection captures are 128², scheduled every two seconds with individual-face time slicing.

This is a coarse, single-bounce approximation using physics collider geometry and material tint. It does not sample albedo textures, skinned/deformed mesh detail or screen-space lighting, and is not hardware ray tracing, path tracing, multi-bounce GI or a mirror-quality reflection system. Thin walls and interpolation can leak light. Cells inside colliders are conservatively dark. Up to 16 in-range registered emitters contribute per probe. Default ambient colors should be authored sensibly; their energy is used directly. Multiple simultaneously active irradiance fields/cameras require a per-camera binding extension.

Only original code is included. No purchased game assets or water package code are distributed here. No open-source license has been selected by the repository owner.
