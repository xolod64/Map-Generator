using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MapVisualizer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Texture2D texture;
    private Color32[] pixels;
    private int textureWidth;
    private Map map;

    public void InitTexture(int width, int height, Map map)
    {
        this.map = map;

        pixels = new Color32[width * height];
        textureWidth = width;

        texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point; // без розмиття пікселів

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f), // pivot по центру
            1f                        // 1 піксель = 1 юніт
        );

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
    }

    public void DrawMapGraph()
    {
        RegionsGraph regionsGraph = map.regionsGraph;
        BordersGraph bordersGraph = map.bordersGraph;

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.black;
        }

        foreach (Road currentRoad in regionsGraph.roads)
        {
            (int ax, int ay) = currentRoad.firstRegion.center;
            (int bx, int by) = currentRoad.secondRegion.center;

            DrawLine(ax, ay, bx, by, Color.gray);
        }

        foreach (Border currentBorder in bordersGraph.borders)
        {
            (int ax, int ay) = currentBorder.firstPoint.center;
            (int bx, int by) = currentBorder.secondPoint.center;

            DrawLine(ax, ay, bx, by, Color.white);
        }

        foreach (Region currentRegion in regionsGraph.regions)
        {
            (int currentX, int currentY) = currentRegion.center;

            SetPixel(currentX, currentY, Color.red);
        }

        foreach (BorderPoint currentPoint in bordersGraph.points)
        {
            //SetPixel(currentPoint.center.x, currentPoint.center.y, Color.white);
        }

        Apply();
    }

    public void DrawSeaLandGraph()
    {
        RegionsGraph regionsGraph = map.regionsGraph;
        BordersGraph bordersGraph = map.bordersGraph;

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.black;
        }

        foreach (Border currentBorder in bordersGraph.borders)
        {
            (int ax, int ay) = currentBorder.firstPoint.center;
            (int bx, int by) = currentBorder.secondPoint.center;

            if (!currentBorder.road.firstRegion.isSea || !currentBorder.road.secondRegion.isSea)
                DrawLine(ax, ay, bx, by, Color.green);
            else
                DrawLine(ax, ay, bx, by, Color.blue);
        }

        foreach (Region currentRegion in regionsGraph.regions)
        {
            (int currentX, int currentY) = currentRegion.center;
            if (currentRegion.isSea) SetPixel(currentX, currentY, Color.blue);
            else SetPixel(currentX, currentY, Color.green);
        }

        Apply();
    }

    public void DrawLandmasses()
    {
        List<RegionsGraph> landmasses = map.landmasses;
        BordersGraph bordersGraph = map.bordersGraph;

        HashSet<Border> drawedBorders = new HashSet<Border>();

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.black;
        }

        // ColorUtility.TryParseHtmlString("13102b", out Color waterColor);

        // foreach (Border currentBorder in bordersGraph.borders)
        // {
        //     if (!currentBorder.road.firstRegion.isSea || !currentBorder.road.secondRegion.isSea) continue;

        //     (int ax, int ay) = map.To2DCoords(currentBorder.firstPoint.centerIndex);
        //     (int bx, int by) = map.To2DCoords(currentBorder.secondPoint.centerIndex);

        //     Vector2Int aPoint = new Vector2Int(ax, ay);
        //     Vector2Int bPoint = new Vector2Int(bx, by);

        //     DrawLine(aPoint, bPoint, waterColor);
        // }

        float hue = 0f;
        float goldenRatioConjugate = 0.618033988749895f;

        foreach (RegionsGraph landmass in landmasses)
        {
            Color uniqueColor = Color.HSVToRGB(hue, 0.8f, 0.85f);

            hue += goldenRatioConjugate;
            hue %= 1f;

            foreach (Region currentRegion in landmass.regions)
            {
                (int currentX, int currentY) = currentRegion.center;
                SetPixel(currentX, currentY, uniqueColor);

                foreach (Road currentRoad in currentRegion.roads)
                {
                    if (drawedBorders.Contains(currentRoad.border)) continue;
                    drawedBorders.Add(currentRoad.border);

                    (int ax, int ay) = currentRoad.border.firstPoint.center;
                    (int bx, int by) = currentRoad.border.secondPoint.center;

                    Vector2Int aPoint = new Vector2Int(ax, ay);
                    Vector2Int bPoint = new Vector2Int(bx, by);

                    DrawLine(ax, ay, bx, by, uniqueColor);
                }
            }
        }

        Apply();
    }

    public void DrawTectonicForces()
    {
        Color[] regionsColors = new Color[map.regionsGraph.regions.Count];

        foreach (Region region in map.regionsGraph.regions)
        {
            float tectonicForce = map.regionsTectonicForces[region.id];
            regionsColors[region.id] = new Color(tectonicForce, tectonicForce, tectonicForce, 1f);
        }

        for (int x = 0; x < map.sizeInCells; x++)
        {
            for (int y = 0; y < map.sizeInCells; y++)
            {
                Color cellColor = map.regionsGraph.regions[map.cellToRegion[x, y]].isSea ? Color.blue : regionsColors[map.cellToRegion[x, y]];
                SetPixel(x, y, cellColor);
            }
        }

        Apply();
    }

    public void DrawRegionsHeights()
    {
        Color[] regionsColors = new Color[map.regionsGraph.regions.Count];

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        foreach (Region region in map.regionsGraph.regions)
        {
            if (minHeight > region.height) minHeight = region.height;
            if (maxHeight < region.height) maxHeight = region.height;
        }

        foreach (Region region in map.regionsGraph.regions)
        {
            float normalized = region.height / maxHeight;
            regionsColors[region.id] = new Color(normalized, normalized, normalized, 1f);
        }

        for (int x = 0; x < map.sizeInCells; x++)
        {
            for (int y = 0; y < map.sizeInCells; y++)
            {
                Color cellColor = map.regionsGraph.regions[map.cellToRegion[x, y]].isSea ? Color.blue : regionsColors[map.cellToRegion[x, y]];
                SetPixel(x, y, cellColor);
            }
        }

        Apply();
    }

    public void DrawRegionsAndBorderPointsHeights()
    {
        Color[] regionsColors = new Color[map.regionsGraph.regions.Count];

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        foreach (Region region in map.regionsGraph.regions)
        {
            if (minHeight > region.height) minHeight = region.height;
            if (maxHeight < region.height) maxHeight = region.height;
        }

        foreach (BorderPoint point in map.bordersGraph.points)
        {
            if (minHeight > point.height) minHeight = point.height;
            if (maxHeight < point.height) maxHeight = point.height;
        }

        foreach (Region region in map.regionsGraph.regions)
        {
            float normalized = region.height / maxHeight;
            regionsColors[region.id] = new Color(normalized, normalized, normalized, 1f);
        }

        for (int x = 0; x < map.sizeInCells; x++)
        {
            for (int y = 0; y < map.sizeInCells; y++)
            {
                Color cellColor = map.regionsGraph.regions[map.cellToRegion[x, y]].isSea ? Color.blue : regionsColors[map.cellToRegion[x, y]];
                SetPixel(x, y, cellColor);
            }
        }
        
        foreach (BorderPoint point in map.bordersGraph.points)
        {
            float normalized = point.height / maxHeight;
            SetPixel(point.center.x, point.center.y, new Color(normalized, normalized, normalized, 1f));
        }

        Apply();
    }

    public void DrawCellsHeights()
    {
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        foreach (Cell cell in map.cells)
        {
            if (minHeight > cell.height) minHeight = cell.height;
            if (maxHeight < cell.height) maxHeight = cell.height;
        }

        for (int x = 0; x < map.sizeInCells; x++)
        {
            for (int y = 0; y < map.sizeInCells; y++)
            {
                float normalized = map.cells[x, y].height / maxHeight;
                Color cellColor = map.regionsGraph.regions[map.cellToRegion[x, y]].isSea ? Color.blue : new Color(normalized, normalized, normalized, 1f);
                SetPixel(x, y, cellColor);
            }
        }

        Apply();
    }
    
    public void DrawBasins()
    {
        Color[] regionsColors = new Color[map.regionsGraph.regions.Count];

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        foreach (Region region in map.regionsGraph.regions)
        {
            if (minHeight > region.height) minHeight = region.height;
            if (maxHeight < region.height) maxHeight = region.height;
        }

        foreach (Region region in map.regionsGraph.regions)
        {
            float normalized = region.height / maxHeight;
            regionsColors[region.id] = new Color(normalized, normalized, normalized, 1f);
        }

        for (int x = 0; x < map.sizeInCells; x++)
        {
            for (int y = 0; y < map.sizeInCells; y++)
            {
                Color cellColor = map.regionsGraph.regions[map.cellToRegion[x, y]].isSea ? Color.blue : regionsColors[map.cellToRegion[x, y]];
                SetPixel(x, y, cellColor);
            }
        }

        // for (int i = 0; i < pixels.Length; i++)
        // {
        //     pixels[i] = Color.black;
        // }

        // foreach (RegionsGraph landmass in map.landmasses)
        // {
        //     foreach (Road currentRoad in landmass.roads)
        //     {
        //         (int ax, int ay) = currentRoad.border.firstPoint.center;
        //         (int bx, int by) = currentRoad.border.secondPoint.center;

        //         Vector2Int aPoint = new Vector2Int(ax, ay);
        //         Vector2Int bPoint = new Vector2Int(bx, by);

        //         DrawLine(ax, ay, bx, by, new Color(0.1f, 0.1f, 0.1f, 1f));
        //     }
        // }


        Dictionary<BorderPoint, List<Border>> basins = map.basins;

        // ColorUtility.TryParseHtmlString("13102b", out Color waterColor);

        // foreach (Border currentBorder in bordersGraph.borders)
        // {
        //     if (!currentBorder.road.firstRegion.isSea || !currentBorder.road.secondRegion.isSea) continue;

        //     (int ax, int ay) = map.To2DCoords(currentBorder.firstPoint.centerIndex);
        //     (int bx, int by) = map.To2DCoords(currentBorder.secondPoint.centerIndex);

        //     Vector2Int aPoint = new Vector2Int(ax, ay);
        //     Vector2Int bPoint = new Vector2Int(bx, by);

        //     DrawLine(aPoint, bPoint, waterColor);
        // }

        float hue = 0f;
        float goldenRatioConjugate = 0.618033988749895f;

        foreach (List<Border> basin in basins.Values)
        {
            Color uniqueColor = Color.HSVToRGB(hue, 0.8f, 0.85f);

            hue += goldenRatioConjugate;
            hue %= 1f;

            foreach (Border border in basin)
            {
                (int ax, int ay) = border.firstPoint.center;
                (int bx, int by) = border.secondPoint.center;

                Vector2Int aPoint = new Vector2Int(ax, ay);
                Vector2Int bPoint = new Vector2Int(bx, by);

                DrawLine(ax, ay, bx, by, uniqueColor);
            }
        }

        Apply();
    }

    public void DrawRegions()
    {
        Color[] regionsColors = new Color[map.regionsGraph.regions.Count];

        float hue = 0f;
        float goldenRatioConjugate = 0.618033988749895f;

        foreach (Region region in map.regionsGraph.regions)
        {
            Color uniqueColor = Color.HSVToRGB(hue, 0.8f, 0.85f);

            regionsColors[region.id] = uniqueColor;

            hue += goldenRatioConjugate;
            hue %= 1f;
        }

        for (int x = 0; x < map.sizeInCells; x++)
        {
            for (int y = 0; y < map.sizeInCells; y++)
            {
                Color cellColor = regionsColors[map.cellToRegion[x, y]];
                SetPixel(x, y, cellColor);
            }
        }

        Apply();
    }

    private void DrawLine(int x0, int y0, int x1, int y1, Color32 color)
    {
        // 1. Рахуємо абсолютну відстань між точками по X та Y (завжди додатні числа)
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);

        // 2. Визначаємо напрямок руху (-1 або 1). 
        // Якщо крокуємо вперед — 1, якщо назад (координата зменшується) — -1
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        // 3. Початковий баланс нашої "помилки" відхилення
        int err = dx - dy;

        // Головний цикл малювання
        while (true)
        {
            // Малюємо поточний піксель через твій метод
            SetPixel(x0, y0, color);

            // Якщо дійшли до фінальної точки — зупиняємо цикл
            if (x0 == x1 && y0 == y1)
                break;

            // Подвоюємо помилку для цілочисельної математики Брезенгема
            int e2 = 2 * err;

            // Перевіряємо, чи пора робити крок по осі X
            if (e2 > -dy)
            {
                err -= dy; // Зміщуємо баланс терезів
                x0 += sx;  // Робимо крок вбік (вліво/вправо)
            }

            // Перевіряємо, чи пора робити крок по осі Y
            if (e2 < dx)
            {
                err += dx; // Зміщуємо баланс терезів
                y0 += sy;  // Робимо крок по вертикалі (вгору/вниз)
            }
        }
    }

    private void SetPixel(int x, int y, Color32 color)
    {
        int i = y * textureWidth + x;

        pixels[i] = color;
    }

    private void Apply()
    {
        texture.SetPixels32(pixels);
        texture.Apply();
    }
}