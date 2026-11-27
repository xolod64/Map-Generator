# Map-Generator

## Visualization Modes

The editor/visualizer provides several rendering modes to inspect each stage of the procedural generation pipeline:

* **Voronoi/Delone Graph:** Displays the foundational dual-graph topology of the map, showing polygonal region borders (Voronoi edges/vertices) alongside the adjacency connections between region centers (Delaunay edges).
<br><br><img width="1919" height="891" alt="VD-Graph" src="https://github.com/user-attachments/assets/00890339-056b-479d-9e6e-f94854b5a4e1" />  <br><br>

* **Sea/Land Graph:** Visualizes the binary partition of the world into ocean and land regions resulting from the initial boundary random-walk flooding pass.
<br><br><img width="1919" height="892" alt="SeaLand" src="https://github.com/user-attachments/assets/af4d2cd1-55ab-49e0-9399-da38f24502d0" /><br><br>

* **Landmasses Graph:** Highlights isolated land clusters.
<br><br><img width="1919" height="882" alt="Landmasses" src="https://github.com/user-attachments/assets/e3b8d8c1-954d-4111-97b1-486d57612c80" />  <br><br>

* **Regions Heights:** Displays the base coarse elevation calculated for each land region as height propagates inland from the coastlines.
<br><br><img width="1919" height="894" alt="RegionHeights" src="https://github.com/user-attachments/assets/8fb0703b-db9a-4d21-8568-02e2ed2e664f" /><br><br>  
  
* **Basins:** Visualizes individual river paths growing from coastal mouths into the landmass.
<br><br><img width="1919" height="892" alt="Basins" src="https://github.com/user-attachments/assets/63a55b2a-c302-42c4-9d35-185301337918" /><br><br> 
