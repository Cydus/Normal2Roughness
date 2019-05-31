# Normal2Roughness
A tool for Unity uses normal maps to create more accurate roughness mipmaps.

This tool is designed to to reduce specular aliasing in specific conditions where normals get smeared by the mipmapping process. It's especially useful for displaying detailed metalic surfaces in VR. Works best when texturs are the same size.

Features:
---------
- Drag and drop materials to convert them (SRP and custom shader textures must be set manually)
- Zero performance cost.
- Adjustable Stength modifier
- Ideal for improving VR image quality

Requirements
------------
Unity 2017+

How it works
------------
Each mip level of a normal map in Unity is a downscaled version of the original. One downside of this downscaling is that the "bumps" in the surface get averaged out and smoothed together. Normal to Roughness tool restores this discarded information by turning the normals into roughness information and adding it to the roughness map.
