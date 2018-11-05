# Delaunay / Voronoi diagram (Delaunoï)

`2D Delaunay triangulation` and `Voronoi diagram` construction based on **Leonidas J. Guibas** and **Jorge Stolfi** paper
[Primitives for the manipulation of general subdivisions and the computation of Voronoi diagrams](https://dl.acm.org/citation.cfm?doid=282918.282923).

Triangulation and voronoi diagram are implemented using [QuadEdge](https://en.wikipedia.org/wiki/Quad-edge)
datastructure with a divide and conquer algorithm in **C#**.

# Getting started:

Unity example can be found in `Assets/Scenes/GuibasStolfiTest`.


![Delaunay faces](Doc/Delaunay_unfilled.PNG)
![Voronoi faces](Doc/Voronoi.PNG)


# References:

  - [Guibas and Stolfi paper](https://dl.acm.org/citation.cfm?doid=282918.282923)
  - [Geometry predicates](https://www.ics.uci.edu/~eppstein/junkyard/)
  - [Voronoi diagram](https://en.wikipedia.org/wiki/Voronoi_diagram)
