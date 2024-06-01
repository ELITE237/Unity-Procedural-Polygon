# Unity-Procedural-Polygon
A Unity project to generate a polygon via C# scripts

The mesh of the generated polygon is in three levels (the base, the middle, and the top). The base and the levels are composed of a minimum of triangles while the top is adjustable in number of triangles (sampling), and in height for each vertex which allows to have the necessary deformations for example the creation of a height map.
It is also possible to define a different material depending on the height of each triangle on the top side. while the side faces are defined by two unique materials.
The resulting polygon can be modified at the level of the distance of the vertices from the center, the angle that the vertex must make with respect to the x-axis.
