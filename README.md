# Unity-Procedural-Polygon
A Unity project to generate a polygon via C# scripts

The mesh of the generated polygon is in three levels (the base, the middle, and the top). The base and the levels are composed of a minimum of triangles while the top is adjustable in number of triangles (sampling), and in height for each vertex which allows to have the necessary deformations for example the creation of a height map.
It is also possible to define a different material depending on the height of each triangle on the top side. while the side faces are defined by two unique materials.
The resulting polygon can be modified at the level of the distance of the vertices from the center, the angle that the vertex must make with respect to the x-axis.

## Content
Two scripts are provided in this project. 
- **PolyMeshData** which contains all the logic to create a polygon mesh,
- **PolyNode** which is an example script for generating a polygon using PolyMeshData.

## Documentation

### HeightRenderer Struct
- **[Material](https://docs.unity3d.com/ScriptReference/Material.html) HeightRenderer.Material**
- **float HeightRenderer.Height**
  
### PolyMeshRendererSettings
- **[Material](https://docs.unity3d.com/ScriptReference/Material.html) PolyMeshRendererSettings.BaseMaterial** : Material for the base of the polygon and the lower part of the polygon sides
- **[Material](https://docs.unity3d.com/ScriptReference/Material.html) PolyMeshRendererSettings.TopMaterial** : Material for the upper part of the polygon sides
- **HeightRenderer[] PolyMeshRendererSettings.HeightsRenderer** : Parameters for rendering the height levels of the top of the polygon.

### PolyMeshData Class
- **const int PolyMeshData.MinPolyLen** : The minimum number of sides for a polygon. its value is 3.
- **int PolyMeshData.PolyLen** : The number of sides of the polygon.
- **int PolyMeshData.Sampling** : The sampling level for the top face of the polygon. its minimum value is 1.
- **float[] PolyMeshData.BaseSizes** : The different distances of the base vertices from the base center.
- **float[] PolyMeshData.TopSizes** : The different distances of the top vertices from the top center.
- **float PolyMeshData.FloretFactor** : A value between 0 and 1 that transforms the polygon into a polygon of florets.
- **float[] PolyMeshData.SummitsAngles** : The different angles of the polygon vertices.
- **float[] PolyMeshData.PolyHeights** : The different heights of the polygon vertices of the top side.
- **float PolyMeshData.Slope** : A value between -1 and 1 that changes the height of the center of the top face of the polygon to produce a concave (-1), flat (0), or convex (1) face.
- **bool PolyMeshData.Curving** : Determines whether the slope of a vertex at the center of both of the top face will be curved or not.
- **float PolyMeshData.CuttOff** : The height at which the polygon was cut in order to use these two basic materials. This value is automatically clamped between the different heights of the polygon's vertices.
- **float[] PolyMeshData.MeshHeights** : The height limits for the creation of the different sub-meshes of the top face for a rendering in several materials.
- **float PolyMeshData.RendFactor** : A value between 0 and 1 that determines the height to be taken for a triangle of the mesh for its attribution to a sub-mesh.
- **void PolyMeshData.SetSummits(int polyLen, int sampling = 1, float floretFactor = 0f, float[] baseSizes = null, float[] topSizes = null, float[] summitsAngles = null)** : Assigns the various parameters to the corresponding properties for generating polygon vertices.
- **void PolyMeshData.SetPolyHeights(float[] polyHeights, float slope = 0f, bool curving = false, float cutoOff = 0f)** : Assigns the various parameters to the corresponding properties for generating polygon heights.
- **void PolyMeshData.SetMeshHeights(float[] meshHeights, float rendFactor)** : Assigns the various parameters to the corresponding properties for generating sub-meshes polygon.
- **void PolyMeshData.SetToRegularPolygon(int polyLen, int sampling, float floretFactor, float baseSize, float topSize, float angleOffset)** : Assigns values to generate a regular polygon (single size, evenly spaced vertices).
- **void PolyMeshData.UseSingleHeight(float height)** : Assigns the values to generate a polygon with the vertices of the top face at the same height.
- **[Mesh](https://docs.unity3d.com/ScriptReference/Mesh.html) PolyMeshData.CreateMesh()** : Returns a mesh that can be used for a [MeshFilter](https://docs.unity3d.com/ScriptReference/MeshFilter.html), [MeshCollider](https://docs.unity3d.com/ScriptReference/MeshCollider.html), or [MeshRenderer](https://docs.unity3d.com/ScriptReference/MeshRenderer.html).
